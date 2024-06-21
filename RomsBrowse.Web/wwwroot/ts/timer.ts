"use strict";

namespace Timer {
    const timerElement = q("[data-auto-reload]") as HTMLElement;
    const fallback = 10;

    if (timerElement) {
        let timer = parseInt(timerElement.dataset.autoReload!);
        if (Number.isNaN(timer)) {
            console.error("Element", timerElement, "has invalid timer value:", timerElement.dataset.autoReload);
            throw new Error("Element has invalid timer value: " + timerElement.dataset.autoReload);
        }
        if (timer <= 0) {
            console.warn(`Element timer value ${timer} is out of range. Setting to ${fallback}`);
            timer = fallback;
        }
        //Countdown
        const timerId = setInterval(() => {
            --timer;
            if (timer >= 0) {
                timerElement.textContent = timer.toString();
            }
            //Remove timer handler when reaching zero
            if (timer <= 0) {
                clearInterval(timerId);
                location.reload();
            }
        }, 1000);
    }
}