"use strict";

/** Delay compensated timer that fires once per second */
namespace DCT {
    const handlers = {} as { [index: string]: Function };
    let id = 0;

    /**
     * Gets a new unique id
     * @returns unique id
     */
    function nextId(): string {
        return `TH-${++id}`;
    }

    /**
     * Adds a timer handler
     * @param handler timer handler
     * @returns timer id
     */
    export function setTimer(handler: Function): string {
        const id = nextId();
        handlers[id] = handler;
        return id;
    }

    /**
     * Removes a handler by its function or by the timer id
     * @param handler Handler function or timer id
     * @returns true, if removed, false otherwise
     * 
     * Using the handler function will remove all instances of that function
     */
    export function removeTimer(handler: string | Function): boolean {
        if (typeof (handler) === "string") {
            return delete handlers[handler];
        }
        else {
            let ret = false;
            const keys = Object.keys(handlers);
            for (let key of keys) {
                if (handlers[key] === handler) {
                    ret = ret || removeTimer(key);
                }
            }
            return ret;
        }
    }

    /**
     * Called once per second.
     * Invokes all handlers and then schedules itself again,
     * compensating for the time it took the handlers to complete
     */
    function tick() {
        const keys = Object.keys(handlers);
        for (let k of keys) {
            try {
                handlers[k]();
            } catch (e) {
                console.error("DCT handler crached:", k);
                console.error(e);
            }
        }
        setTimeout(tick, 1000 - (Date.now() % 1000));
    }

    tick();
}
