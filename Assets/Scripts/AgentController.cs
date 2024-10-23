using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    public Sprite agentSprite;  // The agent's sprite
    public Sprite trailSprite;  // The trail's sprite
    public float tileScale = 1f;  // The scale of each tile
    public float moveDelay = 0.1f;  // Delay between each move for visualization

    private Vector2Int goalPosition;  // Position of the goal
    private bool[,] maze;  // The maze structure (walkable or not)
    private bool[,] visited;  // To track visited positions
    private Stack<Vector2Int> pathStack = new Stack<Vector2Int>();  // DFS path

    private GameObject agentObj;  // The agent game object

    void Start()
    {
        // Hardcoded agent start position
        Vector2Int startPosition = new Vector2Int(1, 1);
        Vector3 spawnPosition = new Vector3(startPosition.x * tileScale, startPosition.y * tileScale, 0);

        // Create the agent at the specified position
        agentObj = new GameObject("Agent");
        agentObj.transform.position = spawnPosition;
        SpriteRenderer renderer = agentObj.AddComponent<SpriteRenderer>();
        renderer.sprite = agentSprite;
        renderer.sortingOrder = 2;

        // Start the DFS movement coroutine
        StartCoroutine(MoveAgentDFS(startPosition));
    }

    // Initialize maze data and goal position (called by MazeGenerator)
    public void InitializeMaze(bool[,] generatedMaze, Vector2Int goalPos)
    {
        maze = generatedMaze;
        goalPosition = goalPos;

        // Initialize visited array to track which tiles have been visited
        visited = new bool[maze.GetLength(0), maze.GetLength(1)];
        Debug.Log("Maze initialized, starting DFS...");
    }

    // Coroutine to move the agent using DFS through the maze
    IEnumerator MoveAgentDFS(Vector2Int startPosition)
    {
        if (maze == null)
        {
            Debug.LogError("Maze data not initialized!");
            yield break;
        }

        pathStack.Push(startPosition);
        Vector2Int currentPosition = startPosition;
        visited[currentPosition.x, currentPosition.y] = true;  // Mark start as visited

        while (pathStack.Count > 0)
        {
            currentPosition = pathStack.Pop();
            Vector3 worldPosition = new Vector3(currentPosition.x * tileScale, currentPosition.y * tileScale, 0);

            // Move the agent to the current position
            agentObj.transform.position = worldPosition;

            // Leave a trail behind at the previous position
            CreateTrail(worldPosition);

            yield return new WaitForSeconds(moveDelay);  // Wait for a short time to visualize movement

            // Get valid neighbors and log them
            List<Vector2Int> neighbors = GetValidNeighbors(currentPosition);
            Debug.Log($"Current Position: {currentPosition}, Valid Neighbors: {neighbors.Count}");

            // Check for neighboring tiles to continue the search (up, down, left, right)
            foreach (Vector2Int neighbor in neighbors)
            {
                if (!visited[neighbor.x, neighbor.y])  // Only visit unvisited neighbors
                {
                    pathStack.Push(neighbor);
                    visited[neighbor.x, neighbor.y] = true;  // Mark as visited
                }
            }

            // Stop the loop when goal is reached
            if (currentPosition == goalPosition)
            {
                Debug.Log("Goal reached!");
                yield break;
            }
        }
    }

    // Create a trail sprite at the given position
    void CreateTrail(Vector3 position)
    {
        GameObject trailObj = new GameObject("Trail");
        trailObj.transform.position = position;
        SpriteRenderer trailRenderer = trailObj.AddComponent<SpriteRenderer>();
        trailRenderer.sprite = trailSprite;
        trailRenderer.sortingOrder = 1;  // Render below the agent
    }

    // Get valid neighboring tiles (up, down, left, right) for DFS movement
    List<Vector2Int> GetValidNeighbors(Vector2Int currentPos)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        // Define directions
        Vector2Int[] directions = {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        // Check each direction for a valid, walkable neighbor
        foreach (var direction in directions)
        {
            Vector2Int neighborPos = currentPos + direction;

            if (IsInBounds(neighborPos) && maze[neighborPos.x, neighborPos.y] && !visited[neighborPos.x, neighborPos.y])
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
