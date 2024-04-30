using UnityEngine;
using UnityEngine.UI;

public class LED : MonoBehaviour
{
    private Image ledImage;
    private MeshRenderer meshRenderer;

    void Start()
    {
        ledImage = GetComponent<Image>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (ledImage == null && meshRenderer == null)
        {
            Debug.LogError("LED script requires an Image or MeshRenderer component on the same GameObject.");
        }
    }

    public void SetBrightness(int brightness)
    {
        if (ledImage != null)
        {
            // Map the brightness from 0-255 to 0-1 for the alpha channel
            float alpha = brightness / 255.0f;
            ledImage.color = new Color(ledImage.color.r, ledImage.color.g, ledImage.color.b, alpha);
        }
        else if (meshRenderer != null)
        {
            meshRenderer.enabled = brightness > 0;
        }
    }
}
