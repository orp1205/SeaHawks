using UnityEngine;

[System.Serializable]
public class SafeZone
{
    public float left = -1f;
    public float right = 1f;
    public float top = 1f;
    public float bottom = -1f;
}

public class BallPicker : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public LayerMask interactionLayer;
    public LayerMask ballLayer;

    [Header("Picker Settings")]
    [SerializeField] private float pickupRange = 3f;

    [Header("Safe Zone Settings")]
    public SafeZone safeZone = new SafeZone();

    [Header("Hold Offset (meters)")]
    public float holdForwardOffset = 0.5f;

    [Header("Throw Settings")]
    public float swipeThreshold = 50f;
    private float dragStartTime;
    public float dragResetTime = 0.5f;
    public float forceMax = 10f;
    public float forceMultiplier = 0.5f;

    private BasketBall selectedBall;
    private Vector2 startTouchPos;
    private float startTime;

    private bool hasPlane = false;
    private Vector3 planeCenter;
    private Vector3 planeNormal;
    private Vector3 planeRight;
    private Vector3 planeUp;

    void Update()
    {
        if (cam == null) cam = Camera.main;
        HandleInput();
    }

    void HandleInput()
    {
        // Touch
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchPos = touch.position;
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    startTouchPos = touchPos;
                    startTime = Time.time;
                    TrySelectBall(touchPos);
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    HoldBall(touchPos);
                    break;
                case TouchPhase.Ended:
                    float dragDuration = Time.time - startTime;
                    Vector2 dragVector = touchPos - startTouchPos;
                    ReleaseBall(touchPos, dragVector, dragDuration);
                    break;
            }
        }

        // Mouse
        if (Input.GetMouseButtonDown(0))
        {
            startTouchPos = Input.mousePosition;
            startTime = Time.time;
            TrySelectBall(startTouchPos);
        }
        else if (Input.GetMouseButton(0))
        {
            HoldBall(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            float dragDuration = Time.time - startTime;
            Vector2 dragVector = (Vector2)Input.mousePosition - startTouchPos;
            ReleaseBall(Input.mousePosition, dragVector, dragDuration);
        }
    }

    void TrySelectBall(Vector2 screenPosition)
    {
        if (!SetupInteractionPlane()) return;
        if (cam == null) return;

        if (Physics.Raycast(cam.ScreenPointToRay(screenPosition), out RaycastHit ballHit, 100f, ballLayer))
        {
            if (Vector3.Distance(cam.transform.position, ballHit.point) <= pickupRange)
            {
                selectedBall = ballHit.collider.GetComponent<BasketBall>();
                if (selectedBall != null)
                {
                    selectedBall.OnTouch(screenPosition);
                    startTouchPos = screenPosition;
                    dragStartTime = Time.time;
                }
            }
        }
    }

    void HoldBall(Vector2 screenPosition)
    {
        if (selectedBall == null) return;
        if (!hasPlane && !SetupInteractionPlane()) return;
        if (cam == null) cam = Camera.main;

        if (Time.time - dragStartTime > dragResetTime)
        {
            startTouchPos = screenPosition;
            dragStartTime = Time.time;
        }

        Ray ray = cam.ScreenPointToRay(screenPosition);
        Plane plane = new Plane(planeNormal, planeCenter);

        if (plane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);

            Vector3 offset = hitPoint - planeCenter;
            float localX = Mathf.Clamp(Vector3.Dot(offset, planeRight), safeZone.left, safeZone.right);
            float localY = Mathf.Clamp(Vector3.Dot(offset, planeUp), safeZone.bottom, safeZone.top);

            Vector3 clampedWorldPos = planeCenter + planeRight * localX + planeUp * localY;
            Vector3 forwardOffset = cam.transform.forward * holdForwardOffset;
            selectedBall.OnHold(clampedWorldPos + forwardOffset);
        }
    }

    void ReleaseBall(Vector2 screenPosition, Vector2 dragVector, float dragDuration)
    {
        if (selectedBall == null) return;
        if (!SetupInteractionPlane()) return;
        if (cam == null) cam = Camera.main;

        Ray ray = cam.ScreenPointToRay(screenPosition);
        Plane plane = new Plane(planeNormal, planeCenter);

        if (plane.Raycast(ray, out float enter))
        {
            // Determine throw direction based on swipe
            Vector3 camForward = cam.transform.forward;
            camForward.y = 0; // Base forward on camera's horizontal view

            // Horizontal angle from swipe X
            float horizontalFactor = Mathf.Clamp(dragVector.x / Screen.width, -0.5f, 0.5f);
            Quaternion horizontalRotation = Quaternion.Euler(0, horizontalFactor * 90f, 0);

            // Vertical arc from swipe Y
            Vector3 throwDirection = horizontalRotation * camForward + Vector3.up * selectedBall.arcHeight;
            throwDirection.Normalize();

            // Calculate force
            float verticalDrag = Mathf.Max(0, dragVector.y);
            float dragSpeed = verticalDrag / Mathf.Clamp(dragDuration, 0.05f, 0.5f);
            float calculatedForce = Mathf.Min(dragSpeed * forceMultiplier, forceMax);

            // Release
            if (verticalDrag > swipeThreshold)
                selectedBall.OnRelease(throwDirection, calculatedForce);
            else
                selectedBall.OnRelease(Vector3.zero, 0f);

            selectedBall = null;
        }
    }


    bool SetupInteractionPlane()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return false;

        Ray ray = cam.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f));
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, interactionLayer))
        {
            planeCenter = hit.collider.bounds.center;
            planeNormal = hit.normal;
            planeRight = Vector3.Cross(Vector3.up, planeNormal).normalized;
            if (planeRight.sqrMagnitude < 0.001f) planeRight = cam.transform.right;
            planeUp = Vector3.Cross(planeNormal, planeRight).normalized;
            hasPlane = true;
            return true;
        }
        return false;
    }
}
