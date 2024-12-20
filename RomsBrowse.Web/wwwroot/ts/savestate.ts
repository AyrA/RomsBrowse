"use strict";

type SaveStateParam = {
    screenshot: Uint8Array,
    state: Uint8Array
};

namespace SaveState {
    enum StateMessageType {
        success = 1,
        warning = 2,
        error = 3
    };

    const labels = {
        SRAM: q("#tbSRAMStatus") as HTMLSpanElement,
        state: q("#tbSaveStateStatus") as HTMLSpanElement,
        upload: {
            SRAM: q("#tbLastSRAMUpload") as HTMLSpanElement,
            state: q("#tbLastStateUpload") as HTMLSpanElement
        }
    };
    const lastUpload = {
        SRAM: 0,
        state: 0
    };
    let ramFileContents = new Uint8Array(0);
    let lastState: SaveStateParam | null;

    function getStateMessageClass(messageType: StateMessageType): string {
        switch (messageType) {
            case StateMessageType.success: return "success";
            case StateMessageType.warning: return "warning";
            case StateMessageType.error: return "danger";
            default: return "info";
        }
    }

    function setSRAMState(state: string, messageType: StateMessageType) {
        setState(labels.SRAM, state, messageType);
    }

    function setSaveState(state: string, messageType: StateMessageType) {
        setState(labels.state, state, messageType);
    }

    function setState(item: HTMLElement, state: string, messageType: StateMessageType) {
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

    async function saveSRAM(): Promise<boolean> {
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
                //@ts-ignore
                ramFileContents = sram;
                const screenshot = await EmulatorInterop.getScreenshot();
                if (!screenshot || screenshot.length < 50) {
                    //If the screenshot is very small, the screen has not loaded yet
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

    async function loadSRAM(): Promise<boolean> {
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

    export async function saveState(state: SaveStateParam): Promise<boolean> {
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

    export function loadState(): Uint8Array | undefined {
        if (lastState && lastState.state) {
            EJS_emulator.gameManager.FS.writeFile("/current.state", lastState.state);
            EJS_emulator.gameManager.functions.loadState("/current.state");
            setSaveState("Reloaded", StateMessageType.success);
        }
        return lastState?.state ?? void 0;
    }

    export async function init() {
        //Don't use savestate system if not logged in
        if (!EJS_isSignedIn) {
            setSaveState("Browser only, not logged in", StateMessageType.warning);
            setSRAMState("Browser only, not logged in", StateMessageType.warning);
            return;
        }
        //If no state exists, try to restore the SRAM data
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

    async function monitorSRAM() {
        await saveSRAM();
        setTimeout(monitorSRAM, 2000);
    }
}

var EJS_onGameStart = () => {
    console.log("Game start");
    //window.EJS_emulator.on("loadSave", console.log.bind(console, "EVT: loadSave"));
    //window.EJS_emulator.on("saveSave", console.log.bind(console, "EVT: saveSave"));
    SaveState.init();
};