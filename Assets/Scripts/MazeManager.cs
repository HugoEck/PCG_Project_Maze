using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MazeManager : MonoBehaviour
{
    public enum MazeAlgorithm { PickAlgorithm, DFS, Kruskal, Prim }
    public MazeAlgorithm selectedAlgorithm;
    private IMazeGenerator mazeGenerator;
    private MainMenuManager mainMenuManager;


    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject startPrefab;
    public GameObject goalPrefab;
    public GameObject agentPrefab;
    public GameObject playerPrefab;

    private bool toggleApplied = false;

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
        selectedAlgorithm = (MazeAlgorithm)PlayerPrefs.GetInt("algorithmType", (int)MazeAlgorithm.DFS);
        Debug.Log("Loaded Algorithm Type: " + selectedAlgorithm);
        width = PlayerPrefs.GetInt("mazeWidth", 20);
        height = PlayerPrefs.GetInt("mazeHeight", 20);

        //Generate Maze By Select Algorithm
        switch (selectedAlgorithm)
        {
            case MazeAlgorithm.PickAlgorithm:

            break;
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
        startPos = mazeGenerator.GetStartPosition();

        // Create visual representation of the maze
        UpdateVisualMaze(maze);

        // Initialize and place goal
        SetGoalPosition();
        PlaceStartAndGoal();

        // Initialize AgentMazeRefinement if present
        AgentMazeRefinement agentRefinement = GetComponent<AgentMazeRefinement>();
        if (agentRefinement != null)
        {
            agentRefinement.InitializeWithMazeData(maze, startPos, goalPos);
            Debug.Log("Agent Refinement Initialized");
        }

        //After agents finish handling dead ends, place player
        StartCoroutine(WaitForAgentsThenPlacePlayer(agentRefinement));


        //-- Agent/Player Swapping 

        // Find and cache the MainMenuManager instance
        mainMenuManager = FindObjectOfType<MainMenuManager>();

        // Instantiate prefabs as needed
        if (agentPrefab != null)
        {
            Instantiate(agentPrefab);
        }
        if (playerPrefab != null)
        {
            Instantiate(playerPrefab);
        }

        // Start the coroutine to toggle prefabs with a slight delay
        StartCoroutine(TogglePrefabsWithDelay());
    }

    IEnumerator WaitForAgentsThenPlacePlayer(AgentMazeRefinement agentRefinement)
    {
        // Wait until agent refinement is done (or if there are no agents)
        while (agentRefinement != null && !agentRefinement.IsDone)
        {
            yield return null;
        }


        PlacePlayer();
        AdjustCameraToFitMaze();  
        if (agentPrefab != null)
        {
            GameObject agentInstance = Instantiate(agentPrefab, new Vector3(startPos.x, startPos.y, -1), Quaternion.identity);
            MazeSolvingAgent agentScript = agentInstance.GetComponent<MazeSolvingAgent>();
            if (agentScript != null)
            {
                agentScript.InitializeWithMazeData(maze, startPos, goalPos, width, height);
            }
        }
    }


    private void AdjustCameraToFitMaze()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector3 mazeCenter = new Vector3((width - 1) / 1f, (height - 1) / 2f, -10);

        mainCamera.transform.position = mazeCenter;


        float aspectRatio = (float)Screen.width / Screen.height;
        float verticalSize = height / 2f + 1; 
        float horizontalSize = (width / 2f + 1) / aspectRatio;

        mainCamera.orthographicSize = Mathf.Max(verticalSize, horizontalSize);
    }

    public void PlaceStartAndGoal()
    {
        Instantiate(startPrefab, new Vector3(startPos.x, startPos.y, 0), Quaternion.identity, transform);
        Instantiate(goalPrefab, new Vector3(goalPos.x, goalPos.y, 0), Quaternion.identity, transform);

        Debug.Log("Start and Goal have been placed.");
    }

    private void PlacePlayer()
    {
        GameObject player = Instantiate(playerPrefab, new Vector3(startPos.x, startPos.y, -1), Quaternion.identity);
        Debug.Log("Player placed at start position.");
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
                GameObject tile = Instantiate(prefab, position, Quaternion.identity, transform);

                if (maze[x, y] == 1)
                {
                    tile.tag = "Floor";
                }
            }
        }
    }

    private void SetGoalPosition()
    {
        int wallThickness = (width % 2 == 0) ? 3 : 2;
        goalPos = new Vector2Int(width - wallThickness, height - wallThickness);

        Debug.Log($"Goal Position set to: {goalPos}");
    }

    public Vector2Int FindFurthestPathFromStart(Vector2Int start)
    {
        if (maze == null)
        {
            Debug.LogError("Maze is null in FindFurthestPathFromStart");
            return start; 
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

            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (var dir in directions)
            {
                Vector2Int neighbor = current + dir;

                if (IsInBounds(neighbor) && maze[neighbor.x, neighbor.y] == 1 && !distances.ContainsKey(neighbor))
                {
                    distances[neighbor] = currentDistance + 1;
                    queue.Enqueue(neighbor);


                    //longest path
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
        goalPos = FindFurthestPathFromStart(start);
        Debug.Log($"Updated Goal Position after dead-end processing: {goalPos}");
    }

    private IEnumerator TogglePrefabsWithDelay()
    {
        yield return new WaitForSeconds(0.1f);

        TogglePrefabs();
    }

    private void TogglePrefabs()
    {
        bool includeAgentPrefab = PlayerPrefs.GetInt("IncludeAgentPrefab", 0) == 1;

        GameObject[] playerClones = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] agentClones = GameObject.FindGameObjectsWithTag("Agent");

        if (includeAgentPrefab)
        {
            foreach (GameObject player in playerClones)
            {
                player.SetActive(false);
            }
            foreach (GameObject agent in agentClones)
            {
                agent.SetActive(true);
            }
        }
        else
        {
            foreach (GameObject player in playerClones)
            {
                player.SetActive(true);
            }
            foreach (GameObject agent in agentClones)
            {
                agent.SetActive(false);
            }
        }
    }
}
