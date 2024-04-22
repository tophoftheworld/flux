using UnityEngine;
using UnityEngine.UI; // Required for accessing Button component

public class PushButton : MonoBehaviour
{
    public ArduinoController arduinoController;
    public string port; // Port on the arduino where pin would be located
    public int pin;


    public void Press()
    {
        // Send the state change to simulate button press
        arduinoController.SendButtonStateChange(port, pin);
        Invoke(nameof(Release), 0.1f);
    }

    private void Release()
    {
        // Call input change again to return to previous value which is FALSE
        arduinoController.SendButtonStateChange(port, pin);
    }
}