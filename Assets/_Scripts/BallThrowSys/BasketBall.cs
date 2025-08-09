using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BasketBall : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Physics Parameters")]
    public float ballMass = 0.62f;     // kg
    public float ballRadius = 0.12f;   // meters
    public float dragCoefficient = 0.47f;
    public float airDensity = 1.225f;
    public float spinFactor = 0.0004f;
    public Vector3 initialSpin = new Vector3(0, -30f, 0); // deg/sec
    public float arcHeight = 1.2f;     // upward throw adjustment

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = ballMass;
    }

    public void OnTouch(Vector2 screenPosition)
    {
        rb.isKinematic = true;
    }

    public void OnHold(Vector3 targetPosition)
    {
        transform.position = targetPosition;
    }

    public void OnRelease(Vector3 throwDirection, float force)
    {
        rb.isKinematic = false;
        rb.detectCollisions = true;

        if (force > 0f)
        {
            rb.angularVelocity = initialSpin * Mathf.Deg2Rad;
            transform.rotation = Quaternion.identity;
            rb.linearVelocity = throwDirection.normalized * force;
        }
    }

    private void FixedUpdate()
    {
        if (!rb.isKinematic && Application.isPlaying)
        {
            ApplyAirDrag();
            ApplyMagnusEffect();
        }
    }

    private void ApplyAirDrag()
    {
        Vector3 velocity = rb.linearVelocity;
        float speed = velocity.magnitude;
        if (speed > 0.01f)
        {
            float area = Mathf.PI * ballRadius * ballRadius;
            float dragForceMag = 0.5f * airDensity * speed * speed * dragCoefficient * area;
            Vector3 dragForce = -velocity.normalized * dragForceMag;
            rb.AddForce(dragForce);
        }
    }

    private void ApplyMagnusEffect()
    {
        Vector3 velocity = rb.linearVelocity;
        if (velocity.magnitude > 0.01f && rb.angularVelocity.magnitude > 0.01f)
        {
            Vector3 magnusForce = spinFactor * Vector3.Cross(rb.angularVelocity, velocity);
            rb.AddForce(magnusForce, ForceMode.Force);
        }
    }
}
