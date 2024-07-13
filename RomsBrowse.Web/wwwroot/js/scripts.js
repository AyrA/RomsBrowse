"use strict";
var DCT;
(function (DCT) {
    const handlers = {};
    let id = 0;
    function nextId() {
        return `TH-${++id}`;
    }
    function setTimer(handler) {
        const id = nextId();
        handlers[id] = handler;
        return id;
    }
    DCT.setTimer = setTimer;
    function removeTimer(handler) {
        if (typeof (handler) === "string") {
            return delete handlers[handler];
        }
        else {
            let ret = false;
            const keys = Object.keys(handlers);
            for (let key of keys) {
                if (handlers[key] === handler) {
                    ret = ret || removeTimer(key);
                }
            }
            return ret;
        }
    }
    DCT.removeTimer = removeTimer;
    function tick() {
        const keys = Object.keys(handlers);
        for (let k of keys) {
            try {
                handlers[k]();
            }
            catch (e) {
                console.error("DCT handler crached:", k);
                console.error(e);
            }
        }
        setTimeout(tick, 1000 - (Date.now() % 1000));
    }
    tick();
})(DCT || (DCT = {}));
var EmulatorInterop;
(function (EmulatorInterop) {
    function getScreenshot() {
        return EJS_emulator.gameManager.screenshot();
    }
    EmulatorInterop.getScreenshot = getScreenshot;
    function getSRAM() {
        return EJS_emulator.gameManager.getSaveFile();
    }
    EmulatorInterop.getSRAM = getSRAM;
    function setSRAM(ramFileContents) {
        const evtResult = EJS_emulator.callEvent("loadSave");
        if (!(0 < evtResult)) {
            const savePath = EJS_emulator.gameManager.getSaveFilePath();
            if (!savePath) {
                return false;
            }
            const saveParts = savePath.split("/");
            saveParts.pop();
            EJS_emulator.gameManager.FS.mkdirTree(saveParts.join(""));
            if (EJS_emulator.gameManager.FS.analyzePath(savePath).exists) {
                EJS_emulator.gameManager.FS.unlink(savePath);
            }
            EJS_emulator.gameManager.FS.writeFile(savePath, ramFileContents);
            EJS_emulator.gameManager.loadSaveFiles();
            return true;
        }
        return false;
    }
    EmulatorInterop.setSRAM = setSRAM;
    function reset() {
        EJS_emulator.gameManager.reset();
    }
    EmulatorInterop.reset = reset;
    function getGameId() {
        const match = location.pathname.match(/\/Play\/(\d+)/i);
        return match && match.length > 1 ? match[1] : null;
    }
    EmulatorInterop.getGameId = getGameId;
    function isEmulatorReady() {
        if (typeof (EJS_emulator) === "undefined") {
            return false;
        }
        if (!EJS_emulator.gameManager || !EJS_emulator.gameManager.FS || !EJS_emulator.gameManager.functions) {
            return false;
        }
        return true;
    }
    EmulatorInterop.isEmulatorReady = isEmulatorReady;
    function waitForEmulatorReady() {
        if (isEmulatorReady()) {
            return Promise.resolve();
        }
        return new Promise((accept, reject) => {
            const check = function () {
                if (isEmulatorReady()) {
                    accept();
                }
                else {
                    setTimeout(check, 100);
                }
            };
            check();
        });
    }
    EmulatorInterop.waitForEmulatorReady = waitForEmulatorReady;
    async function startEmulator() {
        await loadEmulator();
    }
    EmulatorInterop.startEmulator = startEmulator;
})(EmulatorInterop || (EmulatorInterop = {}));
var WasmCheck;
(function (WasmCheck) {
    function hasWebAssembly() {
        return typeof (WebAssembly) === "object";
    }
    WasmCheck.hasWebAssembly = hasWebAssembly;
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
            const dlg = q("dialog");
            dlg.addEventListener("close", () => void dlg.remove());
            dlg.showModal();
            q("emulator-container")?.remove();
            return false;
        }
        return true;
    }
    WasmCheck.reportWebAssembly = reportWebAssembly;
})(WasmCheck || (WasmCheck = {}));
FormData.prototype.addCsrf = function () {
    CSRF.enrich(this);
    return this;
};
class CSRF {
    static #token = JSON.parse(q("[data-token]").dataset.token);
    static get name() {
        return CSRF.#token.formFieldName;
    }
    static get value() {
        return CSRF.#token.requestToken;
    }
    static enrich(fd) {
        fd.set(CSRF.name, CSRF.value);
        return fd;
    }
}
var DirBrowse;
(function (DirBrowse) {
    const emoji = {
        folder: "\uD83D\uDCC1",
        check: "\uD83D\uDD79"
    };
    async function browse(browseItem) {
        if (!(browseItem instanceof HTMLElement)) {
            throw new TypeError("Argument not an Element");
        }
        const target = browseItem.dataset.target;
        if (!target) {
            throw new Error("Target not defined");
        }
        const ie = document.querySelector(target);
        if (!ie) {
            throw new Error(`Target item '${target}' does not exist`);
        }
        if (!(ie instanceof HTMLInputElement)) {
            throw new Error(`Target item '${target}' is not an INPUT element`);
        }
        getAndRender(ie.value, target);
    }
    DirBrowse.browse = browse;
    function selectFolder(target, folder) {
        const e = document.querySelector(target);
        const dlg = document.querySelector("#dirBrowse");
        if (e instanceof HTMLInputElement) {
            e.value = folder;
            dlg.close();
            dlg.remove();
        }
    }
    async function getAndRender(folder, target) {
        const folders = await getFolder(folder);
        if (folders) {
            renderFolder(folders, target);
        }
    }
    function nav(e) {
        e.preventDefault();
        const dir = this.dataset.folder;
        const mode = this.dataset.mode;
        const target = this.dataset.target;
        if (mode === "select") {
            selectFolder(target, dir);
        }
        else if (mode === "browse") {
            getAndRender(dir, target);
        }
        else {
            throw new Error("Unknown mode: " + mode);
        }
    }
    function renderFolder(result, target) {
        const dlg = (document.querySelector("dialog#dirBrowse") || document.createElement("dialog"));
        dlg.id = "dirBrowse";
        if (!dlg.open) {
            document.body.appendChild(dlg);
            dlg.showModal();
        }
        dlg.innerHTML = "";
        dlg.appendChild(document.createElement("h1")).textContent = "Select folder";
        if (result.currentFolder) {
            result.folders.unshift({ canBeSelected: false, fullPath: result.parentFolder ?? "", name: "<up>" });
        }
        for (let item of result.folders) {
            const entry = dlg.appendChild(document.createElement("a"));
            entry.href = "#";
            entry.classList.add("d-block", "mb-2");
            entry.textContent = (item.canBeSelected ? emoji.check : emoji.folder) + item.name;
            entry.dataset.folder = item.fullPath;
            entry.dataset.mode = item.canBeSelected ? "select" : "browse";
            entry.dataset.target = target;
            entry.addEventListener("click", nav);
        }
        const form = dlg.appendChild(document.createElement("form"));
        form.method = "dialog";
        const btn = form.appendChild(document.createElement("input"));
        btn.classList.add("btn", "btn-primary");
        btn.value = "Close";
        btn.type = "submit";
    }
    async function getFolder(parent) {
        const fd = new FormData();
        fd.addCsrf();
        fd.set("Folder", parent ?? "");
        const result = await fetch("/Admin/Folder", { method: "POST", body: fd });
        if (result.ok) {
            const contents = (await result.json());
            return contents;
        }
        else {
            const error = await result.text();
            alert(error.substring(0, 200).replace(/\s+/g, ' '));
            return null;
        }
    }
    document.addEventListener("click", (e) => {
        if (e.target instanceof HTMLElement) {
            if (e.target.dataset.action === "browse") {
                e.preventDefault();
                browse(e.target);
            }
        }
    });
})(DirBrowse || (DirBrowse = {}));
var Init;
(function (Init) {
    const form1 = q("#frmInitSql");
    const form2 = q("#frmInitSqlite");
    const btnTest1 = q("#btnInitTest1");
    const btnTest2 = q("#btnInitTest2");
    const rbSec1 = q("#showsec1");
    const rbSec2 = q("#showsec2");
    async function init(e, form, btnTest) {
        e.preventDefault();
        btnTest.disabled = true;
        btnTest.value = "Testing...";
        try {
            if (form.reportValidity()) {
                const fd = new FormData(form);
                const response = await fetch("/Init/Test", { method: "POST", body: fd });
                if (response.ok) {
                    const result = (await response.json());
                    console.log(result);
                    if (result.success) {
                        alert("Data is valid");
                        form.submit();
                    }
                    else {
                        alert(result.error || "Unspecified error");
                    }
                }
                else {
                    alert("Unspecified error");
                }
            }
        }
        finally {
            btnTest.value = "Test";
            btnTest.disabled = false;
        }
    }
    function handleChange() {
        const items = {
            a: q("section#sec1"),
            b: q("section#sec2")
        };
        if (rbSec2.checked) {
            const c = items.a;
            items.a = items.b;
            items.b = c;
        }
        else if (!rbSec1.checked) {
            items.a.classList.add("d-none");
            items.a.classList.remove("d-block");
            items.b.classList.add("d-none");
            items.b.classList.remove("d-block");
            return;
        }
        items.a.classList.add("d-block");
        items.a.classList.remove("d-none");
        items.b.classList.add("d-none");
        items.b.classList.remove("d-block");
    }
    if (btnTest1 && btnTest2 && rbSec1 && rbSec2) {
        btnTest1.addEventListener("click", (e) => init(e, form1, btnTest1));
        btnTest2.addEventListener("click", (e) => init(e, form2, btnTest2));
        rbSec1.addEventListener("change", handleChange);
        rbSec2.addEventListener("change", handleChange);
        handleChange();
    }
})(Init || (Init = {}));
var SaveState;
(function (SaveState) {
    let StateMessageType;
    (function (StateMessageType) {
        StateMessageType[StateMessageType["success"] = 1] = "success";
        StateMessageType[StateMessageType["warning"] = 2] = "warning";
        StateMessageType[StateMessageType["error"] = 3] = "error";
    })(StateMessageType || (StateMessageType = {}));
    ;
    const labels = {
        SRAM: q("#tbSRAMStatus"),
        state: q("#tbSaveStateStatus"),
        upload: {
            SRAM: q("#tbLastSRAMUpload"),
            state: q("#tbLastStateUpload")
        }
    };
    const lastUpload = {
        SRAM: 0,
        state: 0
    };
    let ramFileContents = new Uint8Array(0);
    let lastState;
    function getStateMessageClass(messageType) {
        switch (messageType) {
            case StateMessageType.success: return "success";
            case StateMessageType.warning: return "warning";
            case StateMessageType.error: return "danger";
            default: return "info";
        }
    }
    function setSRAMState(state, messageType) {
        setState(labels.SRAM, state, messageType);
    }
    function setSaveState(state, messageType) {
        setState(labels.state, state, messageType);
    }
    function setState(item, state, messageType) {
        item.textContent = state;
        item.classList.remove("alert-danger", "alert-warning", "alert-success");
        item.classList.add(`alert-${getStateMessageClass(messageType)}`);
    }
    function refreshUploadTimer() {
        const now = Date.now();
        const lastSRAM = ((now - lastUpload.SRAM) / 1000) | 0;
        const lastState = ((now - lastUpload.state) / 1000) | 0;
        if (lastUpload.SRAM > 0) {
            labels.upload.SRAM.textContent = `${lastSRAM} seconds ago`;
        }
        else {
            labels.upload.SRAM.textContent = "never uploaded";
        }
        if (lastUpload.state > 0) {
            labels.upload.state.textContent = `${lastState} seconds ago`;
        }
        else {
            labels.upload.state.textContent = "never uploaded";
        }
    }
    async function saveSRAM() {
        const id = EmulatorInterop.getGameId();
        if (!id) {
            return false;
        }
        const sram = await EmulatorInterop.getSRAM();
        if (sram && sram.length > 0) {
            let changed = false;
            if (sram.length != ramFileContents.length) {
                changed = true;
            }
            else {
                for (let i = 0; i < sram.length; i++) {
                    if (sram[i] !== ramFileContents[i]) {
                        changed = true;
                        break;
                    }
                }
            }
            if (changed) {
                ramFileContents = sram;
                const screenshot = await EmulatorInterop.getScreenshot();
                if (!screenshot || screenshot.length < 50) {
                    console.log("Emulator has no valid screenshot");
                    return false;
                }
                setSRAMState("Uploading...", StateMessageType.warning);
                const fd = new FormData();
                fd.addCsrf();
                fd.set("GameId", id);
                fd.set("SaveState", new Blob([ramFileContents]), "save.bin");
                fd.set("Screenshot", new Blob([screenshot]), "image.png");
                const result = await fetch("/Rom/SaveSRAM", { method: "POST", body: fd });
                if (!result.ok) {
                    setSRAMState("Failed to upload", StateMessageType.error);
                    console.warn("Failed to save SRAM to server. Status was", result.status, result.statusText);
                    console.warn(await result.text());
                    return false;
                }
                else {
                    lastUpload.SRAM = Date.now();
                    setSRAMState("Saved to server", StateMessageType.success);
                }
            }
            return changed;
        }
        return false;
    }
    async function loadSRAM() {
        const id = EmulatorInterop.getGameId();
        if (!id) {
            return false;
        }
        if (!EmulatorInterop.isEmulatorReady()) {
            setSRAMState("Emulator not ready", StateMessageType.warning);
            return false;
        }
        if (ramFileContents.length === 0) {
            const response = await fetch(`/Rom/GetSRAM/${id}`);
            if (response.ok) {
                setSRAMState("Loaded from server", StateMessageType.success);
                ramFileContents = new Uint8Array(await response.arrayBuffer());
            }
            else {
                setSRAMState("No SRAM on server", StateMessageType.success);
                return false;
            }
        }
        return EmulatorInterop.setSRAM(ramFileContents);
    }
    async function saveState(state) {
        const id = EmulatorInterop.getGameId();
        if (!id) {
            throw new Error("Game id cannot be obtained. Not a game URL?");
        }
        setSaveState("Uploading...", StateMessageType.warning);
        lastState = state;
        const fd = new FormData();
        fd.addCsrf();
        fd.set("GameId", id);
        fd.set("Screenshot", new Blob([state.screenshot]), "image.png");
        fd.set("SaveState", new Blob([state.state]), "save.bin");
        const result = await fetch("/Rom/SaveState", { method: "POST", body: fd });
        if (!result.ok) {
            setSaveState("Failed to upload", StateMessageType.error);
            console.warn("Failed to save state to server. Status was", result.status, result.statusText);
            console.warn(await result.text());
            return false;
        }
        else {
            lastUpload.state = Date.now();
            setSaveState("Saved to server", StateMessageType.success);
        }
        return true;
    }
    SaveState.saveState = saveState;
    function loadState() {
        if (lastState && lastState.state) {
            EJS_emulator.gameManager.FS.writeFile("/current.state", lastState.state);
            EJS_emulator.gameManager.functions.loadState("/current.state");
            setSaveState("Reloaded", StateMessageType.success);
        }
        return lastState?.state ?? void 0;
    }
    SaveState.loadState = loadState;
    async function init() {
        if (!EJS_isSignedIn) {
            setSaveState("Browser only, not logged in", StateMessageType.warning);
            setSRAMState("Browser only, not logged in", StateMessageType.warning);
            return;
        }
        if (!EJS_loadStateURL) {
            setSaveState("None found", StateMessageType.success);
            console.log("No saved state found. Checking for SRAM instead.");
            if (await loadSRAM()) {
                setSRAMState("Loaded from server", StateMessageType.success);
                console.log("SRAM restored from server");
            }
            else {
                setSRAMState("None found", StateMessageType.success);
                console.log("No SRAM on server. Game started without save data");
            }
        }
        else {
            setSaveState("Restored from server", StateMessageType.success);
        }
        await monitorSRAM();
        window.EJS_emulator.on("loadState", loadState);
        window.EJS_emulator.on("saveState", saveState);
        DCT.setTimer(refreshUploadTimer);
    }
    SaveState.init = init;
    async function monitorSRAM() {
        await saveSRAM();
        setTimeout(monitorSRAM, 2000);
    }
})(SaveState || (SaveState = {}));
var EJS_onGameStart = () => {
    console.log("Game start");
    SaveState.init();
};
function q(x) { return document.querySelector(x); }
function qa(x) { return document.querySelectorAll(x); }
var Timer;
(function (Timer) {
    const timerElement = q("[data-auto-reload]");
    const fallback = 10;
    if (timerElement) {
        let timer = parseInt(timerElement.dataset.autoReload);
        if (Number.isNaN(timer)) {
            console.error("Element", timerElement, "has invalid timer value:", timerElement.dataset.autoReload);
            throw new Error("Element has invalid timer value: " + timerElement.dataset.autoReload);
        }
        if (timer <= 0) {
            console.warn(`Element timer value ${timer} is out of range. Setting to ${fallback}`);
            timer = fallback;
        }
        const timerId = setInterval(() => {
            --timer;
            if (timer >= 0) {
                timerElement.textContent = timer.toString();
            }
            if (timer <= 0) {
                clearInterval(timerId);
                location.reload();
            }
        }, 1000);
    }
})(Timer || (Timer = {}));
