"use strict";
var Account;
(function (Account) {
    const difficulty = 1_000_000;
    async function getAccountId(username, password) {
        if (!ratePassword(password).isSafe) {
            throw new Error("Password is not safe");
        }
        const bytes = await Cryptography.deriveBytes(username, password, difficulty, 16);
        const id = Guid.parse(bytes);
        console.log(id);
    }
    Account.getAccountId = getAccountId;
    async function signIn(id) {
        if (!Guid.test(id) || id === Guid.empty) {
            throw new Error("Invalid id");
        }
        const fd = CSRF.enrich(new FormData());
        fd.set("Id", id);
        const result = await fetch("/Account/Login", { method: "POST", body: fd });
        return result.ok;
    }
    Account.signIn = signIn;
    async function signOut() {
        const fd = CSRF.enrich(new FormData());
        const result = await fetch("/Account/Logout", { method: "POST", body: fd });
        return result.ok;
    }
    Account.signOut = signOut;
    function ratePassword(password) {
        password = password || "";
        const ret = {};
        ret.hasUppercase = /[A-Z]/.test(password);
        ret.hasLowercase = /[a-z]/.test(password);
        ret.hasDigit = /\d/.test(password);
        ret.hasSymbol = /[^\da-zA-Z]/.test(password);
        ret.length = password.length;
        ret.minLength = 8;
        ret.score = (ret.hasUppercase ? 1 : 0) +
            (ret.hasLowercase ? 1 : 0) +
            (ret.hasDigit ? 1 : 0) +
            (ret.hasSymbol ? 1 : 0);
        ret.maxScore = 4;
        ret.minScore = 3;
        ret.isSafe = ret.length >= ret.minLength && ret.score >= ret.minScore;
        return ret;
    }
    Account.ratePassword = ratePassword;
})(Account || (Account = {}));
var Cryptography;
(function (Cryptography) {
    async function deriveBytes(salt, key, difficulty, byteCount) {
        if (difficulty < 1_000) {
            throw new RangeError(`difficulty must be at least 1000, but is ${difficulty}`);
        }
        if (byteCount < 1) {
            throw new RangeError(`byteCount must be at least 1, but is ${byteCount}`);
        }
        if (!salt || !key) {
            throw new Error("Salt and key cannot be empty");
        }
        const params = {
            iterations: difficulty,
            hash: "SHA-256",
            name: "PBKDF2",
            salt: toBuffer(salt)
        };
        var cryptoKey = await crypto.subtle.importKey("raw", toBuffer(key), params.name, false, ["deriveBits", "deriveKey"]);
        return await crypto.subtle.deriveBits(params, cryptoKey, byteCount * 8);
    }
    Cryptography.deriveBytes = deriveBytes;
    function getRandom(count) {
        if (count < 1) {
            throw new RangeError("Byte count must be at least 1");
        }
        const ret = new Uint8Array(count);
        crypto.getRandomValues(ret);
        return ret;
    }
    Cryptography.getRandom = getRandom;
    function toBuffer(data) {
        if (typeof (data) === "string") {
            return new TextEncoder().encode(data).buffer;
        }
        if (data instanceof ArrayBuffer) {
            return data;
        }
        return data.buffer;
    }
    Cryptography.toBuffer = toBuffer;
})(Cryptography || (Cryptography = {}));
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
var Guid;
(function (Guid) {
    Guid.empty = "00000000-0000-0000-0000-000000000000";
    function parse(data) {
        if (!data) {
            throw new Error("Data not supplied");
        }
        const raw = new Uint8Array(Cryptography.toBuffer(data));
        if (raw.length !== 16) {
            throw new RangeError(`Data must consist of exactly 16 bytes, but ${raw.length} was present`);
        }
        const nibbles = [];
        raw.forEach(value => { nibbles.push(value >> 4); nibbles.push(value & 0xF); });
        return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[4xy]/g, function (e) {
            var t = nibbles.shift() | 0;
            let ret = 0;
            switch (e) {
                case 'x':
                    ret = t;
                    break;
                case 'y':
                    ret = 3 & t | 8;
                    break;
                case '4':
                    ret = 4;
                    break;
            }
            return ret.toString(16);
        });
    }
    Guid.parse = parse;
    function generate() {
        return parse(Cryptography.getRandom(16));
    }
    Guid.generate = generate;
    function test(guid) {
        if (typeof (guid) !== "string" || guid.length === 0) {
            return false;
        }
        if (guid === Guid.empty) {
            return true;
        }
        return /^[\da-f]{8}-[\da-f]{4}-[\da-f]{4}-[\da-f]{4}-[\da-f]{12}$/i.test(guid);
    }
    Guid.test = test;
    function toRaw(guid) {
        if (!Guid.test(guid)) {
            throw new Error("Invalid Guid value");
        }
        return new Uint8Array(Array.from(guid.match(/[a-f\d]{2}/gi) || []).map(v => parseInt(v, 16)));
    }
    Guid.toRaw = toRaw;
})(Guid || (Guid = {}));
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
