using UnityEngine;

public class BallCircleSelector : MonoBehaviour
{
    [Header("References")]
    public BasketBallSO basketBallSO;   // Scriptable Object containing the list of balls
    public GameObject ballBasePrefab;   // Base prefab for each ball (sphere/model)

    [Header("Layout Settings")]
    public float radius = 3f;           // Radius of the circle
    public float minScale = 0.8f;       // Scale for non-selected balls
    public float selectedScale = 1.2f;  // Scale for selected ball
    public float tiltAngle = 15f;

    [Header("Control Settings")]
    public float dragSpeed = 5f;        // Rotation speed when dragging

    private Vector3 lastMousePos;
    private float scrollOffset = 0f;
    private float angleStep;
    private float startAngle;

    void Start()
    {
        if (basketBallSO == null || basketBallSO.basketBallDataList.Count == 0)
        {
            Debug.LogError("BasketBallSO is missing or empty!");
            return;
        }

        int totalBalls = basketBallSO.basketBallDataList.Count;
        angleStep = 360f / totalBalls;
        startAngle = 0f;

        InitBalls();
    }

    void InitBalls()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        for (int i = 0; i < basketBallSO.basketBallDataList.Count; i++)
        {
            GameObject ballObj = ballBasePrefab != null ?
                Instantiate(ballBasePrefab, transform) : GameObject.CreatePrimitive(PrimitiveType.Sphere);

            var col = ballObj.GetComponent<Collider>();
            if (col) Destroy(col);

            var data = basketBallSO.basketBallDataList[i];
            if (data.ballMaterial != null)
            {
                // Search all children for a MeshRenderer
                MeshRenderer[] renderers = ballObj.GetComponentsInChildren<MeshRenderer>();
                foreach (var rend in renderers)
                {
                    rend.material = data.ballMaterial;
                }
            }

            ballObj.name = data.name;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            lastMousePos = Input.mousePosition;
        else if (Input.GetMouseButton(0))
        {
            float deltaX = Input.mousePosition.x - lastMousePos.x;
            scrollOffset += deltaX * Time.deltaTime * dragSpeed;
            lastMousePos = Input.mousePosition;
        }

        ArrangeBalls();
    }

    void ArrangeBalls()
    {
        int totalBalls = basketBallSO.basketBallDataList.Count;
        int selectedIndex = GetClosestBallIndex();

        for (int i = 0; i < totalBalls; i++)
        {
            float angle = startAngle + (i - scrollOffset) * angleStep;
            float rad = angle * Mathf.Deg2Rad;

            Vector3 pos = new Vector3(Mathf.Sin(rad) * radius, 0, Mathf.Cos(rad) * radius);

            // Apply tilt around X axis
            Quaternion tilt = Quaternion.Euler(tiltAngle, 0, 0);
            pos = tilt * pos;
            Transform ball = transform.GetChild(i);
            ball.localPosition = pos;

            // Scale up selected ball
            if (i == selectedIndex)
                ball.localScale = Vector3.one * selectedScale;
            else
                ball.localScale = Vector3.one * minScale;

            ball.LookAt(Camera.main.transform.position);
        }
    }

    /// <summary>
    /// Finds the index of the ball closest to the front (Z positive).
    /// </summary>
    int GetClosestBallIndex()
    {
        int closestIndex = 0;
        float closestZ = float.MinValue;

        for (int i = 0; i < transform.childCount; i++)
        {
            float z = transform.GetChild(i).localPosition.z;
            if (z > closestZ)
            {
                closestZ = z;
                closestIndex = i;
            }
        }

        return closestIndex;
    }
}
