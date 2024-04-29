using UnityEngine;

public class PlaneGrid : MonoBehaviour
{
    public GameObject groundObject; // Assign the ground object (plane) in the Inspector
    public LayerMask obstacleMask;
    public float nodeDensity = 1f; // Density of nodes per unit distance on the ground
    public GameObject nodePrefab; // Prefab for visualizing nodes (optional)

    Node[,] grid;

    void Start()
    {
        if (groundObject == null)
        {
            Debug.LogError("Ground object not assigned to the PlaneGrid script!");
            return;
        }

        CreateGrid();
    }

    void CreateGrid()
    {
        MeshFilter meshFilter = groundObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("MeshFilter component not found on the ground object.");
            return;
        }

        Mesh mesh = meshFilter.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        Bounds bounds = mesh.bounds;

        float nodeDiameter = 1f / nodeDensity;
        int gridSizeX = Mathf.RoundToInt(bounds.size.x * nodeDensity);
        int gridSizeY = Mathf.RoundToInt(bounds.size.z * nodeDensity);

        grid = new Node[gridSizeX, gridSizeY];

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = groundObject.transform.TransformPoint(new Vector3(
                    bounds.min.x + x * nodeDiameter + nodeDiameter / 2f,
                    bounds.min.y,
                    bounds.min.z + y * nodeDiameter + nodeDiameter / 2f
                ));

                bool walkable = !Physics.CheckBox(worldPoint, new Vector3(nodeDiameter / 2f, 0.1f, nodeDiameter / 2f), Quaternion.identity, obstacleMask);
                grid[x, y] = new Node(walkable, worldPoint, x, y);

                // Optionally, instantiate node visualizations
                if (nodePrefab != null)
                {
                    Instantiate(nodePrefab, worldPoint, Quaternion.identity, transform);
                }
            }
        }
    }

    // Define the Node class
    public class Node
    {
        public bool walkable;
        public Vector3 worldPosition;
        public int gridX;
        public int gridY;

        public Node(bool walkable, Vector3 worldPosition, int gridX, int gridY)
        {
            this.walkable = walkable;
            this.worldPosition = worldPosition;
            this.gridX = gridX;
            this.gridY = gridY;
        }
    }
}
