using UnityEngine;

// Gắn vào NPCCanvas (World Space Canvas trên đầu NPC kunkun)
// Đảm bảo thought bubble luôn quay về phía Main Camera
public class NPCBillboard : MonoBehaviour
{
    private Camera _cam;

    private void Start()
    {
        _cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (_cam == null) return;
        transform.LookAt(transform.position + _cam.transform.rotation * Vector3.forward,
                         _cam.transform.rotation * Vector3.up);
    }
}