using TMPro;
using UnityEngine;

public class PinManager : MonoBehaviour
{
    // public static PinManager Instance { get; private set; }
    [SerializeField] private TextMeshProUGUI pinNumberText;

    private Pin currentlyHoveredPin = null;

    private void Awake()
    {
        // if (Instance != null && Instance != this)
        // {
        //     Destroy(gameObject);
        // }
        // else
        // {
        //     Instance = this;
        // }

        pinNumberText.gameObject.SetActive(false);
    }

    public void ShowPinNumber(Pin pin)
    {
        currentlyHoveredPin = pin;
        pinNumberText.text = $"{pin.PinNumber}";
        pinNumberText.gameObject.SetActive(true);
    }

    public void HidePinNumber(Pin pin)
    {
        if (pin == currentlyHoveredPin)
        {
            pinNumberText.gameObject.SetActive(false);
            currentlyHoveredPin = null;
        }
    }
}
