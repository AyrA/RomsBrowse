/** Loads EmulatorJS emulator engine */
declare function loadEmulator(): void;

declare var EJS_emulator: Emulator;
declare var EJS_gameName: string;

type Emulator = {
    gameManager: EmulatorGameManager;
};

type EmulatorGameManager = {
    FS: EmulatorFileSystem;
    functions: EmulatorGameFunctions;
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
};