using UnityEngine;

[System.Serializable]
public class SafeZone
{
    public float left = -1f;   // Left boundary on interaction plane
    public float right = 1f;   // Right boundary on interaction plane
    public float top = 1f;     // Top boundary on interaction plane
    public float bottom = -1f; // Bottom boundary on interaction plane
}

public class BallPicker : MonoBehaviour
{
    public Camera cam;                  // Main camera
    public LayerMask interactionLayer;  // Layer for interaction zone (plane)
    public LayerMask ballLayer;         // Layer for basketballs

    [SerializeField] private float pickupRange = 3f;
    public float forceMultiplier = 0.5f;

    [Header("Safe Zone Settings")]
    public SafeZone safeZone = new SafeZone();

    private BasketBall selectedBall;
    private Vector2 startTouchPos;
    private float startTime;

    // Interaction plane data
    private bool hasPlane = false;
    private Vector3 planeCenter;
    private Vector3 planeNormal;
    private Vector3 planeRight;
    private Vector3 planeUp;

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // Touch input
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

        // Mouse input
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

        if (Physics.Raycast(cam.ScreenPointToRay(screenPosition), out RaycastHit ballHit, 100f, ballLayer))
        {
            float distanceToBall = Vector3.Distance(cam.transform.position, ballHit.point);
            if (distanceToBall <= pickupRange)
            {
                selectedBall = ballHit.collider.GetComponent<BasketBall>();
                selectedBall?.OnTouch(screenPosition);
            }
        }
    }

    void HoldBall(Vector2 screenPosition)
    {
        if (selectedBall == null) return;
        if (!hasPlane && !SetupInteractionPlane()) return;

        Ray ray = cam.ScreenPointToRay(screenPosition);
        Plane plane = new Plane(planeNormal, planeCenter);

        if (plane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);

            // Convert to local coordinates on the plane
            Vector3 offset = hitPoint - planeCenter;
            float localX = Vector3.Dot(offset, planeRight);
            float localY = Vector3.Dot(offset, planeUp);

            // Clamp position within safe zone
            localX = Mathf.Clamp(localX, safeZone.left, safeZone.right);
            localY = Mathf.Clamp(localY, safeZone.bottom, safeZone.top);

            // Convert back to world space
            Vector3 clampedWorldPos = planeCenter + planeRight * localX + planeUp * localY;

            selectedBall.OnHold(clampedWorldPos);
        }
    }

    void ReleaseBall(Vector2 screenPosition, Vector2 dragVector, float dragDuration)
    {
        if (selectedBall == null) return;
        if (!SetupInteractionPlane()) return;

        Ray ray = cam.ScreenPointToRay(screenPosition);
        Plane plane = new Plane(planeNormal, planeCenter);

        if (plane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 direction = (hitPoint - cam.transform.position).normalized;
            Vector3 throwDirection = (direction + Vector3.up * 1.2f).normalized;

            float dragDistance = dragVector.magnitude;
            float clampedDuration = Mathf.Clamp(dragDuration, 0.05f, 0.5f);
            float dragSpeed = dragDistance / clampedDuration;
            float calculatedForce = dragSpeed * forceMultiplier;

            selectedBall.OnRelease(throwDirection, calculatedForce);
            selectedBall = null;
        }
    }

    // Detect and set up interaction plane
    bool SetupInteractionPlane()
    {
        Ray ray = cam.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f));
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, interactionLayer))
        {
            planeCenter = hit.collider.bounds.center; // Use collider center
            planeNormal = hit.normal;

            // Create right/up vectors on the plane
            planeRight = Vector3.Cross(Vector3.up, planeNormal).normalized;
            if (planeRight.sqrMagnitude < 0.001f) planeRight = cam.transform.right;
            planeUp = Vector3.Cross(planeNormal, planeRight).normalized;

            hasPlane = true;
            return true;
        }
        return false;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (hasPlane)
        {
            // Draw safe zone rectangle on the interaction plane
            Vector3 topLeft = planeCenter + planeRight * safeZone.left + planeUp * safeZone.top;
            Vector3 topRight = planeCenter + planeRight * safeZone.right + planeUp * safeZone.top;
            Vector3 bottomRight = planeCenter + planeRight * safeZone.right + planeUp * safeZone.bottom;
            Vector3 bottomLeft = planeCenter + planeRight * safeZone.left + planeUp * safeZone.bottom;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);
        }
    }
#endif
}
