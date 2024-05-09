using UnityEngine;
using UnityEngine.UI;

public class LED : MonoBehaviour, IOutputDevice
{
    public int pin;
    private Image ledImage;
    private MeshRenderer meshRenderer;

    private ArduinoController arduinoController;

    void Start()
    {
        ledImage = GetComponent<Image>();
        meshRenderer = GetComponent<MeshRenderer>();

        ArduinoController arduinoController = FindObjectOfType<ArduinoController>();
        if (arduinoController != null)
        {
            arduinoController.RegisterDevice(this, pin);
        }

        if (ledImage == null && meshRenderer == null)
        {
            Debug.LogError("LED script requires an Image or MeshRenderer component on the same GameObject.");
        }
         // Ensure the material is ready to handle transparency
        if (meshRenderer != null)
        {
            meshRenderer.material = new Material(meshRenderer.material);
            meshRenderer.material.SetFloat("_Mode", 2);  // Sets the material to Fade mode
            meshRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            meshRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            meshRenderer.material.SetInt("_ZWrite", 0);
            meshRenderer.material.DisableKeyword("_ALPHATEST_ON");
            meshRenderer.material.EnableKeyword("_ALPHABLEND_ON");
            meshRenderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            meshRenderer.material.renderQueue = 3000;
        }
    }

    public void SetBrightness(int brightness)
    {
        if (ledImage != null)
        {
            float alpha = brightness / 255.0f;
            ledImage.color = new Color(ledImage.color.r, ledImage.color.g, ledImage.color.b, alpha);
        }
        else if (meshRenderer != null)
        {
            float alpha = brightness / 255.0f;
            Color currentColor = meshRenderer.material.color;
            meshRenderer.material.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
        }
    }

    public void UpdatePinState(int newState)
    {
        SetBrightness(newState);
    }
}
