using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera gameCamera;
    [SerializeField] private RectTransform crosshairUI;

    [Header("Look Pan (cursor-follow)")]
    [SerializeField] private float maxPanYaw = 10f;    // degrees, left/right cap
    [SerializeField] private float maxPanPitch = 10f;  // degrees, up/down cap
    [SerializeField] private float panSmoothTime = 0.3f; // higher = slower/laggier "delayed" feel
    private float currentPanYaw, currentPanPitch;
    private float panYawVelocity, panPitchVelocity; // SmoothDampAngle refs

    [Header("Sway Settings")]
    [SerializeField] private float swayAmount = 0.5f;
    [SerializeField] private float swaySpeed = 0.5f;
    private Quaternion cameraHomeRotation;

    [Header("Shoot Settings")]
    [SerializeField] private float shootRange = 100f;
    [SerializeField] private LayerMask targetLayer;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        cameraHomeRotation = gameCamera.transform.localRotation;
    }

    private void Update()
    {
        UpdateCrosshair();
        ApplyLookPan();
        ApplySway();

        if (Input.GetMouseButtonDown(0))
            HandleShoot();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void UpdateCrosshair()
    {
        if (crosshairUI != null)
            crosshairUI.position = Input.mousePosition;
    }

    private void ApplyLookPan()
    {
        // -1 (edge) .. +1 (opposite edge), 0 = screen center
        float normX = Mathf.Clamp((Input.mousePosition.x - Screen.width * 0.5f) / (Screen.width * 0.5f), -1f, 1f);
        float normY = Mathf.Clamp((Input.mousePosition.y - Screen.height * 0.5f) / (Screen.height * 0.5f), -1f, 1f);

        float targetYaw = normX * maxPanYaw;     // cursor right -> pan right
        float targetPitch = -normY * maxPanPitch; // cursor up -> pan up

        currentPanYaw = Mathf.SmoothDampAngle(currentPanYaw, targetYaw, ref panYawVelocity, panSmoothTime);
        currentPanPitch = Mathf.SmoothDampAngle(currentPanPitch, targetPitch, ref panPitchVelocity, panSmoothTime);
    }

    private void ApplySway(float instability = 0f)
    {
        float baseAmount = swayAmount * (1f + instability);
        float swayYaw = (Mathf.PerlinNoise(Time.time * swaySpeed, 0f) - 0.5f) * 2f * baseAmount;
        float swayPitch = (Mathf.PerlinNoise(0f, Time.time * swaySpeed) - 0.5f) * 2f * baseAmount;

        float finalPitch = currentPanPitch + swayPitch;
        float finalYaw = currentPanYaw + swayYaw;

        gameCamera.transform.localRotation = cameraHomeRotation * Quaternion.Euler(finalPitch, finalYaw, 0f);
    }

    private void HandleShoot()
    {
        Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, shootRange, targetLayer))
            Debug.Log("Hit: " + hit.collider.name);
        else
            Debug.Log("Miss");
    }
}