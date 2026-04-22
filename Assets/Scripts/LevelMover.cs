using UnityEngine;

public class LevelMover : MonoBehaviour
{
    public Transform xrOrigin; // Nhân vật người chơi
    public Transform nextLocation; // Điểm đến mới

    public void TeleportPlayer()
    {
        xrOrigin.position = nextLocation.position;
        // Xoay người chơi hướng về phía bàn bắn súng
        xrOrigin.rotation = nextLocation.rotation;
    }
}