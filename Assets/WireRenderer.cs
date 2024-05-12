using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WireRenderer : MonoBehaviour
{
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private float wireRadius = 0.01f;
    [SerializeField] private Color wireColor = Color.white;
    [SerializeField] private int segments = 10;  // Number of segments to create the curve
    [SerializeField] private float curveHeight = 1f;  // Maximum height of the curve
    [SerializeField] private float verticalSegmentHeight = 0.015f; 

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        SetupLineRenderer();
    }

    private void Update()
    {
        DrawCurvedLine();
    }

     private void SetupLineRenderer()
    {
        // lineRenderer.positionCount = segments + 1; // One point per segment plus one for the end
        lineRenderer.startWidth = wireRadius;
        lineRenderer.endWidth = wireRadius;
        // lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Set a default material
        lineRenderer.startColor = wireColor;
        lineRenderer.endColor = wireColor;
    }

    private void DrawCurvedLine()
    {
        Vector3[] curvePoints = new Vector3[segments + 1];
        Vector3 startPointPos = startPoint.position;
        Vector3 endPointPos = endPoint.position;

        // Calculate the distance between start and end points
        float distance = Vector3.Distance(startPointPos, endPointPos);

        // Set minimum length and height constants
        float minimumLength = 0.1f; // Length below which the curve height is minimum
        float minimumHeight = 0.035f; // Minimum curve height

        // Calculate dynamic curve height
        float dynamicCurveHeight = minimumHeight;
        if (distance > minimumLength) {
            // Only increase curve height beyond the minimum length
            dynamicCurveHeight += Mathf.Log(1 + (distance - minimumLength)) * 0.125f;
        }

        // Ensure dynamic curve height does not fall below the minimum height
        dynamicCurveHeight = Mathf.Max(dynamicCurveHeight, minimumHeight);

        // Create control points using local upward direction of each endpoint and dynamic curve height
        Vector3 controlPoint1 = startPointPos + startPoint.up * dynamicCurveHeight;
        Vector3 controlPoint2 = endPointPos + endPoint.up * dynamicCurveHeight;

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            curvePoints[i] = CalculateCubicBezierPoint(t, startPointPos, controlPoint1, controlPoint2, endPointPos);
        }

        lineRenderer.positionCount = segments + 1;
        lineRenderer.SetPositions(curvePoints);
    }

    private Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uuu = u * u * u;
        float ttt = tt * t;

        Vector3 p = uuu * p0; // first term
        p += 3 * u * u * t * p1; // second term
        p += 3 * u * tt * p2; // third term
        p += ttt * p3; // fourth term

        return p;
    }
}
