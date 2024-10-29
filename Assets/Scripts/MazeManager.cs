using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeManager : MonoBehaviour
{
    public enum MazeAlgorithm { DFS, Kruskal }
    public MazeAlgorithm selectedAlgorithm;
    private IMazeGenerator mazeGenerator;

    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject startPrefab;  // Prefab for start marker
    public GameObject goalPrefab;   // Prefab for goal marker

    private int[,] maze;
    private Vector2Int startPos;
    private Vector2Int goalPos;

    public int width = 10;
    public int height = 10;

    private static MazeManager instance;
    public static MazeManager Instance => instance;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        switch (selectedAlgorithm)
        {
            case MazeAlgorithm.DFS:
                mazeGenerator = gameObject.AddComponent<DFSMazeGenerator>();
                break;
            case MazeAlgorithm.Kruskal:
                mazeGenerator = gameObject.AddComponent<KruskalMazeGenerator>();
                break;
        }

        maze = mazeGenerator.GenerateMaze(width, height);
        startPos = new Vector2Int(1, 1);
        goalPos = new Vector2Int(width - 2, height - 2);

        AgentMazeRefinement agentRefinement = GetComponent<AgentMazeRefinement>();
        if (agentRefinement != null)
        {
            agentRefinement.InitializeWithMazeData(maze, startPos, goalPos, width, height);
        }

        UpdateVisualMaze(maze);  // Draw initial maze
    }

    public void PlaceStartAndGoal()
    {
        // Instantiate start and goal prefabs at their respective positions
        Instantiate(startPrefab, new Vector3(startPos.x, startPos.y, 0), Quaternion.identity, transform);
        Instantiate(goalPrefab, new Vector3(goalPos.x, goalPos.y, 0), Quaternion.identity, transform);

        Debug.Log("Start and Goal have been placed.");
    }

    // Method to update the maze visuals
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
