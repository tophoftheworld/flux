using UnityEngine;

public class ArmMotor : MonoBehaviour, IOutputDevice
{
    public int pin;
    public Transform pivot1;
    public Transform pivot2;
    public float maxDegreePivot1 = 180f; // Public variable for maximum degree of pivot1
    public float maxDegreePivot2 = 180f; // Public variable for maximum degree of pivot2
    private ArduinoController arduinoController;
    private Quaternion initialRotationPivot1;
    private Quaternion initialRotationPivot2;

    void Start()
    {
        if (pivot1 == null || pivot2 == null)
        {
            Debug.LogError("ArmMotor script requires two child Transforms assigned as pivots.");
        }

        initialRotationPivot1 = pivot1.localRotation;
        initialRotationPivot2 = pivot2.localRotation;

        arduinoController = FindObjectOfType<ArduinoController>();
        if (arduinoController != null)
        {
            arduinoController.RegisterDevice(this, pin);
        }
    }

    public void RotatePivotsByAngle(float anglePivot1, float anglePivot2)
    {
        if (pivot1 != null && pivot2 != null)
        {
            // Rotate pivot1
            float newZAnglePivot1 = Mathf.Clamp(anglePivot1, 0, maxDegreePivot1); // Ensure the angle stays within the bounds
            pivot1.localRotation = initialRotationPivot1 * Quaternion.Euler(0, 0, newZAnglePivot1);

            // Rotate pivot2
            float newZAnglePivot2 = Mathf.Clamp(anglePivot2, 0, maxDegreePivot2); // Ensure the angle stays within the bounds
            pivot2.localRotation = initialRotationPivot2 * Quaternion.Euler(0, 0, newZAnglePivot2);
        }
        else
        {
            Debug.LogError("No pivots assigned for the ArmMotor.");
        }
    }

    public void UpdatePinState(int newState)
    {
        // Map 0-255 to 0-maxDegree for each pivot
        float anglePivot1 = Map(newState, 0, 255, 0, maxDegreePivot1);
        float anglePivot2 = Map(newState, 0, 255, 0, maxDegreePivot2);
        RotatePivotsByAngle(anglePivot1, anglePivot2); // Assuming both pivots should rotate by mapped angles
    }

    private float Map(int value, int fromSource, int toSource, float fromTarget, float toTarget)
    {
        return (value - fromSource) / (float)(toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
    }
}
