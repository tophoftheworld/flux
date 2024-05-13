using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour, IOutputDevice
{
    public DCMotor tr; // Top right motor
    public DCMotor tl; // Top left motor
    public DCMotor br; // Bottom right motor
    public DCMotor bl; // Bottom left motor

    public float fullSpeed = 10f; // Full speed for the car
    public float rotationSpeed = 5f; // Speed for rotation
    public float speedFactor = 0.5f; // Factor to reduce speed when rotating
    private float speedMultiplier = 1f;

    private Rigidbody rb;
    public LayerMask groundLayer; // Layer for ground check
    public float groundCheckDistance = 0.1f;
    public float uprightTolerance = 0.5f;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (IsOnGround()&& IsUpright())
        {
            Vector3 movement = Vector3.zero;
            Vector3 rotation = Vector3.zero;

            // Check the movement status of each motor
            bool isTRMoving = tr.isMoving;
            bool isTLMoving = tl.isMoving;
            bool isBRMoving = br.isMoving;
            bool isBLMoving = bl.isMoving;

            // Calculate the number of motors moving
            int movingCount = 0;
            if (isTRMoving) movingCount++;
            if (isTLMoving) movingCount++;
            if (isBRMoving) movingCount++;
            if (isBLMoving) movingCount++;

            // Full movement forward if all motors are moving
            if (movingCount == 4)
            {
                movement = transform.forward * fullSpeed * Time.deltaTime;
            }
            else if (movingCount > 0)
            {
                // Adjust speed based on the number of motors moving
                float adjustedSpeed = fullSpeed * (movingCount / 4f);

                // Determine rotation based on imbalance between left and right sides
                int leftMoving = 0;
                int rightMoving = 0;
                if (isTLMoving) rightMoving++;
                if (isBLMoving) rightMoving++;
                if (isTRMoving) leftMoving++;
                if (isBRMoving) rightMoving++;

                // Rotate towards the side with fewer active motors if there's an imbalance
                if (leftMoving > rightMoving)
                {
                    rotation = Vector3.up * -rotationSpeed * Time.deltaTime * ((leftMoving - rightMoving) / 2f);
                    adjustedSpeed *= speedFactor;
                }
                else if (rightMoving > leftMoving)
                {
                    rotation = Vector3.up * rotationSpeed * Time.deltaTime * ((rightMoving - leftMoving) / 2f);
                    adjustedSpeed *= speedFactor;
                }

                // Move forward at adjusted speed
                movement = transform.forward * adjustedSpeed * Time.deltaTime;
            }

            transform.Translate(movement * speedMultiplier, Space.World);
            transform.Rotate(rotation * speedMultiplier);
        }
    }

    public void UpdatePinState(int newState)
    {
        speedMultiplier = newState / 255f;
        tr.SetSpeedMultiplier(speedMultiplier);
        tl.SetSpeedMultiplier(speedMultiplier);
        br.SetSpeedMultiplier(speedMultiplier);
        bl.SetSpeedMultiplier(speedMultiplier);

    }

    private bool IsOnGround()
    {
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }
    
    private bool IsUpright()
    {
        return Vector3.Dot(transform.up, Vector3.up) > uprightTolerance;
    }

}
