namespace Account {
    /** Password rating result */
    type PasswordRating = {
        /** true, if the password is considered safe */
        isSafe: boolean;

        /** Score metric for uppercase */
        hasUppercase: boolean;
        /** Score metric for lowercase */
        hasLowercase: boolean;
        /** Score metric for digit */
        hasDigit: boolean;
        /** Score metric for symbol */
        hasSymbol: boolean;

        /** Password length */
        length: number;
        /** Minimum required length */
        minLength: number;

        /** Computed score */
        score: number;
        /** Minimum required score */
        minScore: number;
        /** Maximum possible score */
        maxScore: number
    };

    const difficulty = 1_000_000;

    export async function getAccountId(username: string, password: string) {
        if (!ratePassword(password).isSafe) {
            throw new Error("Password is not safe");
        }
        const bytes = await Cryptography.deriveBytes(username, password, difficulty, 16);
        const id = Guid.parse(bytes);
        console.log(id);
    }

    export async function signIn(id: string) {
        if (!Guid.test(id) || id === Guid.empty) {
            throw new Error("Invalid id");
        }
        const fd = CSRF.enrich(new FormData());
        fd.set("Id", id);
        const result = await fetch("/Account/SignIn", { method: "POST", body: fd });
        return result.ok;
    }

    export async function signOut() {
        const fd = CSRF.enrich(new FormData());
        const result = await fetch("/Account/SignOut", { method: "POST", body: fd });
        return result.ok;
    }

    export function ratePassword(password?: string): PasswordRating {
        password = password || "";

        const ret = {} as PasswordRating;

        ret.hasUppercase = /[A-Z]/.test(password);
        ret.hasLowercase = /[a-z]/.test(password);
        ret.hasDigit = /\d/.test(password);
        ret.hasSymbol = /[^\da-zA-Z]/.test(password);

        ret.length = password.length;
        ret.minLength = 8;

        //Compute score
        ret.score = (ret.hasUppercase ? 1 : 0) +
            (ret.hasLowercase ? 1 : 0) +
            (ret.hasDigit ? 1 : 0) +
            (ret.hasSymbol ? 1 : 0);

        ret.maxScore = 4;
        ret.minScore = 3;

        ret.isSafe = ret.length >= ret.minLength && ret.score >= ret.minScore;

        return ret;
    }
}