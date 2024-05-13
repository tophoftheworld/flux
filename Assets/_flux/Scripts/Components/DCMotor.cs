using UnityEngine;

public class DCMotor : MonoBehaviour, IOutputDevice
{
    public int pin;
    public Transform pivot;
    private ArduinoController arduinoController;
    public bool isMoving = false;

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

    void Update()
    {
        if (isMoving)
        {
            // Rotate the pivot around the Z-axis
            transform.Rotate(100 * Time.deltaTime, 0, 0);
        }
    }

    public void Rotate(bool shouldRotate)
    {
        isMoving = shouldRotate;
    }

    public void UpdatePinState(int newState)
    {
        Rotate(newState > 0);
    }
}
