using UnityEngine;

public class LEDController : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer; // Assign this in the editor

    [SerializeField] private Color defaultColor; // This color will be modified to change the hue

    private bool isOn = false; // Track whether the LED is on or off

    void Start()
    {
        if (meshRenderer == null)
        {
            Debug.LogError("MeshRenderer is not assigned on " + gameObject.name);
            return;
        }

        // Randomize the color hue but keep the same saturation and brightness
        defaultColor = meshRenderer.material.color;
        defaultColor = RandomizeHue(defaultColor);
        meshRenderer.material.color = defaultColor;
        meshRenderer.material.SetColor("_EmissionColor", defaultColor);

        UpdateEmission();
    }

    private Color RandomizeHue(Color color)
    {
        float hue, saturation, value;
        Color.RGBToHSV(color, out hue, out saturation, out value);
        hue = Random.Range(0f, 1f); // Randomize the hue
        return Color.HSVToRGB(hue, saturation, value);
    }

    // Call this method to toggle the LED on or off
    public void ToggleLED()
    {
        isOn = !isOn;
        UpdateEmission();
    }

    private void UpdateEmission()
    {
        if (isOn)
        {
            meshRenderer.material.EnableKeyword("_EMISSION");
            meshRenderer.material.SetColor("_EmissionColor", defaultColor);
        }
        else
        {
            meshRenderer.material.DisableKeyword("_EMISSION");
            meshRenderer.material.SetColor("_EmissionColor", Color.black);
        }
    }
}
