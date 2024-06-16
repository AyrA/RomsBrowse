"use strict";

declare type NesEmuGlobal = {
    NES: NesEmu,
    Controller: NesButtonConst
};

declare type NesButtonConst = {
    BUTTON_UP: number,
    BUTTON_LEFT: number,
    BUTTON_DOWN: number,
    BUTTON_RIGHT: number,
    BUTTON_A: number,
    BUTTON_B: number,
    BUTTON_START: number,
    BUTTON_SELECT: number
};

declare class NesEmu {
    constructor(init: NesInitArg);
    frame: () => {};
    loadROM: (data: string) => {};
    buttonDown: NesInputCallback;
    buttonUp: NesInputCallback;
};

type NesInitArg = {
    onFrame: NesFrameCallback,
    onAudioSample: NesAudioSampleCallback
};

type NesInputCallback = (player: number, input: number) => {};
type NesFrameCallback = (framebuffer_24: Uint32Array) => {};
type NesAudioSampleCallback = (l: number, r: number) => {};
type NesInitFunction = (init: NesInitArg) => {};

declare global {
    interface Window {
        jsnes: NesEmuGlobal
    }
};
