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
    private HashSet<PointableUnityEventWrapper> activeInteractions; // Track active interactions

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
        activeInteractions = new HashSet<PointableUnityEventWrapper>();
    }

    public void SpawnTool(Tools tool) {
        if (toolPrefabDict.ContainsKey(tool) && spawnPoint != null) {
            if (currentToolInstance != null && activeInteractions.Count == 0) {
                // No active interactions, safe to replace the tool
                Destroy(currentToolInstance);
            }
            
            // Spawn the new tool and assign the component to track interaction
            currentToolInstance = Instantiate(toolPrefabDict[tool], spawnPoint.position, Quaternion.identity);
            SetupInteractionTracking(currentToolInstance);
        } else {
            Debug.LogWarning("ToolSpawner: No prefab or spawn point set for tool " + tool.ToString());
        }
    }

    private void SetupInteractionTracking(GameObject tool) {
        foreach (var wrapper in tool.GetComponentsInChildren<PointableUnityEventWrapper>()) {
            wrapper.WhenSelect.AddListener(evt => HandleSelect(wrapper));
            wrapper.WhenUnselect.AddListener(evt => HandleUnselect(wrapper));
        }
    }

    private void HandleSelect(PointableUnityEventWrapper wrapper) {
        activeInteractions.Add(wrapper);
    }

    private void HandleUnselect(PointableUnityEventWrapper wrapper) {
        activeInteractions.Remove(wrapper);
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
