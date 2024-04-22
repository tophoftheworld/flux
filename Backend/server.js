const express = require('express');
const cors = require('cors');
const axios = require('axios');
const { parse } = require('intel-hex');
const { CPU, avrInstruction, AVRIOPort, portBConfig,portDConfig, PinState, AVRTimer, timer0Config } = require('avr8js');
const WebSocket = require('ws');
const http = require('http');

const buildHex  = require('./compile');
const { CPUPerformance }  = require  ('./cpu-performance');
const { AVRRunner }  = require  ('./execute');
const { formatTime }  = require  ('./format-time');

const app = express();
const port = 4004;

app.use(cors());
app.use(express.json());

let digitalPinStates = {
    pin0: false,
    pin1: false,
    pin2: false,
    pin3: false,
    pin4: false,
    pin5: false,
    pin6: false,
    pin7: false,
    pin13: false // Built-in LED
  };
  

// Placeholder code
let arduinoCode = `

const int buttonAPin = 2; 
const int buttonBPin = 3; 
const int ledPin = 13;
const int ledPin6 = 6;
const int ledPin7 = 7;

bool builtInLedState = false;
int lastButtonAState = LOW;

bool led7State = false;
int lastButtonBState = LOW;

unsigned long lastDebounceTime = 0;  // Last time the output pin was toggled
unsigned long debounceDelay = 50;    // Debounce time in milliseconds

unsigned long previousMillis = 0;    // Stores last update time
const long interval = 1000;          // Interval at which to blink (milliseconds)

void setup() {
    pinMode(ledPin6, OUTPUT);
    pinMode(ledPin7, OUTPUT);
    pinMode(ledPin, OUTPUT);
    pinMode(buttonAPin, INPUT);
    pinMode(buttonBPin, INPUT);
}

void loop() {
    unsigned long currentMillis = millis();

    // LED on pin 6 will blink
    if (currentMillis - previousMillis >= interval) {
        previousMillis = currentMillis;
        digitalWrite(ledPin6, digitalRead(ledPin6) == LOW ? HIGH : LOW);
    }

    // Read the state of the pushbutton
    int readingA = digitalRead(buttonAPin);
    int readingB = digitalRead(buttonBPin);

    // If the switch changed, due to noise or pressing:
    if (readingA != lastButtonAState) {
        // Reset the debouncing timer
        lastDebounceTime = currentMillis;
    }
    if (readingB != lastButtonBState) {
        // Reset the debouncing timer
        lastDebounceTime = currentMillis;
    }

    if ((currentMillis - lastDebounceTime) > debounceDelay) {
        // If the button state has changed:
        if (readingA != builtInLedState) {
            builtInLedState = readingA;

            // Only toggle the LED if the new button state is HIGH
            if (builtInLedState == HIGH) {
                digitalWrite(ledPin, !digitalRead(ledPin));
            }
        }
        if (readingB != led7State) {
            led7State = readingB;

            // Only toggle the LED if the new button state is HIGH
            if (led7State == HIGH) {
                digitalWrite(ledPin7, !digitalRead(ledPin7));
            }
        }
    }

    // Save the reading for next loop
    lastButtonAState = readingA;
    lastButtonBState = readingB;
}

`;

let simulationRunning = false;
let simulationShouldContinue = true;

let globalCpu = null;

