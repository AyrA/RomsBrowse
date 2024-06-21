namespace Cryptography {
    export async function deriveGuid(salt: string | BufferSource, key: string | BufferSource,
        difficulty: number, byteCount: number): Promise<string> {

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
        } as Pbkdf2Params;
        var cryptoKey = await crypto.subtle.importKey("raw", toBuffer(key), params.name, false, ["deriveBits", "deriveKey"]);
        return Guid.parse(await crypto.subtle.deriveBits(params, cryptoKey, byteCount));
    }

    export function getRandom(count: number) {
        if (count < 1) {
            throw new RangeError("Byte count must be at least 1");
        }
        const ret = new Uint8Array(count);
        crypto.getRandomValues(ret);
        return ret;
    }

    export function toBuffer(data: string | BufferSource): ArrayBuffer {
        if (typeof (data) === "string") {
            data = new TextEncoder().encode(data);
        }
        if (data instanceof ArrayBuffer) {
            return data;
        }
        return data.buffer;
    }
}