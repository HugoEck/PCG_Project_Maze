using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OscarsMazeGenerator : MonoBehaviour
{
    public int width;
    public int height;

    public Sprite wallSprite;
    public Sprite floorSprite;
    public Sprite goalSprite;

    public float tileScale = 1f;

    private bool[,] maze;  // The maze structure (walkable or not)
    private Vector2Int startPosition;
    private Vector2Int goalPosition;

    void Start()
    {
        // Ensure the width and height are odd numbers to avoid double boundary issues
        if (width % 2 == 0) width--;
        if (height % 2 == 0) height--;

        // Generate and render the maze
        GenerateMaze();
        RenderMaze();
        AdjustCameraToFitMaze();

        // Debug: Log the start and goal positions
        Debug.Log("Start Position: " + startPosition.x + ", " + startPosition.y);
        Debug.Log("Goal Position: " + goalPosition.x + ", " + goalPosition.y);

        // Pass maze data and goal position to the agent
        AgentController agentController = FindObjectOfType<AgentController>();
        if (agentController != null)
        {
            agentController.InitializeMaze(maze, goalPosition);  // Pass the maze data to the agent
        }
        else
        {
            Debug.LogError("AgentController not found in the scene!");
        }
    }

    // Generate a maze using DFS
    void GenerateMaze()
    {
        maze = new bool[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                maze[x, y] = false;  // Set everything to walls
            }
        }

        // Example: DFS to create a random maze
        startPosition = new Vector2Int(1, 1);
        goalPosition = new Vector2Int(width - 2, height - 2);

        // Create a simple random maze using DFS
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        maze[startPosition.x, startPosition.y] = true;  // Start is walkable
        stack.Push(startPosition);

        Vector2Int[] directions = {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Pop();
            List<Vector2Int> neighbors = new List<Vector2Int>();

            // Check 2 tiles away in all directions
            foreach (Vector2Int direction in directions)
            {
                Vector2Int neighbor = current + direction * 2;
                if (IsInBounds(neighbor) && !maze[neighbor.x, neighbor.y])
                {
                    neighbors.Add(neighbor);
                }
            }

            if (neighbors.Count > 0)
            {
                // Randomly pick a neighbor
                stack.Push(current);
                Vector2Int chosenNeighbor = neighbors[Random.Range(0, neighbors.Count)];

                // Make the path between the current and chosen neighbor walkable
                Vector2Int wall = current + (chosenNeighbor - current) / 2;
                maze[wall.x, wall.y] = true;
                maze[chosenNeighbor.x, chosenNeighbor.y] = true;

                stack.Push(chosenNeighbor);
            }
        }

        // Ensure the goal position is walkable
        maze[goalPosition.x, goalPosition.y] = true;
    }

    void RenderMaze()
    {
        // Render the maze tiles as floor and walls
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = new Vector3(x * tileScale, y * tileScale, 0);

                if (maze[x, y])
                {
                    // Floor tile (walkable)
                    CreateSpriteObject(floorSprite, position, 0);
                }
                else
                {
                    // Wall tile (not walkable)
                    CreateSpriteObject(wallSprite, position, 0);
                }
            }
        }

        // Render the goal point
        Vector3 goalPos = new Vector3(goalPosition.x * tileScale, goalPosition.y * tileScale, 0);
        CreateSpriteObject(goalSprite, goalPos, 1);
    }

    // Helper method to create sprites in the scene
    void CreateSpriteObject(Sprite sprite, Vector3 position, int sortingOrder)
    {
        GameObject obj = new GameObject("TileSprite");
        obj.transform.position = position;
        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
    }

    // Adjust the camera to fit the entire maze on screen
    void AdjustCameraToFitMaze()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // Calculate the orthographic size to fit the maze's height
            float mazeHeight = height * tileScale;
            float mazeWidth = width * tileScale;

            // Set the camera's orthographic size to fit the maze's height
            mainCamera.orthographicSize = mazeHeight / 2f;

            // Calculate the aspect ratio to ensure the width fits as well
            float screenAspect = (float)Screen.width / (float)Screen.height;
            float cameraWidth = mainCamera.orthographicSize * screenAspect;

            // Adjust orthographic size to fit the width if needed
            if (cameraWidth < mazeWidth / 2f)
            {
                mainCamera.orthographicSize = (mazeWidth / 2f) / screenAspect;
            }

            // Center the camera on the maze
            mainCamera.transform.position = new Vector3((width / 2f) * tileScale, (height / 2f) * tileScale, -10f);
        }
    }

    // Check if a position is within bounds of the maze
    bool IsInBounds(Vector2Int pos)
    {
        return pos.x > 0 && pos.x < width - 1 && pos.y > 0 && pos.y < height - 1;
    }
}
