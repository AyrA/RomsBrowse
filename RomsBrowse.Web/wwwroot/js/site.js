"use strict";
var WasmCheck;
(function (WasmCheck) {
    function hasWebAssembly() {
        return typeof (WebAssembly) === "object";
    }
    function reportWebAssembly() {
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
            const dlg = document.querySelector("dialog");
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
    WasmCheck.reportWebAssembly = reportWebAssembly;
    if (document.querySelector("emulator-container")) {
        reportWebAssembly();
    }
})(WasmCheck || (WasmCheck = {}));
