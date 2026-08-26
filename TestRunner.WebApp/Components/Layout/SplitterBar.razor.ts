// Dragging runs entirely in the browser: on Blazor Server every pointermove would otherwise be a
// round-trip over SignalR and the bar would visibly lag behind the cursor.

interface SplitterOptions {
  cssVariable: string;
  storageKey: string;
  minSize: number;
  maxSizeRatio: number;
  defaultSizeRatio: number;
}

const disposers = new WeakMap<HTMLElement, () => void>();

export function initialize(handle: HTMLElement, options: SplitterOptions): void {
  const root = document.documentElement;

  // The size the user asked for is kept apart from the size that fits: a small window renders
  // clamped, but growing the window back restores what the user chose.
  let desiredSize = 0;

  const clamp = (value: number): number =>
    Math.min(Math.max(value, options.minSize), window.innerHeight * options.maxSizeRatio);

  const render = (): void => root.style.setProperty(options.cssVariable, `${clamp(desiredSize)}px`);

  const renderedSize = (): number => parseFloat(getComputedStyle(root).getPropertyValue(options.cssVariable));

  const setSize = (value: number): void => {
    desiredSize = value;
    render();
  };

  const remember = (): void => window.localStorage.setItem(options.storageKey, String(desiredSize));

  const stored = parseFloat(window.localStorage.getItem(options.storageKey) ?? "");
  setSize(Number.isFinite(stored) ? stored : window.innerHeight * options.defaultSizeRatio);

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

export function dispose(handle: HTMLElement): void {
  const disposer = disposers.get(handle);

  if (disposer) {
    disposer();
    disposers.delete(handle);
  }
}
