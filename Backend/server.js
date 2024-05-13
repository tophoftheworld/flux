const express = require('express');
const cors = require('cors');
const axios = require('axios');
const { CPU, avrInstruction, AVRIOPort, portBConfig,portDConfig, PinState, AVRTimer, timer0Config } = require('avr8js');
const WebSocket = require('ws');
const http = require('http');
const { Worker } = require('worker_threads');

const app = express();
const port = 8080;

app.use(cors());
app.use(express.json());

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
  

// Placeholder code
let arduinoCode = `

const int buttonAPin = 2; 
const int buttonBPin = 3; 
const int builtInLed = 13;
const int servoPin = 6;
const int ledPin = 5;

bool builtInLedState = false;
bool ledState = false;
bool fadeDirection = true;

int lastButtonAState = LOW;
int lastButtonBState = LOW;

int brightness = 0;
int fadeAmount = 10;

int rotation = 0;
int rotateAmount = 1;
int maxBrightness = 255;

unsigned long lastDebounceTime = 0;  // Last time the output pin was toggled
unsigned long debounceDelay = 50;    // Debounce time in milliseconds

unsigned long previousMillis = 0;    // Stores last update time
const long interval = 30;          // Interval at which to blink (milliseconds)

void setup() {
    pinMode(servoPin, OUTPUT);
    pinMode(ledPin, OUTPUT);
    pinMode(builtInLed, OUTPUT);
    pinMode(buttonAPin, INPUT);
    pinMode(buttonBPin, INPUT);
}

void loop() {
    unsigned long currentMillis = millis();

    // // LED PWM that works with input
    if (currentMillis - previousMillis >= interval) {
        previousMillis = currentMillis;

        // Change the rotation for next time through the loop:
        analogWrite(servoPin, rotation);

        // Adjust the rotation for next time
        rotation = rotation + rotateAmount;

        // Reverse the direction of the fading at the ends of the fade:
        if (rotation <= 0 || rotation >= 255) {
            rotateAmount = -rotateAmount;
        }

        // Fade LED
        if (fadeDirection) {
            brightness += fadeAmount;
            if (brightness >= 255) {
                brightness = 255;
            }
        } else {
            brightness -= fadeAmount;
            if (brightness <= 0) {
                brightness = 0;
            }
        }
        analogWrite(ledPin, brightness);
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
        lastDebounceTime = currentMillis;

    }

    if ((currentMillis - lastDebounceTime) > debounceDelay) {
        // If the button state has changed:
        if (readingA != builtInLedState) {
            builtInLedState = readingA;

            // Only toggle the LED if the new button state is HIGH
            if (builtInLedState == HIGH) {
                digitalWrite(builtInLed, !digitalRead(builtInLed));
            }
        }
        if (readingB != ledState) {
            ledState = readingB;

            if(ledState == HIGH){
                fadeDirection = !fadeDirection;
            }
        }
    }

    // Save the reading for next loop
    lastButtonAState = readingA;
    lastButtonBState = readingB;
}

`;

let sketchCompiled = '';
let globalHex = '';

let avr8jsWorker = null;

async function execute(hex) {
    sendConsoleLog('Program running...');
    avr8jsWorker = new Worker('./avr-simulation-worker.js', { workerData: { } });;
    avr8jsWorker.postMessage({ type: 'get-code', hex});

    avr8jsWorker.on('message', (message) => {
        if (message.type === 'update-pin-states') {
            Object.keys(message.pinStates).forEach(pin => {
                pinStates[pin] = message.pinStates[pin];
            });
            sendPinStates();
        } else if (message.type === 'status') {
            console.log(`Simulation time: ${message.time} (${message.speed}%)`);
        } else if (message.type === 'stopped') {
            console.log('Simulation has stopped');
        }
    });

    avr8jsWorker.on('error', (error) => {
        console.error('Worker error:', error);
    });

    avr8jsWorker.on('exit', (code) => {
        if (code !== 0) {
            console.error(`Worker stopped with exit code ${code}`);
        }
    });
    // avr8jsWorker.postMessage('stop');

    // avr8jsWorker.postMessage({ type: 'input-change', portName: 'D', pin: 2, state: false });
}


