using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class CableRenderer : MonoBehaviour
{
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private float wireRadius = 0.01f;
    [SerializeField] private int sides = 6;

    private MeshFilter meshFilter;
    private Mesh mesh;
    private Vector3[] vertices;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;
        vertices = new Vector3[(sides + 1) * 2];

        GenerateWire();
    }

    private void Update()
    {
        UpdateWire();
    }

    private void GenerateWire()
    {
        // Initialization of vertices, triangles and uvs...
        UpdateWire(); // Set initial vertex positions
    }

    private void UpdateWire()
    {
        Vector3 startLocal = startPoint.position - transform.position;
        Vector3 endLocal = endPoint.position - transform.position;
        Quaternion rotationToDirection = Quaternion.FromToRotation(Vector3.forward, endLocal - startLocal);

        for (int i = 0; i <= sides; i++)
        {
            float angle = i * 2 * Mathf.PI / sides;
            Vector3 normal = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            normal = rotationToDirection * normal;

            vertices[i] = startLocal + normal * wireRadius; // Bottom vertices
            vertices[i + sides + 1] = endLocal + normal * wireRadius; // Top vertices
        }

        // Assign vertices to the mesh and recalculate normals
        mesh.vertices = vertices;

        int[] triangles = new int[sides * 6];
        for (int i = 0, j = 0; i < sides; i++, j += 6)
        {
            int next = (i + 1) % (sides + 1);

            // First triangle - clockwise winding
            triangles[j + 0] = next + sides + 1;
            triangles[j + 1] = next;
            triangles[j + 2] = i;

            // Second triangle - clockwise winding
            triangles[j + 3] = i;
            triangles[j + 4] = i + sides + 1;
            triangles[j + 5] = next + sides + 1;
        }

        mesh.triangles = triangles; // Assign triangles to the mesh
        mesh.RecalculateNormals(); // Recalculate normals
    }
}
