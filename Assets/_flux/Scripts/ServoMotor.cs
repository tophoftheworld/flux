using UnityEngine;

public class ServoMotor : MonoBehaviour, IOutputDevice
{
    public int pin;
    public Transform pivot;
    private ArduinoController arduinoController;

    void Start()
    {
        if (pivot == null)
        {
            Debug.LogError("ServoMotor script requires a child Transform assigned as pivot.");
        }
        ArduinoController arduinoController = FindObjectOfType<ArduinoController>();
        if (arduinoController != null)
        {
            arduinoController.RegisterDevice(this, pin);
        }
    }

    public void RotateToAngle(float angle)
    {
        if (pivot != null)
        {
            // Rotate the pivot around the Z-axis to the specified angle
            pivot.localRotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            Debug.LogError("No pivot assigned for the ServoMotor.");
        }
    }

    public void UpdatePinState(int newState)
    {
        float angle = Map(newState, 0, 255, 0, 180); // Map 0-255 to 0-180 degrees
        RotateToAngle(angle);
    }

    private float Map(int value, int fromSource, int toSource, float fromTarget, float toTarget)
    {
        return (value - fromSource) / (float)(toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
    }
}