// Compile the Arduino source code
async function compileCode(sketch){
    console.log("COMPILING...");
    sendConsoleLog('Compiling...');
    const result = await axios.post('https://hexi.wokwi.com/build', {
        sketch: sketch
    }, {
        headers: {
            'Content-Type': 'application/json'
        }
    });
    const { hex, stderr } = result.data;
    if (!hex) {
        // console.error(stderr);
        sendConsoleLog(stderr);
        return;
    }
    // Record the arduino code that is recently compiled
    sketchCompiled = sketch;
    globalHex = hex;
    sendConsoleLog('Compile Successful!');
}

async function executeCode(sketch){
    // If the arduino code is different, then compile first
    if (sketch != sketchCompiled){
        await compileCode(sketch);
    }
    execute(globalHex);
}

let lastSentPinStates = {};

// Function to send pin states to Unity
function sendPinStates() {
    // Convert to Int
    const intPinStates = Object.fromEntries(
        Object.entries(pinStates).map(([key, value]) => [key, parseInt(value)])
    );

    const hasChanges = Object.keys(intPinStates).some(key => intPinStates[key] !== lastSentPinStates[key]);

    // Only send updates if there are changes
    if (hasChanges) {
        wss.clients.forEach(client => {
            if (client.readyState === WebSocket.OPEN) {
                client.send(JSON.stringify({ type: 'pin-states', ...intPinStates }));
            }
        });

        // Update last sent states
        lastSentPinStates = { ...intPinStates };
    }
}

// Function to send pin states to Unity
function sendConsoleLog(consoleLog) {
    // Convert to Int
    wss.clients.forEach(client => {
        if (client.readyState === WebSocket.OPEN) {
            client.send(JSON.stringify({ type: 'console-log', log: consoleLog }));
        }
    });
}

// STOP THE RUNNING SIMULATION
function stopExecution() {
    console.log("Stop");
    sendConsoleLog('Stopping...');
    if (avr8jsWorker && avr8jsWorker.postMessage) {
        avr8jsWorker.postMessage({ type: 'stop-execution'});
    } else {
        console.error("No active worker or postMessage method unavailable.");
    }
    avr8jsWorker.terminate();
}

// The changes the input state of a pin on the specified port
function handleInputChange(portName, pin, state) {
    console.log(`Sending input change to worker: port ${portName}, pin ${pin}, state ${state}`);
    // console.log("Worker status:", avr8jsWorker ? "active" : "inactive");

    if (avr8jsWorker && avr8jsWorker.postMessage) {
        avr8jsWorker.postMessage({ type: 'input-change', portName, pin, state });
    } else {
        console.error("No active worker or postMessage method unavailable.");
    }
}



// Create an HTTP server
const server = http.createServer(app);

// Create a WebSocket server
const wss = new WebSocket.Server({ server });

wss.on('connection', function connection(ws) {
    ws.on('message', function incoming(message) {
        // console.log('received: %s', message);
        const data = JSON.parse(message);

        if (data.type === 'execute-code') {
            executeCode(data.sketch);
        } else if (data.type === 'compile-code'){
            compileCode(data.sketch);
        } else if (data.type === 'stop-execution') {
            stopExecution();
        } else if (data.type === 'input-change') {
             // message format: { type: 'input-change', pin: 2, state: true/false }
             handleInputChange(data.port, data.pin, data.state);
        }
    });

    // CAN BE REMOVE IF WE WANT THE SIMULATION TO WORK DEFINITELY
    // Stop the simulation if the client disconnects
    ws.on('close', () => {
        console.log('Client disconnected. Stopping simulation.');
        stopExecution();
    });


    // Send the placeholder code when a new client connects
    ws.send(JSON.stringify({ type: 'code', code: arduinoCode }));
});


server.listen(port, () => {
    console.log(`Server and WebSocket are running at http://localhost:${port}`);
});