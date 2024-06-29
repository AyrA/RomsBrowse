"use strict";

namespace EmulatorInterop {
    export function getSRAM(): Promise<Uint8Array | null> {
        return EJS_emulator.gameManager.getSaveFile();
    }

    export function setSRAM(ramFileContents: Uint8Array): boolean {
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

    export function reset() {
        EJS_emulator.gameManager.reset()
    }

    export function getGameId(): string | null {
        const match = location.pathname.match(/\/Play\/(\d+)/i);
        return match && match.length > 1 ? match[1] : null;
    }

    export function isEmulatorReady(): boolean {
        if (typeof (EJS_emulator) === "undefined") {
            return false;
        }
        if (!EJS_emulator.gameManager || !EJS_emulator.gameManager.FS || !EJS_emulator.gameManager.functions) {
            return false;
        }
        return true;
    }

    export function waitForEmulatorReady(): Promise<void> {
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
            }
            check();
        });
    }

    export async function startEmulator() {
        await loadEmulator();
    }
}
