using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class UltrasonicSensor : MonoBehaviour
{
    public ArduinoController arduinoController;
    public string port; // Port on the Arduino where the pin would be located
    public int pin;
    public float range = 5.0f; // Maximum range of the ultrasonic sensor
    public LayerMask detectionLayer; // Layer on which the sensor detects objects

    private GameObject lastDetectedObject = null; // To keep track of the last detected object
    private Quaternion localRotationOffset = Quaternion.Euler(90, 0, -90);
    public float width = 0.2f;

    void Update()
    {
        RaycastHit hit;
        Vector3 adjustedDirection = transform.rotation * localRotationOffset * (Vector3.forward * -1);
        Vector3 leftStart = transform.position - transform.right * width;
        Vector3 rightStart = transform.position + transform.right * width;

        if (CheckForObstacle(out hit, transform.position, adjustedDirection) ||
            CheckForObstacle(out hit, leftStart, adjustedDirection) ||
            CheckForObstacle(out hit, rightStart, adjustedDirection))
        {
            // lastDetectedObject = hit.collider.gameObject; // Update the last detected object
            ObjectDetected();
        } else {
            NoObjectDetected();
        }
    }

    // Simulate an ultrasonic sensor detection using raycast
    private bool CheckForObstacle(out RaycastHit hit, Vector3 start, Vector3 direction)
    {
        Debug.DrawRay(start, direction * range, Color.red);
        if (Physics.Raycast(start, direction, out hit, range, detectionLayer))
        {
            Debug.Log("Obstacle detected at distance: " + hit.distance + " by " + hit.collider.gameObject.name);
            return true; // Obstacle within range
        }
        return false; // No obstacle within range
    }

    private void ObjectDetected()
    {
        // Simulate sending a signal when the ultrasonic sensor detects something within range
        arduinoController.SendStateChange(port, pin, true);
    }

    private void NoObjectDetected()
    {
        // Simulate sending a signal when no object is detected within range
        arduinoController.SendStateChange(port, pin, false);
    }
}
