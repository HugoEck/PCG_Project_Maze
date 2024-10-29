using UnityEngine;
using System.Collections.Generic;

public class OscarsMazeGenerator : MonoBehaviour
{
    public Sprite wallSprite;      // The sprite for the wall
    public Sprite floorSprite;     // The sprite for the floor
    public Sprite goalPointSprite; // The sprite for the goal point

    public GameObject agentObject; // Reference to the agent GameObject

    private int width;
    private int height;

    private bool[,] maze;          // Boolean array to represent the maze (true = walkable, false = wall)

    private Camera cam;            // Reference to the Main Camera

    // Method to start maze generation based on the selected algorithm
    private void Start()
    {
        // Get the selected algorithm and maze size from the main menu
        width = MainMenuController.mazeSize;
        height = MainMenuController.mazeSize;
        string algorithm = MainMenuController.selectedAlgorithm;

        // Initialize the maze array
        maze = new bool[width, height];

        // Get the camera reference
        cam = Camera.main;

        // Generate the maze based on the selected algorithm
        if (algorithm == "DFS")
        {
            GenerateMazeWithDFS();
        }
        else if (algorithm == "Prim's Algorithm")
        {
            GenerateMazeWithPrims();
        }

        // Render the maze with individual sprites
        RenderMaze();

        // Place the goal at the top-right corner
        PlaceGoalAtTopRight();

        // Adjust the camera to fit the maze
        AdjustCameraToFitMaze();
    }

