using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class MineFlag : MonoBehaviour
{
    public AudioClip plantClip; // Tiếng cắm cờ phập xuống đất
    public AudioClip failClip;  // Tiếng thả trượt cờ
    private XRGrabInteractable grabInteractable;
    private bool isPlanted = false;
    private bool isDuplicate = false; // Đánh dấu đây là cờ bản sao thả rơi

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        
        if (!isDuplicate && grabInteractable != null)
        {
            // Bóp cò (Trigger) để thả một bản sao cờ
            grabInteractable.activated.AddListener(OnActivated);
            // Vẫn giữ sự kiện thả tay phòng trường hợp người chơi lỡ nhả nút Grip
            grabInteractable.selectExited.AddListener(OnReleased);
        }
        else if (isDuplicate)
        {
            // Nếu là cờ bản sao (được thả bằng cò Trigger), chắc chắn nó phải tự biến mất sau 3 giây 
            // để phòng trường hợp lỗi vật lý không bắt được va chạm với mặt đất.
            Invoke("FailDrop", 3f);
        }
    }

    void OnActivated(ActivateEventArgs args)
    {
        if (isPlanted || isDuplicate) return;

        if (MineManager.Instance != null && MineManager.Instance.UseFlag())
        {
            // Tạo bản sao của lá cờ
            GameObject droppedFlag = Instantiate(gameObject, transform.position, transform.rotation);
            MineFlag flagScript = droppedFlag.GetComponent<MineFlag>();
            if (flagScript != null)
            {
                flagScript.isDuplicate = true;
            }
            
            // Xóa GrabInteractable ở bản sao để nó rơi tự do và không bị cầm nhầm lại
            XRGrabInteractable grab = droppedFlag.GetComponent<XRGrabInteractable>();
            if (grab != null) Destroy(grab);
            
            // Biến mọi Collider thành Trigger để lá cờ đi xuyên qua các vật thể (tránh kẹt)
            Collider[] colliders = droppedFlag.GetComponentsInChildren<Collider>();
            foreach (Collider c in colliders)
            {
                c.isTrigger = true;
            }

            // Đảm bảo có Rigidbody để rơi tự do
            Rigidbody rb = droppedFlag.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
        }
    }

    void OnReleased(SelectExitEventArgs args)
    {
        if (isPlanted || isDuplicate) return;

        // Nếu người chơi thả tay (không bóp cò), dùng lại logic cũ (dò tia xuống đất)
        CheckForMineRaycast();
    }

    void CheckForMineRaycast()
    {
        // Bắn một tia từ trên xuống dưới để tìm mìn
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1f))
        {
            if (hit.collider.CompareTag("Mine"))
            {
                PlantOnMine(hit.collider.gameObject);
            }
        }
    }

    // Dùng cho cờ bản sao rơi tự do va chạm vào mìn hoặc đất
    void OnTriggerEnter(Collider other)
    {
        if (isPlanted) return;
        
        if (other.CompareTag("Mine") || other.name.Contains("Mine"))
        {
            PlantOnMine(other.gameObject);
            return;
        }

        if (isDuplicate)
        {
            // Bỏ qua va chạm với Player, Tay cầm, hoặc lá cờ gốc đang cầm trên tay
            if (other.CompareTag("Player") || 
                other.transform.root.name.Contains("XR") || 
                other.transform.root.name.Contains("VR") ||
                other.name.Contains("Flag") || 
                other.GetComponent<XRGrabInteractable>() != null)
            {
                return;
            }
            FailDrop();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isPlanted) return;
        
        if (collision.collider.CompareTag("Mine") || collision.gameObject.name.Contains("Mine"))
        {
            PlantOnMine(collision.collider.gameObject);
            return;
        }

        if (isDuplicate)
        {
            // Bỏ qua va chạm với Player, Tay cầm, hoặc lá cờ gốc đang cầm trên tay
            if (collision.collider.CompareTag("Player") || 
                collision.transform.root.name.Contains("XR") || 
                collision.transform.root.name.Contains("VR") ||
                collision.gameObject.name.Contains("Flag") || 
                collision.collider.GetComponent<XRGrabInteractable>() != null)
            {
                return;
            }
            FailDrop();
        }
    }

    void FailDrop()
    {
        if (isPlanted) return;
        isPlanted = true; // Đánh dấu để khỏi gọi nhiều lần
        if (failClip != null) AudioSource.PlayClipAtPoint(failClip, transform.position);
        Destroy(gameObject);
    }

    void PlantOnMine(GameObject mineObj)
    {
        HiddenMine mine = mineObj.GetComponent<HiddenMine>();
        if (mine != null && !mine.isDefused)
        {
            // Đánh dấu để tránh gọi nhiều lần
            isPlanted = true;
            mine.Defuse(); // Hàm này tự động gọi MineManager.Instance.AddScore()

            // Phát âm thanh ghi điểm
            if (plantClip != null) AudioSource.PlayClipAtPoint(plantClip, transform.position);

            // Theo yêu cầu: xóa mìn và xóa cờ
            Destroy(mineObj);
            Destroy(gameObject);
        }
    }
}
