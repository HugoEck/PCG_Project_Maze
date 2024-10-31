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
    private Vector2Int goalPos;

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

        // Generate maze and get start and goal positions
        maze = mazeGenerator.GenerateMaze(width, height);
        startPos = mazeGenerator.GetStartPosition();
        goalPos = mazeGenerator.GetGoalPosition();

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
}
