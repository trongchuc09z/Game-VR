using UnityEngine;
using Unity.XR.CoreUtils;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(XROrigin))]
public class SyncColliderWithCamera : MonoBehaviour
{
    private CharacterController characterController;
    private XROrigin xrOrigin;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        xrOrigin = GetComponent<XROrigin>();
    }

    void Update()
    {
        if (characterController != null && xrOrigin != null && xrOrigin.Camera != null)
        {
            // Lấy vị trí tương đối của Camera (kính VR) so với điểm gốc
            Vector3 cameraLocalPos = xrOrigin.CameraInOriginSpacePos;
            
            // Liên tục cập nhật tâm của Collider để luôn nằm ngay dưới đầu người chơi
            characterController.center = new Vector3(
                cameraLocalPos.x, 
                characterController.center.y, 
                cameraLocalPos.z
            );
        }
    }
}
