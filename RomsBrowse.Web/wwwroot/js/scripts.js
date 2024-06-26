"use strict";
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
        const result = await fetch("/Settings/Folder", { method: "POST", body: fd });
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
var SaveState;
(function (SaveState) {
    let ramFileContents = new Uint8Array(0);
    let lastState;
    const pending = [];
    async function upload(state) {
        lastState = state;
        pending.push(state);
        if (pending.length === 1) {
            await processQueue();
        }
    }
    SaveState.upload = upload;
    function load() {
        console.log("load", arguments);
        if (lastState && lastState.state) {
            EJS_emulator.gameManager.FS.writeFile("/current.state", lastState.state);
            EJS_emulator.gameManager.functions.loadState("/current.state");
        }
        return lastState?.state ?? void 0;
    }
    SaveState.load = load;
    async function getFromServer() {
        const id = getGameId();
        if (!id) {
            throw new Error("Game id cannot be obtained. Not a game URL?");
        }
        const fd = new FormData();
        fd.addCsrf();
        fd.set("GameId", id);
        const result = await fetch("/Rom/GetState", { method: "POST", body: fd });
        if (result.ok) {
            lastState = {
                state: new Uint8Array(await result.arrayBuffer()),
                screenshot: new Uint8Array(0)
            };
        }
        else if (result.status === 404) {
            console.log("Game has no saved state. starting from blank");
        }
    }
    async function processQueue() {
        const id = getGameId();
        if (!id) {
            throw new Error("Game id cannot be obtained. Not a game URL?");
        }
        const state = pending[0];
        if (!state) {
            return;
        }
        const fd = new FormData();
        fd.addCsrf();
        fd.set("GameId", id);
        fd.set("Screenshot", new Blob([state.screenshot]), "image.png");
        fd.set("SaveState", new Blob([state.state]), "save.bin");
        const result = await fetch("/Rom/SaveState", { method: "POST", body: fd });
        if (!result.ok) {
            console.warn("Failed to save state to server. Status was", result.status, result.statusText);
            console.warn(await result.text());
        }
        else {
            console.log("Ok");
        }
        pending.shift();
        while (pending.length > 1) {
            pending.shift();
        }
        if (pending.length > 0) {
            await processQueue();
        }
    }
    function getGameId() {
        const match = location.pathname.match(/\/Play\/(\d+)/i);
        return match && match.length > 1 ? match[1] : null;
    }
    function isEmulatorReady() {
        if (typeof (EJS_emulator) === "undefined") {
            return false;
        }
        if (!EJS_emulator.gameManager || !EJS_emulator.gameManager.FS || !EJS_emulator.gameManager.functions) {
            return false;
        }
        return true;
    }
    async function getGameSaveData() {
        if (!isEmulatorReady()) {
            return null;
        }
        return await EJS_emulator.gameManager.getSaveFile();
    }
    async function loadSaveFile(data) {
        if (!isEmulatorReady()) {
            return false;
        }
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
            EJS_emulator.gameManager.FS.writeFile(savePath, data);
            EJS_emulator.gameManager.loadSaveFiles();
        }
        return true;
    }
    async function uploadRamFile() {
    }
    async function trackSaveFile() {
        const newData = await getGameSaveData();
        if (newData) {
            let hasNewData = false;
            if (newData.length !== ramFileContents.length) {
                hasNewData = true;
                console.log("SRAM copied");
            }
            else {
                for (let i = 0; i < newData.length; i++) {
                    if (newData[i] !== ramFileContents[i]) {
                        hasNewData = true;
                        console.log("SRAM changed at offset", i);
                        break;
                    }
                }
            }
            if (hasNewData) {
                ramFileContents = newData;
                await uploadRamFile();
            }
        }
        setTimeout(trackSaveFile, 2000);
    }
    if (getGameId()) {
        getFromServer();
        setTimeout(trackSaveFile, 2000);
    }
})(SaveState || (SaveState = {}));
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
            const dlg = q("dialog");
            dlg.addEventListener("close", () => void dlg.remove());
            dlg.showModal();
            q("emulator-container")?.remove();
            return false;
        }
        else {
            loadEmulator();
        }
        return true;
    }
    WasmCheck.reportWebAssembly = reportWebAssembly;
    if (q("emulator-container")) {
        reportWebAssembly();
    }
})(WasmCheck || (WasmCheck = {}));
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
