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
        maze = new int[width, height];
        InitializeMaze(width, height);
        RecursiveDFS(1, 1);

        startPos = new Vector2Int(1, 1);
        goalPos = new Vector2Int(width - 3, height - 3);

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
        maze[x, y] = 1;
        int[] directions = { 1, 2, 3, 4 };
        ShuffleArray(directions);

        foreach (int direction in directions)
        {
            switch (direction)
            {
                case 1: if (y - 2 > 0 && maze[x, y - 2] == 0) { maze[x, y - 1] = 1; RecursiveDFS(x, y - 2); } break;
                case 2: if (y + 2 < maze.GetLength(1) - 1 && maze[x, y + 2] == 0) { maze[x, y + 1] = 1; RecursiveDFS(x, y + 2); } break;
                case 3: if (x - 2 > 0 && maze[x - 2, y] == 0) { maze[x - 1, y] = 1; RecursiveDFS(x - 2, y); } break;
                case 4: if (x + 2 < maze.GetLength(0) - 1 && maze[x + 2, y] == 0) { maze[x + 1, y] = 1; RecursiveDFS(x + 2, y); } break;
            }
        }
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
