using UnityEngine;

public class ServoMotor : MonoBehaviour
{
    public Transform pivot; // The child transform that should rotate

    void Start()
    {
        if (pivot == null)
        {
            Debug.LogError("ServoMotor script requires a child Transform assigned as pivot.");
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
}
