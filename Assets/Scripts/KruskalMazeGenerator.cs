using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KruskalMazeGenerator : MonoBehaviour, IMazeGenerator
{
    private int[,] maze;
    private Vector2Int startPos;
    private Vector2Int goalPos;

    public int[,] GenerateMaze(int width, int height)
    {
        maze = new int[width, height];
        List<Wall> walls = new List<Wall>();
        Dictionary<Vector2Int, int> cellSet = new Dictionary<Vector2Int, int>();
        int nextSetId = 0;

        InitializeCellsAndWalls(width, height, walls, cellSet, ref nextSetId);

        // Kruskal's algorithm for maze generation
        while (walls.Count > 0)
        {
            int randomIndex = Random.Range(0, walls.Count);
            Wall wall = walls[randomIndex];
            walls.RemoveAt(randomIndex);

            Vector2Int cellA = wall.cellA;
            Vector2Int cellB = wall.cellB;

            if (cellSet.ContainsKey(cellA) && cellSet.ContainsKey(cellB) && cellSet[cellA] != cellSet[cellB])
            {
                RemoveWallBetweenCells(cellA, cellB);
                MergeSets(cellSet, cellSet[cellA], cellSet[cellB]);
            }
        }

        // Define start position
        startPos = new Vector2Int(1, 1);

        // Use MazeManager to find the furthest path for goal position
        goalPos = MazeManager.Instance.FindFurthestPathFromStart(startPos);

        return maze;
    }

    private void InitializeCellsAndWalls(int width, int height, List<Wall> walls, Dictionary<Vector2Int, int> cellSet, ref int nextSetId)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                maze[x, y] = 0;

                if (x % 2 == 1 && y % 2 == 1 && x < width - 1 && y < height - 1)
                {
                    Vector2Int cell = new Vector2Int(x, y);
                    cellSet[cell] = nextSetId++;
                    maze[x, y] = 1;

                    if (x + 2 < width) walls.Add(new Wall(cell, new Vector2Int(x + 2, y)));
                    if (y + 2 < height) walls.Add(new Wall(cell, new Vector2Int(x, y + 2)));
                }
            }
        }
    }

    private void RemoveWallBetweenCells(Vector2Int cellA, Vector2Int cellB)
    {
        Vector2Int wallPos = new Vector2Int((cellA.x + cellB.x) / 2, (cellA.y + cellB.y) / 2);
        maze[wallPos.x, wallPos.y] = 1;

        maze[cellA.x, cellA.y] = 1;
        maze[cellB.x, cellB.y] = 1;
    }

    private void MergeSets(Dictionary<Vector2Int, int> cellSet, int setA, int setB)
    {
        List<Vector2Int> cellsToUpdate = new List<Vector2Int>();
        foreach (var cell in cellSet.Keys)
            if (cellSet[cell] == setB)
                cellsToUpdate.Add(cell);

        foreach (var cell in cellsToUpdate)
            cellSet[cell] = setA;
    }

    public Vector2Int GetStartPosition() => startPos;
    public Vector2Int GetGoalPosition() => goalPos;

    private class Wall
    {
        public Vector2Int cellA;
        public Vector2Int cellB;
        public Wall(Vector2Int a, Vector2Int b) { cellA = a; cellB = b; }
    }
}
