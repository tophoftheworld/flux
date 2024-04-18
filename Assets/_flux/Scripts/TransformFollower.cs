using UnityEngine;

public class TransformFollower : MonoBehaviour
{
    public Transform target; // The target object to follow (Arduino)
    public float followSpeed = 1.0f; // Speed at which the IDE follows the target

    private Vector3 offset; // Offset from the target at the start\
    
    private Vector3 lastPosition;
    private Vector3 lastTargetPosition;

    void Start()
    {
        // Calculate the initial offset based on current positions
        if (target != null)
        {
            offset = transform.position - target.position;
            lastPosition = transform.position;
            lastTargetPosition = target.position;
        }
        else
        {
            Debug.LogError("TransformFollower: No target set for the script.");
        }
    }

    void Update()
    {
        if (target != null)
        {
            if(target.position == lastTargetPosition)
            {
                if(transform.position != lastPosition)
                {
                    offset = transform.position - target.position;
                }
            }
            else
            {
                Vector3 desiredPosition = target.position + offset;
                desiredPosition.y = transform.position.y;

                transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
            }
        }
        
        lastPosition = transform.position;
        lastTargetPosition = target.position;
    }
}
