"use strict";
var Account;
(function (Account) {
    const difficulty = 1_000_000;
    async function getAccountId(username, password) {
        if (!ratePassword(password).isSafe) {
            throw new Error("Password is not safe");
        }
        const id = await Cryptography.deriveGuid(username, password, difficulty, 16);
    }
    Account.getAccountId = getAccountId;
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
