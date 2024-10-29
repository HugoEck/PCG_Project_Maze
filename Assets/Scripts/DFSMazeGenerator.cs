using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DFSMazeGenerator : MonoBehaviour
{
    public int width = 10;  // Maze width
    public int height = 10; // Maze height
    public int[,] maze;     // Maze array

    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject startPrefab;
    public GameObject goalPrefab;

    public Vector2Int startPos { get; private set; }  // Start position
    public Vector2Int goalPos { get; private set; }   // Goal position

    void Start()
    {
        GenerateMaze();
        SetStartAndGoal();
        DrawMaze();
    }

    // Generates the maze using DFS
    private void GenerateMaze()
    {
        maze = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                maze[x, y] = 0;
            }
        }

        RecursiveDFS(1, 1);
    }

    // Recursive DFS algorithm for maze generation
    private void RecursiveDFS(int x, int y)
    {
        maze[x, y] = 1;
        int[] directions = { 1, 2, 3, 4 };
        ShuffleArray(directions);

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
                    if (y + 2 < height - 1 && maze[x, y + 2] == 0)
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
                    if (x + 2 < width - 1 && maze[x + 2, y] == 0)
                    {
                        maze[x + 1, y] = 1;
                        RecursiveDFS(x + 2, y);
                    }
                    break;
            }
        }
    }

    // Set start and goal positions within the maze
    public void SetStartAndGoal()
    {
        startPos = new Vector2Int(1, 1);
        goalPos = new Vector2Int(width - 3, height - 3);

        // Ensure start and goal are set as paths
        maze[startPos.x, startPos.y] = 1;
        maze[goalPos.x, goalPos.y] = 1;

        // Instantiate start and goal prefabs
        Instantiate(startPrefab, new Vector2(startPos.x, startPos.y), Quaternion.identity, transform);
        Instantiate(goalPrefab, new Vector2(goalPos.x, goalPos.y), Quaternion.identity, transform);
        Debug.Log($"Start instantiated at: {startPos}, GameObject: {startPrefab}");
        Debug.Log($"Goal instantiated at: {goalPos}, GameObject: {goalPrefab}");
    }

    // Draw the maze using wall and floor prefabs
    public void DrawMaze()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject prefab = maze[x, y] == 1 ? floorPrefab : wallPrefab;
                Instantiate(prefab, new Vector2(x, y), Quaternion.identity, transform);
            }
        }
    }

    // Shuffle the directions array
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
