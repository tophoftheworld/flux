using UnityEngine;
using UnityEngine.EventSystems;

public class ToolSpawnButton : MonoBehaviour {
    public float hoverDepth = -25f;
    public float pressedDepth = -10f;
    public float duration = 0.25f;
    public ToolSpawner.Tools toolType;

    private bool isHovering = false;
    private Vector3 initialPosition;

    // void Awake() {
    //     initialPosition = transform.localPosition;
    // }

    // public void OnPointerEnter(PointerEventData eventData) {
    //     isHovering = true;
    //     LeanTween.moveLocalZ(gameObject, hoverDepth, duration).setEase(LeanTweenType.easeInOutQuad);
    // }

    // public void OnPointerExit(PointerEventData eventData) {
    //     isHovering = false;
    //     LeanTween.moveLocalZ(gameObject, pressedDepth, duration).setEase(LeanTweenType.easeInOutQuad);
    // }

    // public void OnPointerClick(PointerEventData eventData) {
    //     LeanTween.moveLocalZ(gameObject, pressedDepth, duration).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() => {
    //         ToolSpawner.Instance.SpawnTool(toolType);
    //         if (isHovering) {
    //             LeanTween.moveLocalZ(gameObject, hoverDepth, duration).setEase(LeanTweenType.easeInOutQuad);
    //         } else {
    //             LeanTween.moveLocalZ(gameObject, initialPosition.z, duration).setEase(LeanTweenType.easeInOutQuad);
    //         }
    //     });
    // }

    public void SpawnTool()
    {
        ToolSpawner.Instance.SpawnTool(toolType);
        Debug.Log("Calling Tool Spawner");
    }
}
