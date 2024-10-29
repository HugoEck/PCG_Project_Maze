using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    public Sprite agentSprite;    // The sprite for the agent
    public Sprite trailSprite;    // The sprite for the trail
    public float tileScale = 1f;  // The size of each tile (e.g., 1 unit per tile)
    public float moveDelay = 0.05f; // Delay between each move

    private Vector2Int goalPosition;  // The position of the goal
    private bool[,] maze;             // The maze array (walkable or not)
    private bool[,] visited;          // Array to track visited positions

    private GameObject agentObj;      // The GameObject for the agent
    private Coroutine explorationCoroutine;  // Reference to the running exploration coroutine

    public UIController uiController;  // Reference to the UIController for the game over UI

    // Initialize the agent's start position
    public void SpawnAgent(Vector2Int startPosition)
    {
        // If the agent already exists, remove it
        if (agentObj != null)
        {
            Destroy(agentObj);
        }

        // Create a new agent GameObject and set the sorting layer
        agentObj = new GameObject("Agent");
        agentObj.transform.position = new Vector3(startPosition.x * tileScale, startPosition.y * tileScale, 0);
        SpriteRenderer renderer = agentObj.AddComponent<SpriteRenderer>();
        renderer.sprite = agentSprite;
        renderer.sortingOrder = 10;  // Ensure agent is rendered above the maze and trail
    }

    // Initialize the maze data and goal position (called from OscarsMazeGenerator)
    public void InitializeMaze(bool[,] generatedMaze, Vector2Int goalPos)
    {
        maze = generatedMaze;
        goalPosition = goalPos;

        // Initialize the visited array to track which tiles have been visited
        visited = new bool[maze.GetLength(0), maze.GetLength(1)];
        Debug.Log("Maze initialized, starting exploration...");

        // Spawn the agent at the start position (1,1)
        Vector2Int startPosition = new Vector2Int(1, 1);
        if (maze[startPosition.x, startPosition.y])
        {
            SpawnAgent(startPosition);
        }
        else
        {
            Debug.LogError("Start position is blocked!");
        }

        // Start the DFS exploration after initialization
        if (explorationCoroutine != null)
        {
            StopCoroutine(explorationCoroutine);
        }

        explorationCoroutine = StartCoroutine(ExploreMaze(startPosition));
    }

    // Coroutine for agent movement and exploration (DFS)
    IEnumerator ExploreMaze(Vector2Int currentPosition)
    {
        // Check if the current position is the goal
        if (currentPosition == goalPosition)
        {
            Debug.Log("Goal Reached!");
            uiController.ShowGameOverScreen();  // Show the game-over screen when the goal is reached
            yield break;
        }

        // Mark the current position as visited
        visited[currentPosition.x, currentPosition.y] = true;

        // Visualize the agent's trail
        GameObject trailObj = new GameObject("Trail");
        trailObj.transform.position = new Vector3(currentPosition.x * tileScale, currentPosition.y * tileScale, 0);
        SpriteRenderer trailRenderer = trailObj.AddComponent<SpriteRenderer>();
        trailRenderer.sprite = trailSprite;
        trailRenderer.sortingOrder = 1; // Ensure the trail is rendered under the agent

        // Directions to explore (up, down, left, right)
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        // Shuffle directions to explore in a random order
        ShuffleDirections(directions);

        // Explore in each direction
        foreach (Vector2Int dir in directions)
        {
            Vector2Int nextPosition = currentPosition + dir;

            // Check if the next position is within bounds, is walkable, and hasn't been visited
            if (IsInBounds(nextPosition) && maze[nextPosition.x, nextPosition.y] && !visited[nextPosition.x, nextPosition.y])
            {
                // Move the agent to the next position
                agentObj.transform.position = new Vector3(nextPosition.x * tileScale, nextPosition.y * tileScale, 0);

                // Wait for the delay before moving to the next tile
                yield return new WaitForSeconds(moveDelay);

                // Recursively explore the next position
                yield return StartCoroutine(ExploreMaze(nextPosition));
            }
        }

        // Backtrack if no unvisited paths remain (DFS behavior)
        yield return null;
    }

    // Helper function to shuffle directions for randomness
    void ShuffleDirections(Vector2Int[] directions)
    {
        for (int i = 0; i < directions.Length; i++)
        {
            Vector2Int temp = directions[i];
            int randomIndex = Random.Range(i, directions.Length);
            directions[i] = directions[randomIndex];
            directions[randomIndex] = temp;
        }
    }

    // Helper function to check if a position is within maze bounds
    bool IsInBounds(Vector2Int position)
    {
        return position.x >= 0 && position.x < maze.GetLength(0) &&
               position.y >= 0 && position.y < maze.GetLength(1);
    }
}
