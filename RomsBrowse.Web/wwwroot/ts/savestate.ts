"use strict";

type SaveStateParam = {
    screenshot: Uint8Array,
    state: Uint8Array
};

namespace SaveState {
    let ramFileContents = new Uint8Array(0);
    let lastState: SaveStateParam | null;

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
                ramFileContents = sram;
                const fd = new FormData();
                fd.addCsrf();
                fd.set("GameId", id);
                fd.set("SaveState", new Blob([ramFileContents]), "save.bin");
                const result = await fetch("/Rom/SaveSRAM", { method: "POST", body: fd });
                if (!result.ok) {
                    console.warn("Failed to save SRAM to server. Status was", result.status, result.statusText);
                    console.warn(await result.text());
                    return false;
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
            return false;
        }
        if (ramFileContents.length === 0) {
            const response = await fetch(`/Rom/GetSRAM/${id}`);
            if (response.ok) {
                ramFileContents = new Uint8Array(await response.arrayBuffer());
            }
            else {
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
        lastState = state;
        const fd = new FormData();
        fd.addCsrf();
        fd.set("GameId", id);
        fd.set("Screenshot", new Blob([state.screenshot]), "image.png");
        fd.set("SaveState", new Blob([state.state]), "save.bin");
        const result = await fetch("/Rom/SaveState", { method: "POST", body: fd });
        if (!result.ok) {
            console.warn("Failed to save state to server. Status was", result.status, result.statusText);
            console.warn(await result.text());
            return false;
        }
        return true;
    }

    export function loadState(): Uint8Array | undefined {
        if (lastState && lastState.state) {
            EJS_emulator.gameManager.FS.writeFile("/current.state", lastState.state);
            EJS_emulator.gameManager.functions.loadState("/current.state");
        }
        return lastState?.state ?? void 0;
    }

    export async function init() {
        //Don't use savestate system if not logged in
        if (!EJS_isSignedIn) {
            return;
        }
        //If no state exists, try to restore the SRAM data
        if (!EJS_loadStateURL) {
            console.log("No saved state found. Checking for SRAM instead.");
            if (await loadSRAM()) {
                console.log("SRAM restored from server");
            }
            else {
                console.log("No SRAM on server. Game started without save data");
            }
        }
        await monitorSRAM();
        window.EJS_emulator.on("loadState", loadState);
        window.EJS_emulator.on("saveState", saveState);
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