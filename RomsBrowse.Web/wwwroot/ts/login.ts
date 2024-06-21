namespace Account {
    const difficulty = 1_000_000;
    export async function getAccountId(username: string, password: string) {
        if (!isSafePassword(password)) {
            throw new Error("Password is not safe");
        }
        const id = await Cryptography.deriveGuid(username, password, difficulty, 32);
    }

    export function isSafePassword(password?: string) {
        if (!password) {
            return false;
        }
        if (password.length < 8) {
            return false;
        }
        //Check uppercase, lowercase, digits and symbols
        let features = 0;
        features += (/[A-Z]/.exec(password) ? 1 : 0);
        features += (/[a-z]/.exec(password) ? 1 : 0);
        features += (/\d/.exec(password) ? 1 : 0);
        features += (/[^\da-zA-Z]/.exec(password) ? 1 : 0);
        return features >= 3; //Require 3 of the four categories
    }
}