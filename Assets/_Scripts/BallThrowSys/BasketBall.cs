using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BasketBall : MonoBehaviour
{
    private Rigidbody rb;
    private bool isHeld = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void OnTouch(Vector2 screenPosition)
    {
        isHeld = true;
        rb.isKinematic = true;
    }

    public void OnHold(Vector3 targetPosition)
    {
        if (!isHeld) return;

        // Move the ball while holding
        transform.position = targetPosition;
    }

    public void OnRelease(Vector3 throwDirection, float force)
    {
        if (!isHeld) return;

        isHeld = false;
        rb.isKinematic = false;
        rb.detectCollisions = true;

        rb.angularVelocity = Vector3.zero;
        transform.rotation = Quaternion.identity;

        rb.linearVelocity = throwDirection.normalized * force;
    }
}
