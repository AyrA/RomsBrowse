"use strict";
var Guid;
(function (Guid) {
    Guid.empty = "00000000-0000-0000-0000-000000000000";
    function parse(data) {
        if (!data) {
            throw new Error("Data not supplied");
        }
        const raw = new Uint8Array(Cryptography.toBuffer(data));
        if (raw.length !== 16) {
            throw new RangeError("Data must consist of exactly 16 bytes");
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
