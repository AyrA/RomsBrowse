/** Loads EmulatorJS emulator engine */
declare function loadEmulator(): Promise<void>;
declare function setEmulatorInitValues(): void;

declare var EJS_emulator: Emulator;
declare var EJS_gameName: string;
declare var EJS_loadStateURL: string | null;
declare var EJS_onGameStart: () => void;

type Emulator = {
    gameManager: EmulatorGameManager;
    callEvent: (name: string) => number;
    on: (event: string, handler: Function) => void;
};

type EmulatorGameManager = {
    FS: EmulatorFileSystem;
    functions: EmulatorGameFunctions;
    reset: () => void;
    getSaveFile: () => Promise<Uint8Array | null>;
    getSaveFilePath: () => string | null;
    loadSaveFiles: () => void;
};

type EmulatorGameFunctions = {
    loadState: (path: string) => number;
};

type EmulatorPathDetails = {
    error: number;
    exists: boolean;
    isRoot: boolean;
    name: string;
    object: EmulatorFsNode | null;
    parentExists: boolean;
    parentObject?: EmulatorFsNode | null;
    parentPath: string;
    path: string | null;
};

type EmulatorFsNode = {
    contents: Uint8Array;
    id: number;
    mode: number;
    name: string;
    name_next?: string;
    parent: EmulatorFsNode | null;
    rdev: number;
    timestamp: number;
    usedBytes: number;
    isDevice: boolean;
    isFolder: boolean;
    read: boolean;
    write: boolean;
};

type EmulatorFileSystem = {
    readFile: (path: string) => Uint8Array;
    writeFile: (path: string, data: Uint8Array) => void;
    readdir: (path: string) => string[];
    unlink: (path: string) => number;
    analyzePath: (path: string) => EmulatorPathDetails;
    mkdir: (path: string) => void;
    /**
     * Creates all missing directories in the given path string
     * @param path Path
     * 
     * Will do nothing if the entire path already exists.
     * Path string must not contain file name,
     * or it creates the file name as a directory
     */
    mkdirTree: (path: string) => void;
};