using UnityEngine;

public class Billboard : MonoBehaviour
{
    private void Start()
    {
        m_Camera = Camera.main;
    }

    private void LateUpdate()
    {
        float cachedZ = transform.rotation.eulerAngles.z; // Save Z before LookAt
        Vector3 cameraForward = m_Camera.transform.rotation * Vector3.forward;
        cameraForward.y = 0f;
        if (cameraForward.sqrMagnitude > 0.001f)
        {
            transform.LookAt(transform.position + cameraForward);
        }
        // Reapply the Z rotation that LookAt wiped
        transform.rotation = Quaternion.Euler(
            transform.rotation.eulerAngles.x,
            transform.rotation.eulerAngles.y,
            cachedZ
        );
    }

    private Camera m_Camera;
}