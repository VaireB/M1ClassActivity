using System.Collections.Generic;
using UnityEngine;

public class AStar : MonoBehaviour
{
    public LayerMask obstacleMask;
    public float nodeRadius;
    public float moveSpeed;
    public GameObject playerCharacter;

    Node[,] grid;
    Vector3 targetPosition;

    void Start()
    {
        if (playerCharacter == null)
        {
            Debug.LogError("Player character object is not assigned to the AStar script!");
            return;
        }

        CreateGrid();
    }

    void CreateGrid()
    {
        // Calculate grid size based on obstacle layer
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        Renderer[] renderers = FindObjectsOfType<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }
        Vector3 gridSize = bounds.size;

        // Calculate number of nodes
        int gridSizeX = Mathf.RoundToInt(gridSize.x / nodeRadius);
        int gridSizeY = Mathf.RoundToInt(gridSize.z / nodeRadius);
        grid = new Node[gridSizeX, gridSizeY];

        // Create nodes
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridSize.x / 2 - Vector3.forward * gridSize.z / 2;
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeRadius + nodeRadius / 2) + Vector3.forward * (y * nodeRadius + nodeRadius / 2);
                bool walkable = !Physics.CheckSphere(worldPoint, nodeRadius, obstacleMask);
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, obstacleMask))
            {
                targetPosition = hit.point;
                FindPath(playerCharacter.transform.position, targetPosition);
            }
        }

        MoveTowardsTarget();
    }

    void MoveTowardsTarget()
    {
        if (targetPosition != Vector3.zero)
        {
            Vector3 direction = (targetPosition - playerCharacter.transform.position).normalized;
            playerCharacter.transform.position += direction * Time.deltaTime * moveSpeed;

            // Check if the player has reached the target position
            if (Vector3.Distance(playerCharacter.transform.position, targetPosition) < nodeRadius)
            {
                targetPosition = Vector3.zero;
            }
        }
    }

    void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = NodeFromWorldPoint(startPos);
        Node targetNode = NodeFromWorldPoint(targetPos);

        if (startNode == null || targetNode == null)
        {
            Debug.LogWarning("Start node or target node is null. Unable to find path.");
            return;
        }

        // Mark all nodes as walkable initially
        foreach (Node node in grid)
        {
            node.walkable = true;
        }

        // Mark the node not clicked as unwalkable
        Node notClickedNode = NodeFromWorldPoint(targetPos);
        if (notClickedNode != null)
        {
            notClickedNode.walkable = false;
        }

        // Implement A* pathfinding algorithm here
    }

    Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        if (grid == null || grid.Length == 0)
        {
            Debug.LogWarning("Grid array is not initialized or empty. Unable to find node.");
            return null;
        }

        float percentX = (worldPosition.x + transform.position.x + grid.GetLength(0) / 2) / grid.GetLength(0);
        float percentY = (worldPosition.z + transform.position.z + grid.GetLength(1) / 2) / grid.GetLength(1);
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((grid.GetLength(0) - 1) * percentX);
        int y = Mathf.RoundToInt((grid.GetLength(1) - 1) * percentY);

        if (x < 0 || x >= grid.GetLength(0) || y < 0 || y >= grid.GetLength(1))
        {
            Debug.LogWarning("World position is outside of grid bounds. Unable to find node.");
            return null;
        }

        return grid[x, y];
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        return distX + distY;
    }

    public class Node
    {
        public bool walkable;
        public Vector3 worldPosition;
        public int gridX;
        public int gridY;
        public int gCost;
        public int hCost;
        public Node parent;

        public Node(bool walkable, Vector3 worldPosition, int gridX, int gridY)
        {
            this.walkable = walkable;
            this.worldPosition = worldPosition;
            this.gridX = gridX;
            this.gridY = gridY;
        }

        public int fCost
        {
            get { return gCost + hCost; }
        }
    }
}
