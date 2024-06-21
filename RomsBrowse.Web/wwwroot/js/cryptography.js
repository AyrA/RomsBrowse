"use strict";
var Cryptography;
(function (Cryptography) {
    async function deriveGuid(salt, key, difficulty, byteCount) {
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
            hash: "SHA256",
            name: "PBKDF2",
            salt: toBuffer(salt)
        };
        var cryptoKey = await crypto.subtle.importKey("raw", toBuffer(key), params.name, false, ["deriveBits", "deriveKey"]);
        return await crypto.subtle.deriveBits(params, cryptoKey, byteCount);
    }
    Cryptography.deriveGuid = deriveGuid;
    function toBuffer(data) {
        if (typeof (data) === "string") {
            data = new TextEncoder().encode(data);
        }
        if (data instanceof ArrayBuffer) {
            return data;
        }
        return data.buffer;
    }
})(Cryptography || (Cryptography = {}));
