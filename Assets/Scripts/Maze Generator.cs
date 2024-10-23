using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject pathPrefab; // To show the solution path

    private int[,] maze;
    private bool[,] visited;
    private List<Vector2> solutionPath = new List<Vector2>();

    void Start()
    {
        GenerateMaze();
        DrawMaze();

        if (SolveMaze(1, 1)) // Attempt to solve from (1, 1) to (width - 2, height - 2)
        {
            DrawSolution(); // Draw the solution path
        }
        else
        {
            Debug.Log("No Solution Found!");
        }
    }

    void GenerateMaze()
    {
        maze = new int[width, height];
        visited = new bool[width, height];

        // Start the recursive backtracking algorithm from (1, 1)
        RecursiveBacktrack(1, 1);
    }

    void RecursiveBacktrack(int x, int y)
    {
        maze[x, y] = 1; // Mark this cell as part of the maze

        int[] directions = { 1, 2, 3, 4 };
        ShuffleArray(directions); // Shuffle to randomize the direction

        foreach (int direction in directions)
        {
            switch (direction)
            {
                case 1: // Up
                    if (y - 2 <= 0) continue;
                    if (maze[x, y - 2] == 0)
                    {
                        maze[x, y - 1] = 1;
                        RecursiveBacktrack(x, y - 2);
                    }
                    break;
                case 2: // Down
                    if (y + 2 >= height - 1) continue;
                    if (maze[x, y + 2] == 0)
                    {
                        maze[x, y + 1] = 1;
                        RecursiveBacktrack(x, y + 2);
                    }
                    break;
                case 3: // Left
                    if (x - 2 <= 0) continue;
                    if (maze[x - 2, y] == 0)
                    {
                        maze[x - 1, y] = 1;
                        RecursiveBacktrack(x - 2, y);
                    }
                    break;
                case 4: // Right
                    if (x + 2 >= width - 1) continue;
                    if (maze[x + 2, y] == 0)
                    {
                        maze[x + 1, y] = 1;
                        RecursiveBacktrack(x + 2, y);
                    }
                    break;
            }
        }
    }

    // Depth-First Search to find the solution path
    bool SolveMaze(int x, int y)
    {
        // Base case: reached the exit
        if (x == width - 2 && y == height - 2)
        {
            solutionPath.Add(new Vector2(x, y));
            return true;
        }

        if (IsValidMove(x, y))
        {
            visited[x, y] = true;
            solutionPath.Add(new Vector2(x, y));

            // Move up
            if (SolveMaze(x, y - 1)) return true;
            // Move down
            if (SolveMaze(x, y + 1)) return true;
            // Move left
            if (SolveMaze(x - 1, y)) return true;
            // Move right
            if (SolveMaze(x + 1, y)) return true;

            // If none of the moves work, backtrack
            solutionPath.Remove(new Vector2(x, y));
        }

        return false;
    }

    bool IsValidMove(int x, int y)
    {
        return (x >= 0 && x < width && y >= 0 && y < height && maze[x, y] == 1 && !visited[x, y]);
    }

    // Draw the maze
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

    // Draw the solution path using pathPrefab
    void DrawSolution()
    {
        foreach (Vector2 position in solutionPath)
        {
            Instantiate(pathPrefab, position, Quaternion.identity, transform);
        }
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
