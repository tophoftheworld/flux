using UnityEngine;
using UnityEngine.UI;
// using UnityEditor;

// [CustomEditor(typeof(PushButton))]
// public class PushButtonEditor : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         base.OnInspectorGUI();

//         PushButton pushButton = (PushButton)target;

//         if (GUILayout.Button("Press Button"))
//         {
//             pushButton.Press();
//         }
//     }
// }
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