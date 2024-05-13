using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TMPro;
using WebSocketSharp;
// using UnityEditor;

// [CustomEditor(typeof(ArduinoController))]
// public class ArduinoControllerEditor : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         base.OnInspectorGUI();

//         ArduinoController arduinoController = (ArduinoController)target;

//         if (GUILayout.Button("start"))
//         {
//             arduinoController.ExecuteCode();
//         }
//          if (GUILayout.Button("stop"))
//         {
//             arduinoController.StopCodeExecution();
//         }
//     }
// }

public class ArduinoController : MonoBehaviour
{

    [Header("Configuration")]
    public bool enableAVR8JS = true;
    public string serverIpAddress = "flux-422808.et.r.appspot.com";
    public string portAddress = "8080";

    // use 'ws://localhost:4005' if running on same device
    private string serverUrl;

    [Header("Game Objects")]

    public TMP_InputField arduinoCodeInputField;
    public TMP_InputField consoleLogInputField;

    private bool buttonState = false;

    private WebSocket ws;
    private bool isConnecting = false;

    private string pendingArduinoCode;

    [Header("Pin States")]
    public float[] pinStates = new float[14];
    public Pin[] pins = new Pin[14];

    private Dictionary<int, List<IOutputDevice>> deviceListeners = new Dictionary<int, List<IOutputDevice>>();

    public void RegisterDevice(IOutputDevice device, int pin)
    {
        if (!deviceListeners.ContainsKey(pin))
        {
            deviceListeners[pin] = new List<IOutputDevice>();
        }
        deviceListeners[pin].Add(device);
    }

    public void NotifyPinChange(int pin, int newState)
    {
        if (deviceListeners.ContainsKey(pin))
        {
            foreach (var device in deviceListeners[pin])
            {
                device.UpdatePinState(newState);
            }
        }
    }

    void Update() {
        // Update Arduino IDE input field using the placeholder code received from the backend
        if (!string.IsNullOrEmpty(pendingArduinoCode)) {
            arduinoCodeInputField.text = pendingArduinoCode;
            pendingArduinoCode = null;  // Clear the pending code once updated
        }
    }

    void Start()
    {

        if (!enableAVR8JS) return;

        serverUrl = $"ws://{serverIpAddress}:{portAddress}";
        ConnectToWebSocket();
    }

    void ConnectToWebSocket()
    {
        if (isConnecting) return; // Prevent multiple connection attempts running at the same time
        isConnecting = true;

        ws = new WebSocket(serverUrl);
        ws.OnOpen += (sender, e) =>
        {
            Debug.Log("WebSocket connection opened");
            isConnecting = false;
        };
        ws.OnMessage += (sender, e) =>
        {
            Debug.Log("Message Received: " + e.Data);
            ProcessWebSocketMessage(e.Data);
        };
        ws.OnError += (sender, e) =>
        {
            Debug.LogError("WebSocket error: " + e.Message);
            TryReconnect();
        };
        ws.OnClose += (sender, e) =>
        {
            Debug.Log("WebSocket connection closed");
            TryReconnect();
        };

        try
        {
            ws.Connect();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("WebSocket connection failed: " + ex.Message);
            TryReconnect();
        }
    }

    void TryReconnect()
    {
        if (ws != null) { ws.Close(); }
        isConnecting = false; // Reset flag to allow reconnect attempts
        Invoke("ConnectToWebSocket", 2); // Retry after 10 seconds
    }

    void OnDestroy()
    {
        if (ws != null)
        {
            ws.Close();
        }
    }

    // Process message receive from websocket
    private void ProcessWebSocketMessage(string message)
    {
        ServerMessage serverMessage = JsonUtility.FromJson<ServerMessage>(message);

        // Update LEDs based on output received from pins
        if (serverMessage.type == "pin-states")
        {
            // Debug.Log(serverMessage.pin13);
            UpdatePinStates(serverMessage);
        }
        // Get the placeholder code sent from backend
        else if (serverMessage.type == "code")
        {
            pendingArduinoCode = serverMessage.code;  // Store code to be processed in Update()
        }
        else if (serverMessage.type == "console-log"){
            if (consoleLogInputField != null)
            {
                consoleLogInputField.text += serverMessage.log + "\n";
                ScrollToBottom(consoleLogInputField);
            }
        }
    }

    private void ScrollToBottom(TMP_InputField inputField)
    {
        // Set the caret position to the end of the text to auto-scroll to the bottom
        inputField.ActivateInputField();  // Activate the input field
        inputField.caretPosition = inputField.text.Length;  // Move the caret to the end
        inputField.DeactivateInputField();  // Optionally deactivate if not required to stay active
    }

    private void UpdatePinStates(ServerMessage message)
    {
        float[] newStates = {
            message.pin0, message.pin1, message.pin2, message.pin3, message.pin4,
            message.pin5, message.pin6, message.pin7, message.pin8, message.pin9,
            message.pin10, message.pin11, message.pin12, message.pin13
        };
        for (int pin = 0; pin < newStates.Length; pin++)
        {
            int newState = (int)newStates[pin];
            if (pinStates[pin] != newState)
            {
                pinStates[pin] = newState;
                NotifyPinChange(pin, newState);
            }
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

        // Send input change to avr8js
    public void SendStateChange(string port, int pin, bool value) {
            InputStateMessage messageObject = new InputStateMessage {
            type = "input-change",
            port = port,
            pin = pin,
            state = value
        };

        string message = JsonUtility.ToJson(messageObject);
        Debug.Log($"Sending button state change: {message}");
        ws.Send(message);
    }

    // STOP THE RUNNING CODE
    public void StopCodeExecution() {
        string type = "stop-execution";
        
        if (ws.IsAlive)
        {
            CompileCodeMessage messageObject = new CompileCodeMessage("");
            messageObject.type = type;
            string message = JsonUtility.ToJson(messageObject);

            Debug.Log("Stopping ARDUINO!");

            ws.Send(message);
        }
    }

    public void CompileCode()
    {
        if (arduinoCodeInputField != null && ws.IsAlive)
        {
            Debug.Log("Attempting to compile and run code");

            CompileCodeMessage messageObject = new CompileCodeMessage(arduinoCodeInputField.text);
            string message = JsonUtility.ToJson(messageObject);

            Debug.Log("Sending message: " + message);

            ws.Send(message);
        }
    }

    public void ExecuteCode()
    {
        if (arduinoCodeInputField != null && ws.IsAlive)
        {
            Debug.Log("Attempting to compile and run code");

            ExecuteCodeMessage messageObject = new ExecuteCodeMessage(arduinoCodeInputField.text);
            string message = JsonUtility.ToJson(messageObject);

            Debug.Log("Sending message: " + message);

            ws.Send(message);
        }
    }


    [System.Serializable]
    public class ServerMessage {
        public string type;
        public float pin0, pin1, pin2, pin3, pin4, pin5, pin6, pin7, pin8, pin9, pin10, pin11, pin12, pin13;
        public string code; // Arduino sketch code.
        public string log;
    }


    [System.Serializable]
    public class CompileCodeMessage
    {
        public string type = "compile-code";
        public string sketch;

        public CompileCodeMessage(string sketchContent)
        {
            sketch = sketchContent;
        }
    }
    [System.Serializable]
    public class ExecuteCodeMessage
    {
        public string type = "execute-code";
        public string sketch;

        public ExecuteCodeMessage(string sketchContent)
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
    public class ArduinoSketch
    {
        public string sketch;
    }
}
