"use strict";
var Core;
(function (Core) {
    var NES;
    (function (NES) {
        console.log("TODO");
        /*
        //@ts-ignore
        const jsnes = globalThis.jsnes as NesEmuGlobal;
        const SCREEN_WIDTH = 256;
        const SCREEN_HEIGHT = 240;
        const FRAMEBUFFER_SIZE = SCREEN_WIDTH * SCREEN_HEIGHT;
    
        let canvas_ctx: CanvasRenderingContext2D;
        let image: ImageData;
        let framebuffer_u8: Uint8ClampedArray;
        let framebuffer_u32: Uint32Array;
    
        const AUDIO_BUFFERING = 512;
        const SAMPLE_COUNT = 4 * 1024;
        const SAMPLE_MASK = SAMPLE_COUNT - 1;
        const audio_samples_L = new Float32Array(SAMPLE_COUNT);
        const audio_samples_R = new Float32Array(SAMPLE_COUNT);
        let audio_write_cursor = 0, audio_read_cursor = 0;
    
        //@ts-ignore
        const nes = new jsnes.NES({
            onFrame: function (framebuffer_24: Uint32Array) {
                for (var i = 0; i < FRAMEBUFFER_SIZE; i++) {
                    framebuffer_u32[i] = 0xFF000000 | framebuffer_24[i];
                }
            },
            onAudioSample: function (l: number, r: number) {
                audio_samples_L[audio_write_cursor] = l;
                audio_samples_R[audio_write_cursor] = r;
                audio_write_cursor = (audio_write_cursor + 1) & SAMPLE_MASK;
            },
        }) as NesEmu;
    
        function onAnimationFrame() {
            window.requestAnimationFrame(onAnimationFrame);
    
            image.data.set(framebuffer_u8);
            canvas_ctx.putImageData(image, 0, 0);
        }
    
        function audio_remain() {
            return (audio_write_cursor - audio_read_cursor) & SAMPLE_MASK;
        }
    
        function audio_callback(event: AudioProcessingEvent) {
            var dst = event.outputBuffer;
            var len = dst.length;
    
            // Attempt to avoid buffer underruns.
            if (audio_remain() < AUDIO_BUFFERING) nes.frame();
    
            var dst_l = dst.getChannelData(0);
            var dst_r = dst.getChannelData(1);
            for (var i = 0; i < len; i++) {
                var src_idx = (audio_read_cursor + i) & SAMPLE_MASK;
                dst_l[i] = audio_samples_L[src_idx];
                dst_r[i] = audio_samples_R[src_idx];
            }
    
            audio_read_cursor = (audio_read_cursor + len) & SAMPLE_MASK;
        }
    
        function keyboard(callback: NesInputCallback, event: KeyboardEvent) {
            const player = 1;
    
            switch (event.key) {
                case "ArrowUp": // UP
                    callback(player, jsnes.Controller.BUTTON_UP);
                    break;
                case "ArrowDown": // Down
                    callback(player, jsnes.Controller.BUTTON_DOWN);
                    break;
                case "ArrowLeft": // Left
                    callback(player, jsnes.Controller.BUTTON_LEFT);
                    break;
                case "ArrowRight": // Right
                    callback(player, jsnes.Controller.BUTTON_RIGHT);
                    break;
                case "a": // 'a' - qwerty, dvorak
                case "A":
                case "q": // 'q' - azerty
                case "Q":
                    callback(player, jsnes.Controller.BUTTON_A);
                    break;
                case "s": // 's' - qwerty, azerty
                case "S":
                case "o": // 'o' - dvorak
                case "O":
                    callback(player, jsnes.Controller.BUTTON_B);
                    break;
                case "Tab":
                    callback(player, jsnes.Controller.BUTTON_SELECT);
                    break;
                case "Enter":
                    callback(player, jsnes.Controller.BUTTON_START);
                    break;
                default:
                    return;
            }
            event.preventDefault();
        }
    
        function nes_init(canvas: HTMLCanvasElement) {
            if (!canvas) {
                throw new Error("Canvas to draw on is not defined");
            }
            canvas_ctx = canvas.getContext("2d")!;
            image = canvas_ctx.getImageData(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);
    
            canvas_ctx.fillStyle = "black";
            canvas_ctx.fillRect(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);
    
            // Allocate framebuffer array.
            var buffer = new ArrayBuffer(image.data.length);
            framebuffer_u8 = new Uint8ClampedArray(buffer);
            framebuffer_u32 = new Uint32Array(buffer);
    
            // Setup audio.
            var audio_ctx = new window.AudioContext();
            var script_processor = audio_ctx.createScriptProcessor(AUDIO_BUFFERING, 0, 2);
            script_processor.onaudioprocess = audio_callback;
            script_processor.connect(audio_ctx.destination);
        }
    
        function nes_boot(rom_data: string) {
            console.log("Loading ROM...");
            nes.loadROM(rom_data);
            console.log("Loaded");
            window.requestAnimationFrame(onAnimationFrame);
        }
    
        function nes_load_url(canvas: HTMLCanvasElement, path: string) {
            console.log("Init NES");
            nes_init(canvas);
    
            var req = new XMLHttpRequest();
            req.open("GET", path);
            req.overrideMimeType("text/plain; charset=x-user-defined");
            req.onerror = () => console.log(`Error loading ${path}: ${req.statusText}`);
    
            req.onload = function () {
                if (this.status === 200) {
                    nes_boot(this.responseText);
                } else if (this.status === 0) {
                    // Aborted, so ignore error
                    console.log("XHR request aborted");
                } else {
                    throw new Error(`XHR failed to get ROM at ${path}`);
                }
            };
    
            req.send();
        }
    
        document.addEventListener('keydown', (event) => { keyboard(nes.buttonDown, event) });
        document.addEventListener('keyup', (event) => { keyboard(nes.buttonUp, event) });
    
        const btn = document.querySelector("#btnStart") as HTMLInputElement;
    
        btn.addEventListener("click", function () {
            btn.remove();
            const e = document.querySelector("[data-rom-url]") as HTMLElement;
            if (!e) {
                throw new Error("ROM url not set");
            }
            nes_load_url(document.querySelector("canvas")!, e.dataset.romUrl as string);
        });
        //*/
    })(NES = Core.NES || (Core.NES = {}));
})(Core || (Core = {}));
