const { parentPort, workerData } = require('worker_threads');
const { AVRRunner } = require('./execute');
const { parse } = require('intel-hex');
const { CPUPerformance, PinState } = require('avr8js');

let inputChangeQueue = [];

let pinStates = {
    pin0: 0,
    pin1: 0,
    pin2: 0,
    pin3: 0,
    pin4: 0,
    pin5: 0,
    pin6: 0,
    pin7: 0,
    pin8: 0,
    pin9: 0,
    pin10: 0,
    pin11: 0,
    pin12: 0,
    pin13: 0 // Built-in LED
  };

let runner = null;

let simulationRunning = false;
let simulationShouldContinue = true;

parentPort.on('message', (message) => {
    // Only receive commands when the simulation is running
    if(simulationRunning){
        if (message.type === 'input-change') {
            handlePortInputChange(message.portName, message.pin, message.state);
            console.log(`Nakuha input change: port ${message.portName}, pin ${message.pin}, state ${message.state}`);
            // handlePortInputChange(message.portName, message.pin, message.state);
        } else if (message.type === 'stop-execution'){
            stopExecution();
        }
}
});

function handlePortInputChange(portName, pin, state) {
    inputChangeQueue.push({ portName, pin, state });
}

function processInputChangeQueue() {
    while (inputChangeQueue.length > 0) {
        const { portName, pin, state } = inputChangeQueue.shift();
        const port = runner[`port${portName}`];
        if (port) {
            port.setPin(pin, state);
            console.log(`Processed input change on port ${portName} pin ${pin} to state ${state}`);
        }
    }
}

function stopExecution() {
    if (simulationRunning && runner) {
        simulationShouldContinue = false; // Set flag to false to stop the simulation loop
        console.log("Simulation stop requested.");
    }
}


function startSimulation(hex) {
    const MHZ = 16000000;
    runner = new AVRRunner(hex);

    // PWM won't work on 3,9,10, and 11 yet
    let pwmPins = [5, 6].reduce((obj, pin) => {
        obj[pin] = { highCycles: 0, lastCycle: 0, lastUpdateCycles: 0,state: PinState.Low };
        return obj;
    }, {});

    const updatePWM = (pin, state) => {
        if (state !== pwmPins[pin].state) {
            const delta = runner.cpu.cycles - pwmPins[pin].lastCycle;
            if (pwmPins[pin].state === PinState.High) {
                pwmPins[pin].highCycles += delta;
            }
            pwmPins[pin].state = state;
            pwmPins[pin].lastCycle = runner.cpu.cycles;
        }
    };

    const calculateDutyCycle = (pin) => {
        let cyclesSinceUpdate = runner.cpu.cycles - pwmPins[pin].lastUpdateCycles;
        if (pwmPins[pin].state === PinState.High) {
            pwmPins[pin].highCycles += runner.cpu.cycles - pwmPins[pin].lastCycle;
        }
        // Convert to integer first
        let dutyCycle = (pwmPins[pin].highCycles / cyclesSinceUpdate) * 255;
        pinStates[`pin${pin}`] = Math.round(dutyCycle);
        
        pwmPins[pin].lastUpdateCycles = runner.cpu.cycles;
        pwmPins[pin].lastCycle = runner.cpu.cycles;
        pwmPins[pin].highCycles = 0;
    };


    const { data } = parse(hex);
    const progData = new Uint8Array(data);
    console.log(data);

    simulationRunning = true;
    simulationShouldContinue = true;

    runner.portD.addListener(() => {
        for (let pin = 0; pin <= 7; pin++) {
            if (!pwmPins.hasOwnProperty(pin)) { // Skip PWM pins
                pinStates[`pin${pin}`] = runner.portD.pinState(pin) === PinState.High ? 255 : 0;
            }
        }
        // Handle PWM pins on Port D
        Object.keys(pwmPins).forEach(pin => {
            let port;
            // If pin index is less than 8, then its under port D
            port = pin < 8 ? runner.portD : runner.portB;
            let state = port.pinState(pin);
            updatePWM(pin, state);
        });

        parentPort.postMessage({ type: 'update-pin-states', pinStates });
    });

    runner.portB.addListener(() => {
        for (let pin = 8; pin <= 13; pin++) {
            let actualPinIndex = pin - 8; // Convert Arduino pin number to port index
            if (!pwmPins.hasOwnProperty(pin)) { // Skip PWM pins
                pinStates[`pin${pin}`] = runner.portB.pinState(actualPinIndex) === PinState.High ? 255 : 0;
            }
        }
    
        // Handle PWM pins on Port B
        // Object.keys(pwmPins).forEach(pin => {
        //     let port;
        //     // If pin index is less than 8, then its under port D
        //     port = pin < 8 ? runner.portD : runner.portB;
        //     let state = port.pinState(pin);
        //     updatePWM(pin, state);
        // });

        parentPort.postMessage({ type: 'update-pin-states', pinStates });
    });
    

    // const cpuPerf = new CPUPerformance(runner.cpu, MHZ);
    runner.execute((cpu) => {
        if (!simulationShouldContinue) {
            console.log("Stopping simulation...");
            runner.stop(); // Assuming there's a stop method to cleanly stop the runner
            simulationRunning = false;
            return; // Break out of the loop
        }

        // Process pending input changes when the simulation yields
        processInputChangeQueue();
        
        Object.keys(pwmPins).forEach(pin => {
            calculateDutyCycle(parseInt(pin));
        });

        // Yield back to the event loop to keep the system responsive
        setImmediate(() => {
            // console.log('Resumed execution from event loop');
        });

        // parentPort.postMessage({ type: 'update-pin-states', pinStates });
      });
}

parentPort.on('message', (message) => {
    if (message.type === 'get-code') {
        startSimulation(message.hex);
    }
});
