using UnityEngine;
using System.Collections;
using TMPro;
using WebSocketSharp;

    public class ArduinoController : MonoBehaviour
    {
        // use 'ws://localhost:4005' if running on same device
        private string serverUrl = "ws://192.168.1.29:4005";

        public TMP_InputField arduinoCodeInputField;

        public GameObject ledIndicatorPin7;
        public GameObject ledIndicatorPin6;
        public GameObject ledIndicatorBuiltIn;

        private bool buttonState = false;

        private WebSocket ws;

        private float targetZRotation = 0f;
        private float rotationSpeed = 20f;


        private string pendingArduinoCode;

void Update() {
    if (!string.IsNullOrEmpty(pendingArduinoCode)) {
        arduinoCodeInputField.text = pendingArduinoCode;
        pendingArduinoCode = null;  // Clear the pending code once updated
    }
}

        void Start()
        {
            // Initialize the WebSocket connection
            ws = new WebSocket(serverUrl);
            
            ws.OnMessage += (sender, e) =>
            {
                Debug.Log("Message Received: " + e.Data);
                ProcessWebSocketMessage(e.Data);
            };

            ws.OnOpen += (sender, e) => 
            {
                Debug.Log("WebSocket connection opened");
            };

            ws.OnError += (sender, e) => 
            {
                Debug.LogError("WebSocket error: " + e.Message);
            };

            ws.OnClose += (sender, e) => 
            {
                Debug.Log("WebSocket connection closed");
            };

            // Connect to the server
            ws.Connect();
        }

        void OnDestroy()
        {
            if (ws != null)
            {
                ws.Close();
            }
        }

        private void ProcessWebSocketMessage(string message)
        {
            ServerMessage serverMessage = JsonUtility.FromJson<ServerMessage>(message);

            if (serverMessage.type == "pin-states")
            {
                // Debug.Log(serverMessage.pin13);
                // Call UpdateLedIndicator on the main thread for each LED
                UpdateLedIndicator(ledIndicatorPin7, IntToBool(serverMessage.pin7));
                UpdateLedIndicator(ledIndicatorPin6, IntToBool(serverMessage.pin6));
                UpdateLedIndicator(ledIndicatorBuiltIn, IntToBool(serverMessage.pin13));
            }
            else if (serverMessage.type == "code")
            {
                pendingArduinoCode = serverMessage.code;  // Store code to be processed in Update()
            }
        }


    private void UpdateLedIndicator(GameObject ledIndicator, bool status)
    {
        if (ledIndicator != null)
        {
            ledIndicator.SetActive(status);
        }
    }

    // If value other than 0, then return TRUE
    private bool IntToBool(int value) {
        return value != 0;
    }


// Send input change to avr8js
public void SendButtonStateChange(string port, int pin) {
    buttonState = !buttonState; // Act as toggle
    
    InputStateMessage messageObject = new InputStateMessage {
        type = "input-change",
        port = port,
        pin = pin,
        state = buttonState
    };

    string message = JsonUtility.ToJson(messageObject);
    Debug.Log($"Sending button state change: {message}");
    ws.Send(message);
}


public void StopCodeExecution() {
    string type = "stop-code";
    
    if (ws.IsAlive)
    {
        CompileRunMessage messageObject = new CompileRunMessage("");
        messageObject.type = type;
        string message = JsonUtility.ToJson(messageObject);

        Debug.Log("Stopping ARDUINO!");

        ws.Send(message);
    }
}

public void CompileAndRunCode()
{
    if (arduinoCodeInputField != null && ws.IsAlive)
    {
        Debug.Log("Attempting to compile and run code");

        CompileRunMessage messageObject = new CompileRunMessage(arduinoCodeInputField.text);
        string message = JsonUtility.ToJson(messageObject);

        Debug.Log("Sending message: " + message);

        ws.Send(message);
    }
}


[System.Serializable]
public class ServerMessage {
    public string type;
    public int pin0, pin1, pin2, pin3, pin4, pin5, pin6, pin7, pin13;
    public string code; // Arduino sketch code.
}



    [System.Serializable]
    public class CompileRunMessage
    {
        public string type = "compile-run";
        public string sketch;

        public CompileRunMessage(string sketchContent)
        {
            sketch = sketchContent;
        }
    }

    [System.Serializable]
    public class InputStateMessage
    {
        public string type;
        public string port;
        public int pin;
        public bool state;
    }

    [System.Serializable]
    public class LedStateResponse
    {
        public bool ledStatus;
    }

    [System.Serializable]
    public class ArduinoSketch
    {
        public string sketch;
    }
    }
