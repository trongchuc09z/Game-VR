using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class TeleportationActivator : MonoBehaviour
{
    public InputActionProperty teleportActivatorAction;
    public XRRayInteractor rayInteractor;
    public XRRayInteractor teleportInteractor;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        teleportInteractor.gameObject.SetActive(false);
        teleportActivatorAction.action.performed += Action_performed;
        rayInteractor.uiHoverEntered.AddListener(x => DisaleTeleporRay());
    }

    private void Action_performed(InputAction.CallbackContext obj)
    {
        if(rayInteractor && rayInteractor.IsOverUIGameObject())
        {
            return;
        }

        teleportInteractor.gameObject.SetActive(true);
    }

    public void DisaleTeleporRay()
    {
        teleportInteractor.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(teleportActivatorAction.action.WasReleasedThisFrame())
        {
            teleportInteractor.gameObject.SetActive(false);
        }
    }
}
