using Oculus.Interaction;
using UnityEngine;
using Oculus.Interaction;

public class Pin : MonoBehaviour
{
    [SerializeField, Interface(typeof(IInteractableView))]
    private UnityEngine.Object _interactableView;
    private IInteractableView InteractableView { get; set; }

    public string PinNumber;
    public int value;

    private PinManager pinManager;
    private SnapInteractable snapInteractable;

    private void Awake()
    {
        InteractableView = _interactableView as IInteractableView;
        
        if (InteractableView != null)
        {
            InteractableView.WhenStateChanged += HandleStateChange;
        }

        if(snapInteractable == null)
        {
            snapInteractable = GetComponentInChildren<SnapInteractable>();
            snapInteractable.InjectRigidbody(GetComponentInParent<Rigidbody>());
        }

        if(pinManager == null)
        {
            pinManager = GetComponentInParent<PinManager>();
        }
    }

    private void Start()
    {
        // if(snapInteractable.Rigidbody == null)
        // {
        //     snapInteractable.InjectRigidbody(GetComponentInParent<Rigidbody>());
        // }
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
        
        if(pinManager == null)
        {
            return;
        }

        if (InteractableView.State == InteractableState.Hover)
        {
            pinManager.ShowPinNumber(this);
        }
        else
        {
            pinManager.HidePinNumber(this);
        }
    }
}