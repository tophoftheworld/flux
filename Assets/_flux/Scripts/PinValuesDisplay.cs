using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Make sure to include this for TMP_InputField

public class PinValuesDisplay : MonoBehaviour
{
    public ArduinoController arduinoController; 
    public TMP_InputField displayField; 

    // Update is called once per frame
    void Update()
    {
        if (arduinoController != null && displayField != null)
        {
            displayField.text = FormatPinStates(arduinoController.pinStates);
        }
    }

    // Format the pin states into string
    private string FormatPinStates(float[] pinStates)
    {
        string result = "";
        for (int i = 0; i < pinStates.Length; i++)
        {
            result += $"pin{i}: {(int)pinStates[i]}\n";
        }
        return result;
    }
}
