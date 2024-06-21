"use strict";

type CsrfToken = {
    cookieToken: string;
    requestToken: string;
    formFieldName: string;
    headerName: string;
}

interface FormData {
    addCsrf: () => FormData;
}

FormData.prototype.addCsrf = function (this: FormData) {
    CSRF.enrich(this); return this;
};

class CSRF {
    static #token = JSON.parse((q("[data-token]") as HTMLElement).dataset.token!) as CsrfToken;

    static get name() {
        return CSRF.#token.formFieldName;
    }
    static get value() {
        return CSRF.#token.requestToken;
    }

    static enrich(fd: FormData): FormData {
        fd.set(CSRF.name, CSRF.value);
        return fd;
    }
}