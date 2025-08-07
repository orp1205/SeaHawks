using UnityEngine;

public class BallPicker : MonoBehaviour
{
    public Camera cam;
    public LayerMask interactionLayer;
    public LayerMask ballLayer;

    [SerializeField] private float pickupRange = 3f;

    public float forceMultiplier = 0.5f; // Adjusts the throw force sensitivity

    private BasketBall selectedBall;
    private Vector2 startTouchPos;
    private float startTime;

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // Handle mobile touch input
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

        // Handle mouse input for desktop
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

    // Attempts to pick a ball from screen position
    void TrySelectBall(Vector2 screenPosition)
    {
        if (!RaycastToPlane(screenPosition, out Vector3 planeHitPoint)) return;

        Vector3 direction = (planeHitPoint - cam.transform.position).normalized;
        Ray rayToBall = new Ray(cam.transform.position, direction);

        if (Physics.Raycast(rayToBall, out RaycastHit ballHit, 100f, ballLayer))
        {
            float distanceToBall = Vector3.Distance(cam.transform.position, ballHit.point);

            if (distanceToBall <= pickupRange)
            {
                selectedBall = ballHit.collider.GetComponent<BasketBall>();
                selectedBall?.OnTouch(screenPosition);
            }
            else
            {
                Debug.Log("Ball too far to pick up.");
            }
        }
    }

    // Updates ball position while holding
    void HoldBall(Vector2 screenPosition)
    {
        if (selectedBall == null) return;

        if (RaycastToPlane(screenPosition, out Vector3 planeHitPoint))
        {
            Vector3 camToPlane = (planeHitPoint - cam.transform.position).normalized;
            float distanceBehindPlane = 5f;
            Vector3 behindPlanePosition = planeHitPoint + camToPlane * distanceBehindPlane;
            selectedBall.OnHold(behindPlanePosition);
        }
    }

    // Releases the ball with calculated throw force
    void ReleaseBall(Vector2 screenPosition, Vector2 dragVector, float dragDuration)
    {
        if (selectedBall == null) return;

        if (RaycastToPlane(screenPosition, out Vector3 planeHitPoint))
        {
            Vector3 direction = (planeHitPoint - cam.transform.position).normalized;

            // Add a slight upward arc to the throw
            Vector3 throwDirection = (direction + Vector3.up * 0.8f).normalized;

            float dragDistance = dragVector.magnitude;
            float clampedDuration = Mathf.Clamp(dragDuration, 0.05f, 0.5f);
            float dragSpeed = dragDistance / clampedDuration;

            float calculatedForce = dragSpeed * forceMultiplier;

            selectedBall.OnRelease(throwDirection, calculatedForce);
            selectedBall = null;
        }
    }

    // Raycasts from screen position to the interaction plane
    bool RaycastToPlane(Vector2 screenPosition, out Vector3 hitPoint)
    {
        Ray ray = cam.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, interactionLayer))
        {
            hitPoint = hit.point;
            return true;
        }

        hitPoint = Vector3.zero;
        return false;
    }
}
