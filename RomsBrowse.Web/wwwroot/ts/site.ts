"use strict";

namespace WasmCheck {

    function hasWebAssembly(): boolean {
        return typeof (WebAssembly) === "object";
    }

    export function reportWebAssembly() : boolean {
        if (!hasWebAssembly()) {
            document.body.insertAdjacentHTML("beforeend", `
<dialog>
    <h1>WebAssembly is not supported</h1>
    <p>
        Due to performance constraints in JavaScript,
        this emulator requires WebAssembly to work,
        which in your browser is either not present,
        or has been disabled.<br />
        Enable WebAssembly, or use a different browser.
    </p>
    <form method="dialog"><input type="submit" value="Close" /></form>
</dialog>`);
            const dlg = document.querySelector("dialog") as HTMLDialogElement;
            dlg.addEventListener("close", () => void dlg.remove());
            dlg.showModal();
            document.querySelector("emulator-container")?.remove();
            return false;
        }
        else {
            loadEmulator();
        }
        return true;
    }

    if (document.querySelector("emulator-container")) {
        reportWebAssembly();
    }
}