    // Depth First Search maze generation method
    void GenerateMazeWithDFS()
    {
        Debug.Log("Generating maze with DFS, size: " + width + "x" + height);

        // Set all cells to walls by default
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                maze[x, y] = false; // Mark as wall
            }
        }

        // Carve out the maze using DFS from (1,1)
        DepthFirstSearch(1, 1);

        // Ensure boundary walls are present
        SetBoundaryWalls();
    }

    // Prim's Algorithm maze generation method
    void GenerateMazeWithPrims()
    {
        Debug.Log("Generating maze with Prim's Algorithm, size: " + width + "x" + height);

        // Set all cells to walls initially
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                maze[x, y] = false; // Mark as wall
            }
        }

        // Carve out the maze using Prim's Algorithm
        PrimAlgorithm();

        // Ensure boundary walls are present
        SetBoundaryWalls();
    }

    // Prim's Algorithm for maze generation
    void PrimAlgorithm()
    {
        List<Vector2Int> wallList = new List<Vector2Int>();

        // Start at (1, 1) and mark it as a path
        maze[1, 1] = true;
        AddAdjacentWalls(1, 1, wallList);

        while (wallList.Count > 0)
        {
            // Pick a random wall
            int randomIndex = Random.Range(0, wallList.Count);
            Vector2Int wall = wallList[randomIndex];
            wallList.RemoveAt(randomIndex);

            // Ensure the wall is within bounds
            if (!IsInBounds(wall))
                continue;

            // Get neighboring cells (path candidates)
            Vector2Int[] neighbors = GetNeighbors(wall);

            if (IsValidWall(neighbors))
            {
                // Carve out the wall into a path
                maze[wall.x, wall.y] = true;

                // Add adjacent walls to the list
                foreach (var neighbor in neighbors)
                {
                    if (IsInBounds(neighbor) && !maze[neighbor.x, neighbor.y])
                    {
                        wallList.Add(neighbor);
                    }
                }
            }

            // Safety check to prevent infinite loop
            if (wallList.Count > 100000) // Arbitrary large number to prevent crashing
            {
                Debug.LogError("Prim's Algorithm: Potential infinite loop detected. Aborting generation.");
                break;
            }
        }
    }

    // Helper to set boundary walls around the maze (without doubling them)
    void SetBoundaryWalls()
    {
        // Top and bottom walls
        for (int x = 0; x < width; x++)
        {
            maze[x, 0] = false; // Top wall
            maze[x, height - 1] = false; // Bottom wall
        }

        // Left and right walls
        for (int y = 0; y < height; y++)
        {
            maze[0, y] = false; // Left wall
            maze[width - 1, y] = false; // Right wall
        }
    }

    // DFS recursive carving function
    void DepthFirstSearch(int x, int y)
    {
        maze[x, y] = true; // Mark current cell as walkable

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        ShuffleDirections(directions); // Shuffle directions for randomness

        foreach (Vector2Int dir in directions)
        {
            int newX = x + dir.x * 2;
            int newY = y + dir.y * 2;

            if (IsInBounds(newX, newY) && !maze[newX, newY])
            {
                maze[x + dir.x, y + dir.y] = true; // Carve path
                DepthFirstSearch(newX, newY); // Recur
            }
        }
    }

    // Helper function to check if wall has exactly one neighboring path
    bool IsValidWall(Vector2Int[] neighbors)
    {
        int pathCount = 0;
        foreach (var neighbor in neighbors)
        {
            if (IsInBounds(neighbor.x, neighbor.y) && maze[neighbor.x, neighbor.y])
            {
                pathCount++;
            }
        }
        return pathCount == 1; // Exactly one adjacent path
    }

    // Helper function to add adjacent walls to the wall list
    void AddAdjacentWalls(int x, int y, List<Vector2Int> wallList)
    {
        if (IsInBounds(x + 1, y)) wallList.Add(new Vector2Int(x + 1, y));
        if (IsInBounds(x - 1, y)) wallList.Add(new Vector2Int(x - 1, y));
        if (IsInBounds(x, y + 1)) wallList.Add(new Vector2Int(x, y + 1));
        if (IsInBounds(x, y - 1)) wallList.Add(new Vector2Int(x, y - 1));
    }

    // Return neighbors of a wall
    Vector2Int[] GetNeighbors(Vector2Int wall)
    {
        Vector2Int[] neighbors = new Vector2Int[2];
        if (wall.x % 2 == 0)
        {
            neighbors[0] = new Vector2Int(wall.x + 1, wall.y);
            neighbors[1] = new Vector2Int(wall.x - 1, wall.y);
        }
        else
        {
            neighbors[0] = new Vector2Int(wall.x, wall.y + 1);
            neighbors[1] = new Vector2Int(wall.x, wall.y - 1);
        }
        return neighbors;
    }

    // Helper method to create a sprite at a specific position
    void CreateSprite(Sprite sprite, Vector3 position)
    {
        GameObject spriteObj = new GameObject("Sprite");
        spriteObj.transform.position = position;
        SpriteRenderer renderer = spriteObj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 0; // Set sorting order for correct layering
    }

    // Helper function to shuffle directions
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

    // Check if the coordinates are within bounds (int overload)
    bool IsInBounds(int x, int y)
    {
        return x > 0 && x < width - 1 && y > 0 && y < height - 1;
    }

    // Check if the coordinates are within bounds (Vector2Int overload)
    bool IsInBounds(Vector2Int position)
    {
        return IsInBounds(position.x, position.y);
    }

    // Place the goal at the top-right corner
    void PlaceGoalAtTopRight()
    {
        Vector3 goalPosition = new Vector3(width - 2, height - 2, 0);
        CreateSprite(goalPointSprite, goalPosition);
    }

    // Render the maze using SpriteRenderers (walls and floors)
    void RenderMaze()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = new Vector3(x, y, 0);
                if (maze[x, y])
                {
                    CreateSprite(floorSprite, position);
                }
                else
                {
                    CreateSprite(wallSprite, position);
                }
            }
        }
    }

    // Adjust the camera to fit the entire maze
    void AdjustCameraToFitMaze()
    {
        Vector3 mazeCenter = new Vector3(width / 2f, height / 2f, -10);
        cam.transform.position = mazeCenter;

        float aspectRatio = (float)Screen.width / (float)Screen.height;
        float verticalSize = height / 2f;
        float horizontalSize = (width / 2f) / aspectRatio;

        cam.orthographicSize = Mathf.Max(verticalSize, horizontalSize);
    }
}
