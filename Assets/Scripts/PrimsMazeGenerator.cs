using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PrimMazeGenerator : MonoBehaviour, IMazeGenerator
{
    private int[,] maze;
    private Vector2Int startPos;
    private Vector2Int goalPos;

    public int[,] GenerateMaze(int width, int height)
    {
        // Initialize maze grid
        maze = new int[width, height];

        // Set all cells as walls initially
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                maze[x, y] = 0;

        // Define the start position and mark it as a passage
        startPos = new Vector2Int(1, 1);
        maze[startPos.x, startPos.y] = 1;

        // Initialize frontier cells around start position
        HashSet<Vector2Int> frontier = GetNeighborCells(startPos.x, startPos.y, true);

        // Generate maze by carving passages
        while (frontier.Any())
        {
            // Pick a random frontier cell
            Vector2Int cell = frontier.ElementAt(Random.Range(0, frontier.Count));
            frontier.Remove(cell);

            // Ensure this cell connects to only one passage cell
            if (GetNeighborCells(cell.x, cell.y, false).Count == 1)
            {
                maze[cell.x, cell.y] = 1; // Mark as passage

                // Connect this cell to an existing passage cell
                Vector2Int connection = GetNeighborCells(cell.x, cell.y, false).First();
                Vector2Int between = GetBetweenCell(cell, connection);
                maze[between.x, between.y] = 1;

                // Add new frontier cells around this cell
                frontier.UnionWith(GetNeighborCells(cell.x, cell.y, true));
            }
        }

        // Use MazeManager to find the furthest path for goal position
        goalPos = MazeManager.Instance.FindFurthestPathFromStart(startPos);

        Debug.Log($"Maze generation complete with start at {startPos} and goal at {goalPos}");
        return maze;
    }

    //private Vector2Int FindFurthestFloorFromStart()
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

    //            // Check if the neighbor is a floor tile and hasn't been visited
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

    private HashSet<Vector2Int> GetNeighborCells(int x, int y, bool forFrontier)
    {
        HashSet<Vector2Int> neighbors = new HashSet<Vector2Int>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var dir in directions)
        {
            Vector2Int neighbor = new Vector2Int(x + dir.x * 2, y + dir.y * 2);
            if (IsInBounds(neighbor) && maze[neighbor.x, neighbor.y] == (forFrontier ? 0 : 1))
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    private Vector2Int GetBetweenCell(Vector2Int cell1, Vector2Int cell2)
    {
        return new Vector2Int((cell1.x + cell2.x) / 2, (cell1.y + cell2.y) / 2);
    }

    private bool IsInBounds(Vector2Int cell)
    {
        return cell.x > 0 && cell.x < maze.GetLength(0) - 1 && cell.y > 0 && cell.y < maze.GetLength(1) - 1;
    }

    public Vector2Int GetStartPosition() => startPos;
    public Vector2Int GetGoalPosition() => goalPos;
    public void SetGoalPosition(Vector2Int newGoalPos) => goalPos = newGoalPos;
}
