"use strict";

type DirBrowseRequestResult = {
    currentFolder?: string,
    parentFolder?: string,
    folders: DirBrowseFolderListItem[]
};


type DirBrowseFolderListItem = {
    name: string;
    fullPath: string;
    canBeSelected: boolean;
};

type FolderSelectionFunction = (target: string, folder: string) => any;

namespace DirBrowse {
    //TODO

    const emoji = {
        folder: "\uD83D\uDCC1",
        check: "\uD83D\uDD79"
    };
    export async function browse(browseItem: HTMLElement) {
        if (!(browseItem instanceof HTMLElement)) {
            throw new TypeError("Argument not an Element");
        }
        const target = browseItem.dataset.target as string;
        if (!target) {
            throw new Error("Target not defined");
        }
        const ie = document.querySelector(target) as HTMLInputElement;
        if (!ie) {
            throw new Error(`Target item '${target}' does not exist`);
        }
        if (!(ie instanceof HTMLInputElement)) {
            throw new Error(`Target item '${target}' is not an INPUT element`);
        }
        getAndRender(ie.value, target);
        //throw new Error("Not implemented");
    }

    function selectFolder(target: string, folder: string) {
        const e = document.querySelector(target) as HTMLElement;
        const dlg = document.querySelector("#dirBrowse") as HTMLDialogElement
        if (e instanceof HTMLInputElement) {
            e.value = folder;
            dlg.close();
            dlg.remove();
        }
    }

    async function getAndRender(folder: string, target: string) {
        const folders = await getFolder(folder);
        if (folders) {
            renderFolder(folders, target);
        }
    }

    function nav(this: HTMLAnchorElement, e: MouseEvent) {
        e.preventDefault();
        const dir = this.dataset.folder as string;
        const mode = this.dataset.mode as string;
        const target = this.dataset.target as string;
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

    function renderFolder(result: DirBrowseRequestResult, target: string) {
        const dlg = (document.querySelector("dialog#dirBrowse") || document.createElement("dialog")) as HTMLDialogElement;
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

    async function getFolder(parent?: string | null): Promise<DirBrowseRequestResult | null> {
        const fd = new FormData();
        fd.addCsrf();
        fd.set("Folder", parent ?? "");
        const result = await fetch("/Admin/Folder", { method: "POST", body: fd });
        if (result.ok) {
            const contents = (await result.json()) as DirBrowseRequestResult;
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
}