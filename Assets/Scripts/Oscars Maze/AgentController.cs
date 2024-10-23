using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    public Sprite agentSprite;   // The sprite for the agent
    public Sprite trailSprite;   // The sprite for the trail
    public float tileScale = 1f; // The size of each tile (e.g., 1 unit per tile)
    public float moveDelay = 0.1f;  // Delay between each move to visualize movement

    private Vector2Int goalPosition;  // The position of the goal
    private bool[,] maze;             // The maze array (walkable or not)
    private bool[,] visited;          // Array to track visited positions
    private Stack<Vector2Int> pathStack = new Stack<Vector2Int>(); // DFS stack

    private GameObject agentObj;      // The GameObject for the agent

    public UIController uiController;  // Reference to the UIController for the game over UI
    private Coroutine dfsCoroutine;   // Reference to the DFS coroutine to stop it on reset

    void Start()
    {
        // Initialize the agent's start position
        agentObj = new GameObject("Agent");
        agentObj.transform.position = Vector3.zero;
        SpriteRenderer renderer = agentObj.AddComponent<SpriteRenderer>();
        renderer.sprite = agentSprite;
        renderer.sortingOrder = 2;  // Ensure agent is rendered above the maze
    }

    // Initialize the maze data and goal position (called from the OscarsMazeGenerator)
    public void InitializeMaze(bool[,] generatedMaze, Vector2Int goalPos)
    {
        maze = generatedMaze;
        goalPosition = goalPos;

        // Initialize the visited array to track which tiles have been visited
        visited = new bool[maze.GetLength(0), maze.GetLength(1)];
        Debug.Log("Maze initialized, starting DFS...");

        // Start the DFS movement after initialization
        Vector2Int startPosition = new Vector2Int(1, 1);  // Starting from (1,1)

        // Start the DFS coroutine and store the reference to stop it if needed
        if (dfsCoroutine != null)
        {
            StopCoroutine(dfsCoroutine);  // Stop the previous coroutine if it's running
        }
        dfsCoroutine = StartCoroutine(MoveAgentDFS(startPosition));  // Start DFS movement
    }

    // DFS movement through the maze
    IEnumerator MoveAgentDFS(Vector2Int startPosition)
    {
        if (maze == null)
        {
            Debug.LogError("Maze data not initialized!");
            yield break;
        }

        pathStack.Push(startPosition);
        visited[startPosition.x, startPosition.y] = true;  // Mark start as visited

        while (pathStack.Count > 0)
        {
            Vector2Int currentPosition = pathStack.Pop();

            // Move the agent to the current position (tile-based movement)
            Vector3 worldPosition = new Vector3(currentPosition.x * tileScale, currentPosition.y * tileScale, 0);
            agentObj.transform.position = worldPosition;

            // Leave a trail behind the agent
            CreateTrail(worldPosition);

            // Wait for a short time to visualize the movement
            yield return new WaitForSeconds(moveDelay);

            // Get the valid, walkable neighbors (avoid re-exploring them)
            List<Vector2Int> neighbors = GetValidNeighbors(currentPosition);

            // Add unvisited neighbors to the stack
            foreach (Vector2Int neighbor in neighbors)
            {
                if (!visited[neighbor.x, neighbor.y])
                {
                    pathStack.Push(neighbor);
                    visited[neighbor.x, neighbor.y] = true;  // Mark as visited when pushed onto the stack
                }
            }

            // Check if the agent has reached the goal
            if (currentPosition == goalPosition)
            {
                Debug.Log("Goal Reached!");
                uiController.ShowGameOverScreen();  // Show the game over screen
                yield break;
            }
        }

        Debug.LogError("Could not find the goal!");
    }

    // Creates a trail sprite at the agent's current position
    void CreateTrail(Vector3 position)
    {
        GameObject trailObj = new GameObject("Trail");
        trailObj.transform.position = position;
        SpriteRenderer trailRenderer = trailObj.AddComponent<SpriteRenderer>();
        trailRenderer.sprite = trailSprite;
        trailRenderer.sortingOrder = 1;  // Trail is below the agent
    }

    // Get valid neighboring positions (up, down, left, right) for DFS traversal
    List<Vector2Int> GetValidNeighbors(Vector2Int currentPos)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        // Define directions for neighbors: up, down, left, right
        Vector2Int[] directions = {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        // Check each direction for a valid neighbor (it must be walkable)
        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighborPos = currentPos + direction;

            // The tile must be in bounds and walkable, but we allow walking over visited tiles
            if (IsInBounds(neighborPos) && maze[neighborPos.x, neighborPos.y])
            {
                neighbors.Add(neighborPos);
            }
        }

        return neighbors;
    }

    // Check if the position is within the maze bounds
    bool IsInBounds(Vector2Int position)
    {
        return position.x >= 0 && position.x < maze.GetLength(0) && position.y >= 0 && position.y < maze.GetLength(1);
    }
}
