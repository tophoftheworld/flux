using UnityEngine;

public class ServoMotor : MonoBehaviour, IOutputDevice
{
    public int pin;
    public Transform pivot;
    public float maxDegree = 180f; // Public variable for maximum degree
    private ArduinoController arduinoController;
    private float initialAngle;

    void Start()
    {
        if (pivot == null)
        {
            Debug.LogError("ServoMotor script requires a child Transform assigned as pivot.");
        }
        initialAngle = pivot.localRotation.eulerAngles.z;

        arduinoController = FindObjectOfType<ArduinoController>();
        if (arduinoController != null)
        {
            arduinoController.RegisterDevice(this, pin);
        }
    }

    public void RotateByAngle(float angle)
    {
        if (pivot != null)
        {
            // Add the angle to the initial angle and rotate the pivot around the Z-axis
            float newAngle = initialAngle + angle;
            newAngle = Mathf.Clamp(newAngle, 0, maxDegree); // Ensure the angle stays within the bounds
            pivot.localRotation = Quaternion.Euler(0, 0, newAngle);
        }
        else
        {
            Debug.LogError("No pivot assigned for the ServoMotor.");
        }
    }

    public void UpdatePinState(int newState)
    {
        float angle = Map(newState, 0, 255, 0, maxDegree); // Map 0-255 to 0-maxDegree
        RotateByAngle(angle);
    }

    private float Map(int value, int fromSource, int toSource, float fromTarget, float toTarget)
    {
        return (value - fromSource) / (float)(toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
    }
}
