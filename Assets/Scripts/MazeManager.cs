using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeManager : MonoBehaviour
{
    public enum MazeAlgorithm { DFS, Kruskal, Prim }
    public MazeAlgorithm selectedAlgorithm;
    private IMazeGenerator mazeGenerator;

    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject startPrefab;
    public GameObject goalPrefab;
    public GameObject playerPrefab; // Reference to the player prefab

    private int[,] maze;
    private Vector2Int startPos;
    public Vector2Int goalPos;

    public int width;
    public int height;

    private static MazeManager instance;
    public static MazeManager Instance => instance;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Select and initialize the maze generator
        switch (selectedAlgorithm)
        {
            case MazeAlgorithm.DFS:
                mazeGenerator = gameObject.AddComponent<DFSMazeGenerator>();
                break;
            case MazeAlgorithm.Kruskal:
                mazeGenerator = gameObject.AddComponent<KruskalMazeGenerator>();
                break;
            case MazeAlgorithm.Prim:
                mazeGenerator = gameObject.AddComponent<PrimMazeGenerator>();
                break;
        }

        // Generate maze and get start position from generator
        maze = mazeGenerator.GenerateMaze(width, height);
        startPos = mazeGenerator.GetStartPosition();

        // Find furthest point from start to set as goal

        // Initialize AgentMazeRefinement if it exists
        AgentMazeRefinement agentRefinement = GetComponent<AgentMazeRefinement>();
        if (agentRefinement != null)
        {
            agentRefinement.InitializeWithMazeData(maze, startPos, goalPos, width, height);
        }

        // Create visual representation of the maze and place start, goal, and player
        UpdateVisualMaze(maze);
        PlaceStartAndGoal();
        PlacePlayer();
        AdjustCameraToFitMaze();  // Ensure camera shows the whole maze
        goalPos = FindFurthestPathFromStart(startPos);
    }

    private void AdjustCameraToFitMaze()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        // Calculate the maze center for 2D positioning
        Vector3 mazeCenter = new Vector3((width - 1) / 2f, (height - 1) / 2f, -10);

        // Set camera position to the maze center
        mainCamera.transform.position = mazeCenter;

        // Adjust orthographic size based on maze dimensions
        float aspectRatio = (float)Screen.width / Screen.height;
        float verticalSize = height / 2f + 1; // Add some padding
        float horizontalSize = (width / 2f + 1) / aspectRatio;

        mainCamera.orthographicSize = Mathf.Max(verticalSize, horizontalSize);
    }

    public void PlaceStartAndGoal()
    {
        // Instantiate start and goal prefabs at start and goal positions
        Instantiate(startPrefab, new Vector3(startPos.x, startPos.y, 0), Quaternion.identity, transform);
        Instantiate(goalPrefab, new Vector3(goalPos.x, goalPos.y, 0), Quaternion.identity, transform);

        Debug.Log("Start and Goal have been placed.");
    }

    private void PlacePlayer()
    {
        // Instantiate the player at the start position
        GameObject player = Instantiate(playerPrefab, new Vector3(startPos.x, startPos.y, -1), Quaternion.identity);
        Debug.Log("Player placed at start position.");
    }

    public void UpdateVisualMaze(int[,] maze)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject); // Clear old maze elements
        }

        for (int x = 0; x < maze.GetLength(0); x++)
        {
            for (int y = 0; y < maze.GetLength(1); y++)
            {
                Vector2 position = new Vector2(x, y);
                GameObject prefab = maze[x, y] == 0 ? wallPrefab : floorPrefab;
                GameObject tile = Instantiate(prefab, position, Quaternion.identity, transform);

                // Tag the tile as "Floor" for player navigation if it's a walkable path
                if (maze[x, y] == 1)
                {
                    tile.tag = "Floor";
                }
            }
        }
    }

    // Method to find the furthest floor tile from a given start position
    public Vector2Int FindFurthestPathFromStart(Vector2Int start)
    {

        if (maze == null)
        {
            Debug.LogError("Maze is null in FindFurthestPathFromStart");
            return start; // Return start or handle the error
        }

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, int> distances = new Dictionary<Vector2Int, int>();

        queue.Enqueue(start);
        distances[start] = 0;

        Vector2Int furthestCell = start;
        int maxDistance = 0;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            int currentDistance = distances[current];

            // Explore neighbors in 4 cardinal directions
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (var dir in directions)
            {
                Vector2Int neighbor = current + dir;

                // Check if the neighbor is within bounds, is a floor tile, and hasn't been visited
                if (IsInBounds(neighbor) && maze[neighbor.x, neighbor.y] == 1 && !distances.ContainsKey(neighbor))
                {
                    distances[neighbor] = currentDistance + 1;
                    queue.Enqueue(neighbor);

                    // Update furthest cell if this is the longest path found
                    if (distances[neighbor] > maxDistance)
                    {
                        maxDistance = distances[neighbor];
                        furthestCell = neighbor;
                    }
                }
            }
        }

        return furthestCell;
    }

    private bool IsInBounds(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < maze.GetLength(0) && cell.y >= 0 && cell.y < maze.GetLength(1);
    }

    public void UpdateGoalAfterDeadEndProcessing(Vector2Int start)
    {
        // Find the furthest accessible path cell from start
        goalPos = FindFurthestPathFromStart(start);
        Debug.Log($"Updated Goal Position after dead-end processing: {goalPos}");
    }
}
