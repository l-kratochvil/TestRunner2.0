// The way a front-end script writes into the log file. A module asks for a logger once, naming
// itself with import.meta.url:
//
//   import { createLogger } from "/js/logging.js";
//   const log = createLogger(import.meta.url);
//
// The reference below is module state, so it only works while there is one instance of this file
// in the browser. There is: the import map rendered by App.razor keys on exactly this path and
// rewrites it to the fingerprinted URL, so every spelling of it resolves to the same module.
// Reaching the file by a path the import map does not know would load a second copy, holding a
// reference of its own that nobody ever registers.

/** Severity a diagnostic can carry, mirroring what BrowserLogger maps onto LogLevel. */
type Level = "debug" | "info" | "warn" | "error";

/** The .NET BrowserDiagnostic record, as it travels over the circuit. */
interface BrowserDiagnostic {
  level: Level;
  module: string;
  message: string;
  detail: string | null;
}

/** The DotNetObjectReference<BrowserLogger> the JsBridge component hands over. */
interface BrowserLoggerReference {
  invokeMethodAsync(identifier: "Log", diagnostic: BrowserDiagnostic): Promise<void>;
}

/** What a module logs through, once it has named itself. */
export interface Logger {
  debug(message: string, detail?: string): void;
  info(message: string, detail?: string): void;
  warn(message: string, detail?: string): void;
  error(message: string, detail?: string): void;
}

const unknownModule = "(unknown)";

// A broken timer or a render loop raises errors by the thousand, and every report is a call over
// the SignalR circuit: past some point the reporting freezes the UI harder than the bug does.
const maxCaughtErrors = 100;

let reference: BrowserLoggerReference | null = null;
let caughtErrors = 0;

const log = createLogger(import.meta.url);

/**
 * Takes the reference the .NET side is reachable through, and starts watching for the errors
 * nobody reports by hand.
 */
export function register(logger: unknown): void {
  // BrowserLoggerReference is an interface, so there is no class to hold a value against: what
  // makes one usable here is the single method every call back into .NET goes through. Checking
  // it by hand rather than through /js/guards.js keeps that module from having to grow a way to
  // describe a shape for this one caller.
  if (typeof (logger as BrowserLoggerReference | null)?.invokeMethodAsync !== "function") {
    log.warn("Ignored a register call: the argument is not a BrowserLogger reference.");
    return;
  }

  reference = logger as BrowserLoggerReference;

  // addEventListener rather than window.onerror, which is a single slot: assigning it would evict
  // whatever handler is already there, Blazor's included.
  window.addEventListener("error", onError);
  window.addEventListener("unhandledrejection", onUnhandledRejection);
}

/** Gives the reference up, before the component disposes it. */
export function unregister(): void {
  window.removeEventListener("error", onError);
  window.removeEventListener("unhandledrejection", onUnhandledRejection);

  reference = null;
}

/**
 * Builds the logger a module writes through.
 *
 * @param moduleUrl - The caller's own import.meta.url, which names it in the log. JavaScript
 * cannot see who called it, so this is the one thing the caller has to say about itself.
 */
export function createLogger(moduleUrl: string): Logger {
  const module = toModuleName(moduleUrl);
  return {
    debug: (message, detail) => send("debug", module, message, detail),
    info: (message, detail) => send("info", module, message, detail),
    warn: (message, detail) => send("warn", module, message, detail),
    error: (message, detail) => send("error", module, message, detail),
  };
}

function send(level: Level, module: string, message: string, detail?: string): void {
  // Before the bridge registers, and after it has given the reference up, there is nowhere to send
  // to. The console is the first place anyone looks at a script anyway, so nothing is lost quietly.
  if (reference === null) {
    console[level](`[${module}] ${message}`, detail ?? "");

    return;
  }

  // Fire and forget, but never unhandled: a rejection here would surface as unhandledrejection,
  // which onUnhandledRejection would try to report through this very function. Catching it closes
  // that loop at its source, and the failure goes to the console for the same reason.
  reference
    .invokeMethodAsync("Log", { level, module, message, detail: detail ?? null })
    .catch((error: unknown) => console.error(`[${module}] Failed to log to the server.`, error));
}

function onError(event: ErrorEvent): void {
  const detail = event.error instanceof Error ? event.error.stack : undefined;

  report(toModuleName(event.filename), event.message, detail);
}

function onUnhandledRejection(event: PromiseRejectionEvent): void {
  const reason: unknown = event.reason;

  // A rejection carries no location of its own, so unlike onError there is no file name to take.
  if (reason instanceof Error) {
    report(unknownModule, `${reason.name}: ${reason.message}`, reason.stack);
    return;
  }

  report(unknownModule, `Unhandled promise rejection: ${String(reason)}`);
}

/**
 * Reports an error that arrived on its own, up to a limit. Only these are counted: an explicit
 * call is written by hand and stops when its author stops it.
 */
function report(module: string, message: string, detail?: string): void {
  if (caughtErrors >= maxCaughtErrors) {
    return;
  }

  caughtErrors += 1;
  send("error", module, message, detail);

  // The count only ever grows: reporting resumes on a reload, which is also what replaces the dead
  // page a storm like this usually leaves behind.
  if (caughtErrors === maxCaughtErrors) {
    log.warn(
      `Stopped reporting uncaught browser errors after ${maxCaughtErrors} of them. ` +
        "Reload the page to start again.",
    );
  }
}

function toModuleName(url: string): string {
  if (!url) {
    return unknownModule;
  }

  try {
    const path = new URL(url, document.baseURI).pathname.replace(/^\//, "");

    // The import map rewrites module paths to fingerprinted ones, so the URL a module knows itself
    // by carries a hash of its content: left in, a script would change its name in the log every
    // time it is edited. The .razor part is optional because a shared module has none.
    return path.replace(/(\.[a-z0-9]+)?(\.razor)?\.js$/, "") || unknownModule;
  } catch {
    // Not every location a browser reports parses as a URL, and the raw value still says more than
    // "(unknown)" would.
    return url;
  }
}
