const avr8js = require("avr8js");
const { loadHex } = require("./intelhex");

const {
  avrInstruction,
  AVRTimer,
  CPU,
  timer0Config,
  timer1Config,
  timer2Config,
  AVRIOPort,
  AVRUSART,
  portBConfig,
  portCConfig,
  portDConfig,
  usart0Config
} = avr8js;

const FLASH = 0x8000;
  
class AVRRunner {
    program = new Uint16Array(FLASH)
    speed = 16e6 // 16 MHZ
    workUnitCycles = 500000

    constructor(hex) {
        loadHex(hex, new Uint8Array(this.program.buffer))
        this.cpu = new CPU(this.program)
        this.timer0 = new AVRTimer(this.cpu, timer0Config)
        this.timer1 = new AVRTimer(this.cpu, timer1Config)
        this.timer2 = new AVRTimer(this.cpu, timer2Config)
        this.portB = new AVRIOPort(this.cpu, portBConfig)
        this.portC = new AVRIOPort(this.cpu, portCConfig)
        this.portD = new AVRIOPort(this.cpu, portDConfig)
        this.usart = new AVRUSART(this.cpu, usart0Config, this.speed)
    }

    // CPU main loop
    execute(callback) {
        this.stopped = false;

        const executeCycle = () => {
            const startTime = Date.now();
            while (!this.stopped && (Date.now() - startTime < 20)) { // Run for no more than 20ms at a time
                avrInstruction(this.cpu);
                this.cpu.tick();
            }
            callback(this.cpu);

            if (!this.stopped) {
                setTimeout(executeCycle, 0); // Schedule the next execution slice
            }
        };

        executeCycle();
    }
    
    stop() {
        this.stopped = true; // Set running to false to stop execution
    }
}
module.exports = { AVRRunner };