// COMPILE AND RUN
async function compileAndRunCode(sketch) {

    const MHZ = 16000000;

    let lastState = PinState.Input;
    let lastStateCycles = 0;
    let lastUpdateCycles = 0;
    let ledHighCycles = 0;

        
    console.log("COMPILING...");
    // Compile the Arduino source code
    const result = await axios.post('https://hexi.wokwi.com/build', {
        sketch: sketch
    }, {
        headers: {
            'Content-Type': 'application/json'
        }
    });

    const { hex, stderr } = result.data;
    if (!hex) {
        console.error(stderr);
        return;
    }

    const { data } = parse(hex);
    const progData = new Uint8Array(data);
    console.log(data);

    // Set up the simulation
    globalCpu = new CPU(new Uint16Array(progData.buffer));
    globalCpu.portD = new AVRIOPort(globalCpu, portDConfig); // Attach Port D
    globalCpu.portB = new AVRIOPort(globalCpu, portBConfig); // Attach Port B

    const timer = new AVRTimer(globalCpu, timer0Config);


    // Reset control flags for new simulation
    simulationRunning = true;
    simulationShouldContinue = true;

    // Listen to Port D for pin 6 and 7 state changes
    globalCpu.portD.addListener(() => {
        for (let pin = 0; pin <= 7; pin++) {
            digitalPinStates[`pin${pin}`] = globalCpu.portD.pinState(pin) === PinState.High ? 255 : 0;;
        }
        console.log(`LED Pin 7: ${digitalPinStates.pin7 ? 'ON' : 'OFF'}, LED Pin 6: ${digitalPinStates.pin6 ? 'ON' : 'OFF'}`);
    });

    // Listen to Port B for the built-in LED state change
    globalCpu.portB.addListener(() => {
        digitalPinStates.pin13 = globalCpu.portB.pinState(5) === PinState.High ? 255 : 0;; // Pin 13 is PB5 on Port B
        console.log(`LED Builtin: ${digitalPinStates.pin13 ? 'ON' : 'OFF'}`);
    });



      
    // Run the simulation
    while (simulationShouldContinue) {
        for (let i = 0; i < 500000; i++) {
            if (!globalCpu) break;
            avrInstruction(globalCpu);
            globalCpu.tick();
        }
        if (!globalCpu) break;
        sendPinStates();
        await new Promise(resolve => setTimeout(resolve, 0));
    }

    
    simulationRunning = false;
    console.log("Simulation stopped.");
}

// Function to send pin states to Unity
function sendPinStates() {
    wss.clients.forEach(client => {
        if (client.readyState === WebSocket.OPEN) {
        client.send(JSON.stringify({ type: 'pin-states', ...digitalPinStates }));
        }
    });
    }

// STOP THE RUNNING SIMULATION
function stopSimulation() {
    console.log("Stop");
    if (simulationRunning) {
        simulationShouldContinue = false;
        globalCpu = null;
        console.log("Simulation stop requested.");
    } 
}

// The changes the input state of a pin on the specified port
function handleInputChange(portName, pin, state) {
    console.log(`Simulating input change on port ${portName} pin ${pin} to state ${state}`);

    let port;
    switch (portName) {
        case 'D':
            port = globalCpu.portD;
            break;
        case 'B':
            port = globalCpu.portB;
            break;
        case 'C':
            port = globalCpu.portC;
            break;
        case 'A':
            port = globalCpu.portA;
            break;
        default:
            console.error(`Unknown port: ${portName}`);
            return; // Exit if the port name is not recognized
    }

    if (!port) {
        console.error(`Port ${portName} is not initialized`);
        return;
    }
    // Set the pin state
    port.setPin(pin, state);
}



// Create an HTTP server
const server = http.createServer(app);

// Create a WebSocket server
const wss = new WebSocket.Server({ server });

wss.on('connection', function connection(ws) {
    ws.on('message', function incoming(message) {
        console.log('received: %s', message);
        const data = JSON.parse(message);

        if (data.type === 'compile-run') {
            compileAndRunCode(data.sketch);
        } else if (data.type === 'stop-code') {
            stopSimulation();
        } else if (data.type === 'input-change') {
             // message format: { type: 'input-change', pin: 2, state: true/false }
             handleInputChange(data.port, data.pin, data.state);
        }
    });

    // Send the placeholder code when a new client connects
    ws.send(JSON.stringify({ type: 'code', code: arduinoCode }));
});

app.listen(port, () => {
    console.log(`Server running at http://localhost:${port}`);
});

server.listen(4005, () => {
    console.log(`WebSocket Server running at http://localhost:4005`);
});
