using UnityEngine;

public class ServoMotor : MonoBehaviour, IOutputDevice
{
    public int pin;
    public Transform pivot;
    public float maxDegree = 180f; // Public variable for maximum degree
    private ArduinoController arduinoController;
    private Vector3 initialRotation;

    void Start()
    {
        if (pivot == null)
        {
            Debug.LogError("ServoMotor script requires a child Transform assigned as pivot.");
        }
        initialRotation = pivot.localRotation.eulerAngles;

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
            // Add the angle to the initial Z rotation angle and rotate the pivot
            float newZAngle = initialRotation.z + angle;
            newZAngle = Mathf.Clamp(newZAngle, 0, maxDegree); // Ensure the angle stays within the bounds
            pivot.localRotation = Quaternion.Euler(initialRotation.x, initialRotation.y, newZAngle);
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
