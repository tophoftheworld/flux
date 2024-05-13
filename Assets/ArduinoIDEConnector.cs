using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArduinoIDEConnector : MonoBehaviour
{
    public GameObject idePrefab; // Assign the prefab in the inspector
    public GameObject spawnedIDE;

    void Start()
    {
        // Check if the IDE prefab has already been spawned
        // spawnedIDE = GameObject.Find(idePrefab.name + "(Clone)");
        if (spawnedIDE == null)
        {
            // Calculate the position 0.1 meters behind the current object
            Vector3 spawnPosition = transform.position - transform.forward * 0.1f;

            // Instantiate the prefab at the calculated position
            spawnedIDE = Instantiate(idePrefab, spawnPosition, Quaternion.identity);

            // Optionally, you can set the spawned object as a child of the current object
            spawnedIDE.transform.SetParent(transform, true);

            if(spawnedIDE.GetComponent<TransformFollower>().target = transform);
        }
    }
}
