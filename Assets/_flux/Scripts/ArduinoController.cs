using UnityEngine;
using System.Collections;
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
//             arduinoController.CompileAndRunCode();
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
    public string serverIpAddress = "192.168.8.101";

    // use 'ws://localhost:4005' if running on same device
    private string serverUrl;

    [Header("Game Objects")]

    public TMP_InputField arduinoCodeInputField;

    public GameObject ledIndicatorPin7;
    public GameObject ledIndicatorPin6;
    public GameObject ledIndicatorBuiltIn;

    private bool buttonState = false;

    private WebSocket ws;

    private string pendingArduinoCode;

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

        serverUrl = $"ws://{serverIpAddress}:4005";
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

    // Process message receive from websocket
    private void ProcessWebSocketMessage(string message)
    {
        ServerMessage serverMessage = JsonUtility.FromJson<ServerMessage>(message);

        // Update LEDs based on output received from pins
        if (serverMessage.type == "pin-states")
        {
            // Debug.Log(serverMessage.pin13);
            UpdateLedIndicator(ledIndicatorPin7, serverMessage.pin7);
            UpdateLedIndicator(ledIndicatorPin6, serverMessage.pin6);
            UpdateLedIndicator(ledIndicatorBuiltIn, serverMessage.pin13);
        }
        // Get the placeholder code sent from backend
        else if (serverMessage.type == "code")
        {
            pendingArduinoCode = serverMessage.code;  // Store code to be processed in Update()
        }
    }

    // Set LEDs as on or off based on value of pin where they are connected
    private void UpdateLedIndicator(GameObject ledIndicator, float fValue)
    {
        if (ledIndicator != null)
        {
            LED led = ledIndicator.GetComponent<LED>();
            if (led != null)
            {
                int iValue = (int)fValue;
                led.SetBrightness(iValue);
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

    // STOP THE RUNNING CODE
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
        public float pin0, pin1, pin2, pin3, pin4, pin5, pin6, pin7, pin13;
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
    public class ArduinoSketch
    {
        public string sketch;
    }
}
