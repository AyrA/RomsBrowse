"use strict";

type SaveStateParam = {
    screenshot: Uint8Array,
    state: Uint8Array
};

namespace SaveState {
    let ramFileContents = new Uint8Array(0);
    let lastState: SaveStateParam | null;
    const pending = [] as SaveStateParam[];

    export async function upload(state: SaveStateParam) {
        lastState = state;
        pending.push(state);
        //Begin queue processing only if this is the first item in the queue
        if (pending.length === 1) {
            await processQueue();
        }
    }

    export function load(): Uint8Array | undefined {
        console.log("load", arguments);
        if (lastState && lastState.state) {
            EJS_emulator.gameManager.FS.writeFile("/current.state", lastState.state);
            EJS_emulator.gameManager.functions.loadState("/current.state");
        }
        return lastState?.state ?? void 0;
    }

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

        //Discard intermediate saves from the queue
        while (pending.length > 1) {
            pending.shift();
        }
        if (pending.length > 0) {
            await processQueue();
        }
    }

    function getGameId(): string | null {
        const match = location.pathname.match(/\/Play\/(\d+)/i);
        return match && match.length > 1 ? match[1] : null;
    }

    function isEmulatorReady(): boolean {
        if (typeof (EJS_emulator) === "undefined") {
            return false;
        }
        if (!EJS_emulator.gameManager || !EJS_emulator.gameManager.FS || !EJS_emulator.gameManager.functions) {
            return false;
        }
        return true;
    }

    async function getGameSaveData(): Promise<Uint8Array | null> {
        if (!isEmulatorReady()) {
            return null;
        }
        return await EJS_emulator.gameManager.getSaveFile();
    }

    async function loadSaveFile(data: Uint8Array) {
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
        //Uploading and downloading SRAM is not yet implemented
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

    //Restore existing state if any and begin save file tracking
    if (getGameId()) {
        getFromServer();
        setTimeout(trackSaveFile, 2000);
    }
}