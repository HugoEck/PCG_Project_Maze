using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DFSMazeGenerator : MonoBehaviour, IMazeGenerator
{
    private int[,] maze;
    private Vector2Int startPos;
    private Vector2Int goalPos;

    public int[,] GenerateMaze(int width, int height)
    {
        // Initialize maze and set up walls
        maze = new int[width, height];
        InitializeMaze(width, height);

        // Start DFS maze generation from position (1, 1)
        RecursiveDFS(1, 1);

        // Define start position
        startPos = new Vector2Int(1, 1);

        // Use MazeManager to find the furthest path for goal position
        goalPos = MazeManager.Instance.FindFurthestPathFromStart(startPos);

        Debug.Log($"Maze generation complete with start at {startPos} and goal at {goalPos}");
        return maze;
    }

    private void InitializeMaze(int width, int height)
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                maze[x, y] = 0;  // Initialize all as walls
    }

    private void RecursiveDFS(int x, int y)
    {
        maze[x, y] = 1;  // Mark cell as a path
        int[] directions = { 1, 2, 3, 4 };
        ShuffleArray(directions);  // Shuffle directions to randomize path

        foreach (int direction in directions)
        {
            switch (direction)
            {
                case 1: // Up
                    if (y - 2 > 0 && maze[x, y - 2] == 0)
                    {
                        maze[x, y - 1] = 1;
                        RecursiveDFS(x, y - 2);
                    }
                    break;
                case 2: // Down
                    if (y + 2 < maze.GetLength(1) - 1 && maze[x, y + 2] == 0)
                    {
                        maze[x, y + 1] = 1;
                        RecursiveDFS(x, y + 2);
                    }
                    break;
                case 3: // Left
                    if (x - 2 > 0 && maze[x - 2, y] == 0)
                    {
                        maze[x - 1, y] = 1;
                        RecursiveDFS(x - 2, y);
                    }
                    break;
                case 4: // Right
                    if (x + 2 < maze.GetLength(0) - 1 && maze[x + 2, y] == 0)
                    {
                        maze[x + 1, y] = 1;
                        RecursiveDFS(x + 2, y);
                    }
                    break;
            }
        }
    }

    //private Vector2Int FindFurthestPathFromStart()
    //{
    //    Queue<Vector2Int> queue = new Queue<Vector2Int>();
    //    Dictionary<Vector2Int, int> distances = new Dictionary<Vector2Int, int>();

    //    queue.Enqueue(startPos);
    //    distances[startPos] = 0;

    //    Vector2Int furthestCell = startPos;
    //    int maxDistance = 0;

    //    while (queue.Count > 0)
    //    {
    //        Vector2Int current = queue.Dequeue();
    //        int currentDistance = distances[current];

    //        // Explore neighbors in 4 cardinal directions
    //        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
    //        foreach (var dir in directions)
    //        {
    //            Vector2Int neighbor = current + dir;

    //            // Check if the neighbor is within bounds, is a floor tile, and hasn't been visited
    //            if (IsInBounds(neighbor) && maze[neighbor.x, neighbor.y] == 1 && !distances.ContainsKey(neighbor))
    //            {
    //                distances[neighbor] = currentDistance + 1;
    //                queue.Enqueue(neighbor);

    //                // Update furthest cell if this is the longest path found
    //                if (distances[neighbor] > maxDistance)
    //                {
    //                    maxDistance = distances[neighbor];
    //                    furthestCell = neighbor;
    //                }
    //            }
    //        }
    //    }

    //    return furthestCell;
    //}

    private bool IsInBounds(Vector2Int cell)
    {
        return cell.x > 0 && cell.x < maze.GetLength(0) - 1 && cell.y > 0 && cell.y < maze.GetLength(1) - 1;
    }

    public Vector2Int GetStartPosition() => startPos;
    public Vector2Int GetGoalPosition() => goalPos;

    private void ShuffleArray(int[] array)
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
