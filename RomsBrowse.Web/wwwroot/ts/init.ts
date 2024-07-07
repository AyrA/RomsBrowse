"use strict";

namespace Init {
    type ApiResponse = {
        success: boolean;
        error?: string;
    };

    const form1 = q("#frmInitSql") as HTMLFormElement;
    const form2 = q("#frmInitSqlite") as HTMLFormElement;
    const btnTest1 = q("#btnInitTest1") as HTMLInputElement;
    const btnTest2 = q("#btnInitTest2") as HTMLInputElement;

    const rbSec1 = q("#showsec1") as HTMLInputElement;
    const rbSec2 = q("#showsec2") as HTMLInputElement;

    async function init(e: Event, form: HTMLFormElement, btnTest: HTMLInputElement) {
        e.preventDefault();
        btnTest.disabled = true;
        btnTest.value = "Testing...";
        try {
            if (form.reportValidity()) {
                const fd = new FormData(form);
                const response = await fetch("/Init/Test", { method: "POST", body: fd });
                if (response.ok) {
                    const result = (await response.json()) as ApiResponse;
                    console.log(result);
                    if (result.success) {
                        alert("Data is valid");
                        form.submit();
                    }
                    else {
                        alert(result.error || "Unspecified error");
                    }
                }
                else {
                    alert("Unspecified error");
                }
            }
        }
        finally {
            btnTest.value = "Test";
            btnTest.disabled = false;
        }
    }

    function handleChange() {
        const items = {
            a: q("section#sec1") as HTMLElement,
            b: q("section#sec2") as HTMLElement
        }
        if (rbSec2.checked) {
            const c = items.a;
            items.a = items.b;
            items.b = c;
        }
        else if (!rbSec1.checked) {
            items.a.classList.add("d-none");
            items.a.classList.remove("d-block");
            items.b.classList.add("d-none");
            items.b.classList.remove("d-block");
            return; //None is checked
        }
        items.a.classList.add("d-block");
        items.a.classList.remove("d-none");
        items.b.classList.add("d-none");
        items.b.classList.remove("d-block");
    }

    if (btnTest1 && btnTest2 && rbSec1 && rbSec2) {
        btnTest1.addEventListener("click", (e: Event) => init(e, form1, btnTest1));
        btnTest2.addEventListener("click", (e: Event) => init(e, form2, btnTest2));

        rbSec1.addEventListener("change", handleChange);
        rbSec2.addEventListener("change", handleChange);

        handleChange();
    }
}