// Dragging runs entirely in the browser: on Blazor Server every pointermove would otherwise be a
// round-trip over SignalR and the bar would visibly lag behind the cursor.

import { createLogger } from "/js/logging.js";
import { ofInstance, ofType } from "/js/guards.js";

const log = createLogger(import.meta.url);

/** What the component sends across with an initialize call, once every field has been checked. */
interface SplitterOptions {
  cssVariable: string;
  storageKey: string;
  minSize: number;
  maxSizeRatio: number;
  defaultSizeRatio: number;
}

const disposers = new WeakMap<HTMLElement, () => void>();

export function initialize(element: unknown, options: unknown): void {
  const handle = ofInstance(element, HTMLElement);

  // A field has to be read off the value before it can be checked, and there is no reading
  // anything off an unknown; the empty object stands in for a caller who sent nothing at all, so
  // that every field is reported missing rather than the whole call at once.
  const given = (options ?? {}) as Partial<Record<keyof SplitterOptions, unknown>>;

  const cssVariable = ofType(given.cssVariable, "string");
  const storageKey = ofType(given.storageKey, "string");
  const minSize = ofType(given.minSize, "number");
  const maxSizeRatio = ofType(given.maxSizeRatio, "number");
  const defaultSizeRatio = ofType(given.defaultSizeRatio, "number");

  if (
    handle === null ||
    cssVariable === null ||
    storageKey === null ||
    minSize === null ||
    maxSizeRatio === null ||
    defaultSizeRatio === null
  ) {
    return;
  }

  const root = document.documentElement;

  // The size the user asked for is kept apart from the size that fits: a small window renders
  // clamped, but growing the window back restores what the user chose.
  let desiredSize = 0;

  const clamp = (value: number): number =>
    Math.min(Math.max(value, minSize), window.innerHeight * maxSizeRatio);

  const render = (): void => root.style.setProperty(cssVariable, `${clamp(desiredSize)}px`);

  const renderedSize = (): number => parseFloat(getComputedStyle(root).getPropertyValue(cssVariable));

  const setSize = (value: number): void => {
    desiredSize = value;
    render();
  };

  const remember = (): void => window.localStorage.setItem(storageKey, String(desiredSize));

  const stored = parseFloat(window.localStorage.getItem(storageKey) ?? "");
  setSize(Number.isFinite(stored) ? stored : window.innerHeight * defaultSizeRatio);

  log.debug(`Splitter initialized on ${cssVariable} at ${desiredSize}px.`);

  let startPosition = 0;
  let startSize = 0;

  // The pane sits below the handle, so dragging up must make it bigger.
  const onPointerMove = (event: PointerEvent): void => setSize(startSize + (startPosition - event.clientY));

  const onPointerUp = (event: PointerEvent): void => {
    handle.releasePointerCapture(event.pointerId);
    handle.removeEventListener("pointermove", onPointerMove);
    handle.removeEventListener("pointerup", onPointerUp);
    handle.classList.remove("is-dragging");
    root.style.userSelect = "";
    remember();
  };

  const onPointerDown = (event: PointerEvent): void => {
    if (event.button !== 0) {
      return;
    }

    startPosition = event.clientY;
    startSize = renderedSize();

    handle.setPointerCapture(event.pointerId);
    handle.addEventListener("pointermove", onPointerMove);
    handle.addEventListener("pointerup", onPointerUp);
    handle.classList.add("is-dragging");

    // Without this the drag selects whatever text it sweeps across.
    root.style.userSelect = "none";
    event.preventDefault();
  };

  const onKeyDown = (event: KeyboardEvent): void => {
    const step = event.shiftKey ? 50 : 10;

    if (event.key === "ArrowUp") {
      setSize(renderedSize() + step);
    } else if (event.key === "ArrowDown") {
      setSize(renderedSize() - step);
    } else {
      return;
    }

    remember();
    event.preventDefault();
  };

  // Re-clamp so a size chosen on a large window does not swallow a small one; the chosen size
  // itself survives, so growing the window back restores it.
  handle.addEventListener("pointerdown", onPointerDown);
  handle.addEventListener("keydown", onKeyDown);
  window.addEventListener("resize", render);

  disposers.set(handle, () => {
    handle.removeEventListener("pointerdown", onPointerDown);
    handle.removeEventListener("keydown", onKeyDown);
    window.removeEventListener("resize", render);
  });
}

export function dispose(element: unknown): void {
  const handle = ofInstance(element, HTMLElement);

  if (handle === null) {
    return;
  }

  const disposer = disposers.get(handle);

  if (disposer) {
    disposer();
    disposers.delete(handle);
  }
}
