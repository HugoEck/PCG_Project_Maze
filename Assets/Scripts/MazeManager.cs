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
        // Generate the maze and configure start and goal
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

        maze = mazeGenerator.GenerateMaze(width, height);
        startPos = new Vector2Int(1, 1);
        goalPos = new Vector2Int(width - 3, height - 3);

        AgentMazeRefinement agentRefinement = GetComponent<AgentMazeRefinement>();
        if (agentRefinement != null)
        {
            agentRefinement.InitializeWithMazeData(maze, startPos, goalPos, width, height);
        }

        UpdateVisualMaze(maze);
        PlaceStartAndGoal();
        AdjustCameraToFitMaze();  // Ensure camera shows the whole maze
    }

    private void AdjustCameraToFitMaze()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        // Calculate the center of the maze
        Vector3 mazeCenter = new Vector3(width / 1f - 0.5f, height / 2f - 0.5f, -10);

        // Set camera position to the maze center
        mainCamera.transform.position = mazeCenter;

        // Adjust the orthographic size based on maze dimensions
        float aspectRatio = (float)Screen.width / Screen.height;
        float verticalSize = height / 2f + 1; // Add some padding
        float horizontalSize = (width / 2f + 1) / aspectRatio;

        mainCamera.orthographicSize = Mathf.Max(verticalSize, horizontalSize);
    }

    public void PlaceStartAndGoal()
    {
        Instantiate(startPrefab, new Vector3(startPos.x, startPos.y, 0), Quaternion.identity, transform);
        Instantiate(goalPrefab, new Vector3(goalPos.x, goalPos.y, 0), Quaternion.identity, transform);

        Debug.Log("Start and Goal have been placed.");
    }

    public void UpdateVisualMaze(int[,] maze)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        for (int x = 0; x < maze.GetLength(0); x++)
        {
            for (int y = 0; y < maze.GetLength(1); y++)
            {
                Vector2 position = new Vector2(x, y);
                GameObject prefab = maze[x, y] == 0 ? wallPrefab : floorPrefab;
                Instantiate(prefab, position, Quaternion.identity, transform);
            }
        }
    }
}
