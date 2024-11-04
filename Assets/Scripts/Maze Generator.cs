using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject pathPrefab;  // For the solution path
    public GameObject goalPrefab;  // For the end goal (green tile)
    public GameObject startPrefab; // For the start tile (e.g., different color)

    public float pathAnimationDelay = 0.0001f; // Delay between drawing each tile

    private int[,] maze;
    private bool[,] visited;
    private List<Vector2> solutionPath = new List<Vector2>();
    private Vector2 startPos;
    public Vector2 goalPos;

    void Start()
    {
        GenerateMaze();
        DrawMaze();

        // Randomly select a start and a goal position
        SelectStartAndGoal();

        // Reset the visited array for pathfinding
        visited = new bool[width, height];

        // Attempt to solve the maze
        if (SolveMaze((int)startPos.x, (int)startPos.y))
        {
            Debug.Log("Solution found!");
            // Start the coroutine to draw the solution path one tile at a time
            StartCoroutine(AnimateSolutionPath());
        }
        else
        {
            Debug.LogError("No solution found. The goal might be unreachable.");
        }

        // Mark the start and goal with appropriate prefabs
        MarkStartAndGoal();
    }

    void GenerateMaze()
    {
        maze = new int[width, height];
        visited = new bool[width, height];

        // Initialize the maze with walls (0 represents walls, 1 represents paths)
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                maze[x, y] = 0; // Set all cells as walls initially
            }
        }

        // Start the recursive backtracking algorithm from (1, 1)
        RecursiveBacktrack(1, 1);

        // Ensure the boundaries (top and right) are 1-tile thick
        for (int x = 0; x < width; x++) maze[x, height - 1] = 0; // Top boundary
        for (int y = 0; y < height; y++) maze[width - 1, y] = 0; // Right boundary
    }

    void RecursiveBacktrack(int x, int y)
    {
        maze[x, y] = 1; // Mark this cell as part of the maze (as a path)

        int[] directions = { 1, 2, 3, 4 };
        ShuffleArray(directions); // Shuffle to randomize the direction

        foreach (int direction in directions)
        {
            switch (direction)
            {
                case 1: // Up
                    if (y - 2 <= 0) continue; // Skip if at the top boundary
                    if (maze[x, y - 2] == 0) // Check if the cell two steps up is a wall
                    {
                        maze[x, y - 1] = 1; // Carve path (break wall between cells)
                        RecursiveBacktrack(x, y - 2);
                    }
                    break;
                case 2: // Down
                    if (y + 2 >= height - 2) continue; // Skip if at the bottom boundary
                    if (maze[x, y + 2] == 0)
                    {
                        maze[x, y + 1] = 1;
                        RecursiveBacktrack(x, y + 2);
                    }
                    break;
                case 3: // Left
                    if (x - 2 <= 0) continue; // Skip if at the left boundary
                    if (maze[x - 2, y] == 0)
                    {
                        maze[x - 1, y] = 1;
                        RecursiveBacktrack(x - 2, y);
                    }
                    break;
                case 4: // Right
                    if (x + 2 >= width - 2) continue; // Skip if at the right boundary
                    if (maze[x + 2, y] == 0)
                    {
                        maze[x + 1, y] = 1;
                        RecursiveBacktrack(x + 2, y);
                    }
                    break;
            }
        }
    }

    // Coroutine to draw the solution path one tile at a time
    IEnumerator AnimateSolutionPath()
    {
        foreach (Vector2 position in solutionPath)
        {
            // Instantiate the path prefab at the current position
            Instantiate(pathPrefab, new Vector2(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y)), Quaternion.identity, transform);

            // Wait for a short delay before drawing the next tile
            yield return new WaitForSeconds(pathAnimationDelay);
        }
    }

    // Depth-First Search to find the solution path
    bool SolveMaze(int x, int y)
    {
        // Check if we've reached the goal
        if (x == (int)goalPos.x && y == (int)goalPos.y)
        {
            solutionPath.Add(new Vector2(x, y));
            return true;
        }

        // Check if the current move is valid
        if (IsValidMove(x, y))
        {
            visited[x, y] = true; // Mark as visited
            solutionPath.Add(new Vector2(x, y));

            // Recursively try all directions (up, down, left, right)
            if (SolveMaze(x, y - 1)) return true; // Up
            if (SolveMaze(x, y + 1)) return true; // Down
            if (SolveMaze(x - 1, y)) return true; // Left
            if (SolveMaze(x + 1, y)) return true; // Right

            // If none of the moves are valid, backtrack
            solutionPath.Remove(new Vector2(x, y));
        }

        return false; // No solution found from this path
    }

    // Ensure the move is valid
    bool IsValidMove(int x, int y)
    {
        // Must be within bounds, must be a path (maze[x, y] == 1), and not already visited
        return (x >= 0 && x < width && y >= 0 && y < height && maze[x, y] == 1 && !visited[x, y]);
    }

    // Draw the maze using wall and floor prefabs
    void DrawMaze()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (maze[x, y] == 0)
                {
                    Instantiate(wallPrefab, new Vector2(x, y), Quaternion.identity, transform);
                }
                else
                {
                    Instantiate(floorPrefab, new Vector2(x, y), Quaternion.identity, transform);
                }
            }
        }
    }

    // Randomly select start from a corner and goal from the maze
    void SelectStartAndGoal()
    {
        startPos = new Vector2(1, 1); // Startpoint is always left lower corner of the maze

        // Ensure the startPos is on a walkable path
        if (maze[(int)startPos.x, (int)startPos.y] == 0)
        {
            Debug.LogError("Start position is in a wall. Adjust the maze.");
            return;
        }

        // Select a random walkable tile as the goal that is reachable and not near walls
        int maxAttempts = 100; // Set a limit for the number of attempts to find a valid goal
        int attempts = 0;
        do
        {
            // Set the goal at least 2 tiles away from walls on all sides
            int goalX = Random.Range(2, width - 3);  // Avoid the boundary walls by starting at 2
            int goalY = Random.Range(2, height - 3);

            // Ensure the goal is a walkable tile and there is a path from start to goal
            if (maze[goalX, goalY] == 1 && PathExists(startPos, new Vector2(goalX, goalY)))
            {
                goalPos = new Vector2(goalX, goalY);
                break;
            }
            attempts++;
        } while (attempts < maxAttempts);

        if (attempts == maxAttempts)
        {
            Debug.LogError("Failed to find a reachable goal. Consider increasing the maze's complexity or size.");
        }

        Debug.Log("Start: " + startPos);
        Debug.Log("Goal: " + goalPos);
    }

    // Check if there is a path between start and goal
    bool PathExists(Vector2 start, Vector2 goal)
    {
        bool[,] tempVisited = new bool[width, height];
        return DFSForPath((int)start.x, (int)start.y, (int)goal.x, (int)goal.y, tempVisited);
    }

    // A simple DFS check for path existence between two points
    bool DFSForPath(int x, int y, int goalX, int goalY, bool[,] tempVisited)
    {
        if (x == goalX && y == goalY)
        {
            return true;
        }

        if (x >= 0 && x < width && y >= 0 && y < height && maze[x, y] == 1 && !tempVisited[x, y])
        {
            tempVisited[x, y] = true;

            // Explore all four directions
            if (DFSForPath(x, y - 1, goalX, goalY, tempVisited)) return true; // Up
            if (DFSForPath(x, y + 1, goalX, goalY, tempVisited)) return true; // Down
            if (DFSForPath(x - 1, y, goalX, goalY, tempVisited)) return true; // Left
            if (DFSForPath(x + 1, y, goalX, goalY, tempVisited)) return true; // Right
        }

        return false;
    }

    // Mark the start and goal tiles with appropriate prefabs
    void MarkStartAndGoal()
    {
        // Mark the start tile
        Instantiate(startPrefab, new Vector2(startPos.x, startPos.y), Quaternion.identity, transform);

        // Mark the goal tile
        Instantiate(goalPrefab, new Vector2(goalPos.x, goalPos.y), Quaternion.identity, transform);
    }

    // Helper function to shuffle directions
    void ShuffleArray(int[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = array[i];
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
    }
}
