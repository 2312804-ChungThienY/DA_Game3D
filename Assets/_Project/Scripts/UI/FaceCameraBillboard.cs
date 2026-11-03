using UnityEngine;

// Nhãn chữ 3D luôn quay về camera (số chuông, ký hiệu) để đọc được từ mọi hướng.
public class FaceCameraBillboard : MonoBehaviour
{
    private Transform cam;

    private void LateUpdate()
    {
        if (cam == null)
        {
            if (Camera.main == null) return;
            cam = Camera.main.transform;
        }
        transform.rotation = Quaternion.LookRotation(transform.position - cam.position);
    }
}
