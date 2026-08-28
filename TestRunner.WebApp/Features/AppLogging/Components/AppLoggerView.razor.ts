import { ofInstance } from "/js/guards.js";

export function scrollToEnd(element: unknown): void {
    // A component can ask for this before its element is rendered, so nothing to scroll is an
    // ordinary state rather than a broken call, and passes without a word. Anything else that is
    // not an element is the caller's mistake, and the guard reports it.
    if (element === null || element === undefined) {
        return;
    }

    const target = ofInstance(element, HTMLElement);

    if (target === null) {
        return;
    }

    target.scrollTop = target.scrollHeight;
}
