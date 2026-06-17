using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    [Header("Target Pusat Rotasi")]
    [Tooltip("Titik tengah papan catur Anda. Sesuaikan angka ini jika putaran terasa melenceng.")]
    public Vector3 targetPosition = new Vector3(3.5f, 0f, 3.5f);

    [Header("Pengaturan Jarak (Zoom)")]
    public float distance = 8.0f;
    public float minDistance = 3.0f;
    public float maxDistance = 15.0f;
    public float zoomSpeed = 5.0f;

    [Header("Kecepatan Putar & Pindah")]
    public float xSpeed = 250.0f;
    public float ySpeed = 120.0f;
    public float panSpeed = 15.0f;

    [Header("Batas Sudut (Rotasi)")]
    public float yMinLimit = 10f; // Tidak bisa tembus ke bawah meja
    public float yMaxLimit = 85f; // Maksimal hampir tegak lurus dari atas
    
    [Tooltip("Nyalakan ini jika tidak ingin kamera bisa memutar 360 derajat")]
    public bool useXLimits = false;
    public float xMinLimit = -45f;
    public float xMaxLimit = 45f;

    [Header("Batas Area Pindah (Pan)")]
    [Tooltip("Nyalakan ini agar kamera tidak bisa digeser jauh menghilang dari papan")]
    public bool usePanLimits = true;
    public float minPanX = -2f;
    public float maxPanX = 9f;
    public float minPanZ = -2f;
    public float maxPanZ = 9f;

    [Header("Sudut Awal Kustom (Opsional)")]
    [Tooltip("Jika dicentang, kamera akan mulai menggunakan sudut X, Y, dan Z di bawah ini daripada menghitung dari posisi kamera di editor.")]
    public bool useManualStartAngle = false;
    public float startAngleX = 180f;
    public float startAngleY = 85f;
    public float startAngleZ = 180f;

    private float x = 0.0f;
    private float y = 0.0f;
    private float z = 0.0f;

    public bool isFocusing = false;
    private Vector3 originalTargetPosition;
    private float originalDistance;
    public bool hasZoomedIn = false;

    private void Start()
    {
        if (useManualStartAngle)
        {
            x = startAngleX;
            y = startAngleY;
            z = startAngleZ;
        }
        else
        {
            // Hitung jarak dan sudut (yaw/pitch) awal berdasarkan posisi kamera di editor relatif terhadap targetPosition
            Vector3 dir = transform.position - targetPosition;
            distance = dir.magnitude;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);

            float horizontalDistance = new Vector2(dir.x, dir.z).magnitude;
            y = Mathf.Atan2(dir.y, horizontalDistance) * Mathf.Rad2Deg;
            x = Mathf.Atan2(-dir.x, -dir.z) * Mathf.Rad2Deg;
            z = transform.eulerAngles.z; // Gunakan nilai Z (roll) bawaan dari Editor
        }

        // Putar kamera 180 derajat jika bermain online sebagai pemain PUTIH.
        if (Photon.Pun.PhotonNetwork.IsConnected && PlayerRole.IsWhitePlayer())
        {
            x += 180f;
        }

        // Terapkan posisi awal
        UpdateCameraTransform();
    }

    private void LateUpdate()
    {
        if (isFocusing) return; // Kunci kontrol manual jika kamera sedang bergerak otomatis

        bool isInputDetected = false;

        // Check for touch input (Mobile)
        if (Input.touchCount > 0)
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Moved)
                {
                    // Rotate using 1-finger drag. touch.deltaPosition is in pixels.
                    // Scale it by a factor to make it feel natural (0.15f works well).
                    float touchRotateSpeed = 0.15f;
                    x += touch.deltaPosition.x * touchRotateSpeed;
                    y -= touch.deltaPosition.y * touchRotateSpeed;
                    isInputDetected = true;
                }
            }
            else if (Input.touchCount == 2)
            {
                // Zoom using 2-finger pinch.
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                if (Mathf.Abs(deltaMagnitudeDiff) > 0.01f)
                {
                    float touchZoomSpeed = 0.02f;
                    distance += deltaMagnitudeDiff * touchZoomSpeed;
                    distance = Mathf.Clamp(distance, minDistance, maxDistance);
                    isInputDetected = true;
                }

                // Pan using 2-finger drag (moving both fingers in the same direction)
                Vector2 touchDelta = (touchZero.deltaPosition + touchOne.deltaPosition) * 0.5f;
                if (touchDelta.magnitude > 0.01f)
                {
                    float touchPanSpeed = 0.001f;
                    float panX = -touchDelta.x * panSpeed * touchPanSpeed;
                    float panY = -touchDelta.y * panSpeed * touchPanSpeed;

                    // Cari arah kanan dan depan dari kamera
                    Vector3 right = transform.right;
                    Vector3 forward = transform.forward;
                    right.y = 0;
                    forward.y = 0;
                    right.Normalize();
                    forward.Normalize();

                    // Geser titik tengah (targetPosition)
                    targetPosition += (right * panX) + (forward * panY);
                    isInputDetected = true;
                }
            }
        }
        else // Fallback to mouse/keyboard controls (PC / Editor)
        {
            // 1. Zoom menggunakan Scroll Wheel (Roda Mouse)
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scrollInput) > 0.01f)
            {
                distance -= scrollInput * zoomSpeed;
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
                isInputDetected = true;
            }

            // 2. Putar menggunakan Klik Kanan (Mouse 1)
            if (Input.GetMouseButton(1)) 
            {
                x += Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
                y -= Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;
                isInputDetected = true;
            }

            // 3. Geser/Pindah (Pan) menggunakan Klik Tengah (Mouse 2 / Scroll Click)
            if (Input.GetMouseButton(2))
            {
                float panX = -Input.GetAxis("Mouse X") * panSpeed * Time.deltaTime;
                float panY = -Input.GetAxis("Mouse Y") * panSpeed * Time.deltaTime;

                // Cari arah kanan dan depan dari kamera
                Vector3 right = transform.right;
                Vector3 forward = transform.forward;

                // Ratakan dengan lantai (Hilangkan sumbu Y agar tidak terbang/menembus lantai saat digeser)
                right.y = 0;
                forward.y = 0;
                right.Normalize();
                forward.Normalize();

                // Geser titik tengah (targetPosition)
                targetPosition += (right * panX) + (forward * panY);
                isInputDetected = true;
            }
        }

        // Jika ada pergerakan, perbarui posisi kamera
        if (isInputDetected)
        {
            UpdateCameraTransform();
        }
    }

    private void UpdateCameraTransform()
    {
        if (useXLimits)
        {
            x = ClampAngle(x, xMinLimit, xMaxLimit);
        }
        
        y = ClampAngle(y, yMinLimit, yMaxLimit);

        if (usePanLimits)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minPanX, maxPanX);
            targetPosition.z = Mathf.Clamp(targetPosition.z, minPanZ, maxPanZ);
        }

        Quaternion rotation = Quaternion.Euler(y, x, z);
        Vector3 position = rotation * new Vector3(0.0f, 0.0f, -distance) + targetPosition;

        transform.rotation = rotation;
        transform.position = position;
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
            
        return Mathf.Clamp(angle, min, max);
    }

    // ============================================
    // SISTEM KONTROL OTOMATIS (UNTUK EFEK SINEMATIK)
    // ============================================
    
    public void FocusOn(Vector3 focusPosition, float focusDistance, System.Action onComplete)
    {
        originalTargetPosition = targetPosition;
        originalDistance = distance;
        hasZoomedIn = true;
        StartCoroutine(FocusCoroutine(focusPosition, focusDistance, onComplete));
    }

    public void ResetFocus(System.Action onComplete)
    {
        if (!hasZoomedIn) 
        {
            onComplete?.Invoke();
            return;
        }
        
        hasZoomedIn = false;
        StartCoroutine(FocusCoroutine(originalTargetPosition, originalDistance, onComplete));
    }

    private System.Collections.IEnumerator FocusCoroutine(Vector3 destPos, float destDist, System.Action onComplete)
    {
        isFocusing = true;
        
        Vector3 startPos = targetPosition;
        float startDist = distance;
        float elapsed = 0f;
        float duration = 1.2f; // Durasi kamera bergerak

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            
            targetPosition = Vector3.Lerp(startPos, destPos, t);
            distance = Mathf.Lerp(startDist, destDist, t);
            
            UpdateCameraTransform();
            yield return null;
        }

        targetPosition = destPos;
        distance = destDist;
        UpdateCameraTransform();
        
        isFocusing = false;
        onComplete?.Invoke();
    }
}
