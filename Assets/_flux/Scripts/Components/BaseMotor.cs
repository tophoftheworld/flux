using UnityEngine;

public class BaseMotor : MonoBehaviour, IOutputDevice
{
    public int pin;
    public Transform pivot;
    private ArduinoController arduinoController;
    public int direction = 0;

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
        if (direction != 0)
        {
            // Rotate the pivot around the Z-axis
            pivot.Rotate(0, 0, direction * 30 * Time.deltaTime);
        }
    }


    public void UpdatePinState(int newState)
    {
        if(newState < 64){
            direction = 0;
        } else if(newState > 192){
            direction = 1;
        } else {
            direction = -1;
        }
    }
}
