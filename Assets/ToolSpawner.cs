using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;

public class ToolSpawner : MonoBehaviour {
    private static ToolSpawner _instance;
    public static ToolSpawner Instance {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<ToolSpawner>();
                if (_instance == null) {
                    GameObject go = new GameObject("ToolSpawner");
                    _instance = go.AddComponent<ToolSpawner>();
                }
            }
            return _instance;
        }
    }

    [System.Serializable]
    public class ToolPrefab {
        public Tools toolType;
        public GameObject prefab;
    }

    public List<ToolPrefab> toolPrefabs;
    private Dictionary<Tools, GameObject> toolPrefabDict = new Dictionary<Tools, GameObject>();

    private GameObject currentToolInstance; // Currently active tool instance
    private bool toolHasBeenInteractedWith = false; // True if the tool has been interacted with

    public Transform spawnPoint; // Position to spawn the tools at

    void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(gameObject);
        } else {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        foreach (ToolPrefab toolPrefab in toolPrefabs) {
            if (toolPrefab.prefab != null && !toolPrefabDict.ContainsKey(toolPrefab.toolType)) {
                toolPrefabDict.Add(toolPrefab.toolType, toolPrefab.prefab);
            }
        }
    }

    public void SpawnTool(Tools tool) {
        if (toolPrefabDict.ContainsKey(tool) && spawnPoint != null) {
            if (currentToolInstance != null && !toolHasBeenInteractedWith) {
                Destroy(currentToolInstance); // Replace tool only if it has not been interacted with
            }
            
            currentToolInstance = Instantiate(toolPrefabDict[tool], spawnPoint.position, Quaternion.identity);
            SetupInteractionTracking(currentToolInstance);
        } else {
            Debug.LogWarning("ToolSpawner: No prefab or spawn point set for tool " + tool.ToString());
        }
    }

    private void SetupInteractionTracking(GameObject tool) {
        var wrappers = tool.GetComponentsInChildren<PointableUnityEventWrapper>();
        if (wrappers.Length > 0) {
            foreach (var wrapper in wrappers) {
                wrapper.WhenSelect.AddListener((PointerEvent evt) => toolHasBeenInteractedWith = true);
            }
        } else {
            toolHasBeenInteractedWith = true; // Assume interaction if no wrappers are found
        }
    }

    public enum Tools {
        JumperWire,
        LEDWhite,
        LEDRed,
        LEDGreen,
        LEDBlue,
        LEDYellow,
        Arduino,
        Breadboard,
        ServoMotor,
        DCMotor,
        PushButton
    }
}