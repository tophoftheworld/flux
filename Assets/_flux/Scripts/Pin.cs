using Oculus.Interaction;
using UnityEngine;

public class Pin : MonoBehaviour
{
    [SerializeField, Interface(typeof(IInteractableView))]
    private UnityEngine.Object _interactableView;
    private IInteractableView InteractableView { get; set; }

    public string PinNumber;

    private void Awake()
    {
        InteractableView = _interactableView as IInteractableView;
        
        if (InteractableView != null)
        {
            InteractableView.WhenStateChanged += HandleStateChange;
        }
    }

    private void OnDestroy()
    {
        if (InteractableView != null)
        {
            InteractableView.WhenStateChanged -= HandleStateChange;
        }
    }

    private void HandleStateChange(InteractableStateChangeArgs args)
    {
        if (InteractableView.State == InteractableState.Hover)
        {
            PinManager.Instance.ShowPinNumber(this);
        }
        else
        {
            PinManager.Instance.HidePinNumber(this);
        }
    }
}