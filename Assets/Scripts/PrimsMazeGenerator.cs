using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimMazeGenerator : MonoBehaviour, IMazeGenerator
{
    private int[,] maze;
    private Vector2Int startPos;
    private Vector2Int goalPos;
    private const int WALL = 0;
    private const int PATH = 1;
    private const int FRONTIER = 2;

    public int[,] GenerateMaze(int width, int height)
    {
        // Initialize the maze array
        maze = new int[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                maze[x, y] = WALL; // All cells start as walls

        // Start from a random odd cell within the maze
        int startX = Random.Range(1, width / 2) * 2 - 1;
        int startY = Random.Range(1, height / 2) * 2 - 1;
        startPos = new Vector2Int(1, 1 );

        MarkCellAsPath(startX, startY);

        // Generate the maze by processing frontiers
        while (frontiers.Count > 0)
        {
            // Randomly select a frontier cell
            Vector2Int frontierCell = frontiers[Random.Range(0, frontiers.Count)];
            frontiers.Remove(frontierCell);

            // Try to connect this frontier cell to an existing path
            if (TryCarvePathToNeighbor(frontierCell))
            {
                // Mark this cell as part of the path
                MarkCellAsPath(frontierCell.x, frontierCell.y);
            }
        }

        // Set the goal position as the furthest path cell from start
        goalPos = MazeManager.Instance.FindFurthestPathFromStart(startPos);

        Debug.Log($"Maze generation complete with start at {startPos} and goal at {goalPos}");
        return maze;
    }

    private List<Vector2Int> frontiers = new List<Vector2Int>();

    private void MarkCellAsPath(int x, int y)
    {
        maze[x, y] = PATH;

        // Add valid neighbors as frontiers
        AddFrontierCell(x + 2, y);
        AddFrontierCell(x - 2, y);
        AddFrontierCell(x, y + 2);
        AddFrontierCell(x, y - 2);
    }

    private void AddFrontierCell(int x, int y)
    {
        if (IsInBounds(x, y) && maze[x, y] == WALL)
        {
            maze[x, y] = FRONTIER;
            frontiers.Add(new Vector2Int(x, y));
        }
    }

    private bool TryCarvePathToNeighbor(Vector2Int cell)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        // Collect neighbors that are paths
        if (IsPath(cell.x + 2, cell.y)) neighbors.Add(new Vector2Int(cell.x + 2, cell.y));
        if (IsPath(cell.x - 2, cell.y)) neighbors.Add(new Vector2Int(cell.x - 2, cell.y));
        if (IsPath(cell.x, cell.y + 2)) neighbors.Add(new Vector2Int(cell.x, cell.y + 2));
        if (IsPath(cell.x, cell.y - 2)) neighbors.Add(new Vector2Int(cell.x, cell.y - 2));

        if (neighbors.Count == 0) return false;

        // Randomly select a neighbor and carve a path to it
        Vector2Int neighbor = neighbors[Random.Range(0, neighbors.Count)];
        Vector2Int between = (cell + neighbor) / 2; // Midpoint between cell and neighbor
        maze[between.x, between.y] = PATH; // Carve a path between the two cells
        return true;
    }

    private bool IsPath(int x, int y)
    {
        return IsInBounds(x, y) && maze[x, y] == PATH;
    }

    private bool IsInBounds(int x, int y)
    {
        return x > 0 && x < maze.GetLength(0) - 1 && y > 0 && y < maze.GetLength(1) - 1;
    }

    public Vector2Int GetStartPosition() => startPos;
    public Vector2Int GetGoalPosition() => goalPos;
    public void SetGoalPosition(Vector2Int newGoalPos) => goalPos = newGoalPos;
}
