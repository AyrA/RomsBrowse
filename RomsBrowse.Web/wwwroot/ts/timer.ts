"use strict";

namespace Timer {
    function reloadHandler() {
        location.reload();
    }

    function reloadDelayed(ms: number): number {
        return setTimeout(reloadHandler, ms);
    }

    reloadDelayed(10000);
}