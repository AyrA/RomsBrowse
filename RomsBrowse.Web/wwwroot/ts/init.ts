"use strict";

namespace Init {
    type ApiResponse = {
        success: boolean;
        error?: string;
    };

    const form = q("#frmInit") as HTMLFormElement;
    const btnTest = q("#frmInit #btnInitTest") as HTMLInputElement;

    async function init(e: Event) {
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

    if (btnTest) {
        btnTest.addEventListener("click", init);
    }
}