using UnityEngine;

public class SC_HpBar : MonoBehaviour
{
    Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        // UI가 항상 카메라를 바라보게 합니다.
        transform.LookAt(transform.position + cam.forward);
    }
}