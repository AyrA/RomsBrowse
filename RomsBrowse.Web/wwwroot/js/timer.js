"use strict";
var Timer;
(function (Timer) {
    function reloadHandler() {
        location.reload();
    }
    function reloadDelayed(ms) {
        return setTimeout(reloadHandler, ms);
    }
    reloadDelayed(10000);
})(Timer || (Timer = {}));
