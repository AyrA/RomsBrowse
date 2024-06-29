"use strict";

type SaveStateParam = {
    screenshot: Uint8Array,
    state: Uint8Array
};

namespace SaveState {
    let ramFileContents = new Uint8Array(0);
    let lastState: SaveStateParam | null;

    async function saveSRAM() {
        const id = EmulatorInterop.getGameId();
        if (!id) {
            return;
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
                console.log("Not implemented: saveSRAM");
                //TODO
            }
        }
    }

    async function loadSRAM(): Promise<boolean> {
        const id = EmulatorInterop.getGameId();
        if (!id) {
            return false;
        }
        //TODO: Get game
        console.log("Not implemented: loadSRAM");

        if (!EmulatorInterop.isEmulatorReady()) {
            return false;
        }
        if (ramFileContents.length === 0) {
            return false;
        }

        return EmulatorInterop.setSRAM(ramFileContents);
    }

    export async function saveState(state: SaveStateParam) {
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
        }
    }

    export function loadState(): Uint8Array | undefined {
        if (lastState && lastState.state) {
            EJS_emulator.gameManager.FS.writeFile("/current.state", lastState.state);
            EJS_emulator.gameManager.functions.loadState("/current.state");
        }
        return lastState?.state ?? void 0;
    }

    export async function init() {
        //TODO
        await monitorSRAM();
    }

    async function monitorSRAM() {
        await saveSRAM();
        setTimeout(monitorSRAM, 2000);
    }
}

var EJS_onGameStart = () => {
    console.log("START");
    window.EJS_emulator.on("loadState", console.log.bind(console, "EVT: loadState"));
    window.EJS_emulator.on("saveState", console.log.bind(console, "EVT: saveState"));
    window.EJS_emulator.on("loadSave", console.log.bind(console, "EVT: loadSave"));
    window.EJS_emulator.on("saveSave", console.log.bind(console, "EVT: saveSave"));
    SaveState.init();
};