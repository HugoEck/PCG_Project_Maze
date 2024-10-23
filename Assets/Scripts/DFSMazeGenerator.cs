using System.Collections.Generic;
using UnityEngine;

public class DFSMazeGenerator : MonoBehaviour
{
    public int width = 10;  // Width of the maze
    public int height = 10; // Height of the maze
    public GameObject wallPrefab;  // Wall prefab
    public GameObject floorPrefab;  // Floor prefab
    public GameObject startPrefab;  // Start point prefab
    public GameObject goalPrefab;   // Goal point prefab

    private int[,] maze;  // Maze array (0 for walls, 1 for paths)
    private Vector2 startPos;
    private Vector2 goalPos;

    void Start()
    {
        GenerateMaze();
        DrawMaze();
        SetStartAndGoal();
    }

    // Generate the maze using DFS
    void GenerateMaze()
    {
        maze = new int[width, height];

        // Initialize all cells as walls (0)
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                maze[x, y] = 0;  // Mark cell as wall
            }
        }

        // Start DFS from a random point (or fixed point like (1,1))
        RecursiveDFS(1, 1);

        // Ensure the outer boundary is walls
        for (int x = 0; x < width; x++)
        {
            maze[x, 0] = maze[x, height - 1] = 0;  // Top and bottom walls
        }
        for (int y = 0; y < height; y++)
        {
            maze[0, y] = maze[width - 1, y] = 0;  // Left and right walls
        }
    }

    // Recursive DFS method to generate the maze
    void RecursiveDFS(int x, int y)
    {
        maze[x, y] = 1;  // Mark current cell as path

        // Randomize directions for more randomness in maze generation
        int[] directions = { 1, 2, 3, 4 };
        ShuffleArray(directions);

        foreach (int direction in directions)
        {
            switch (direction)
            {
                case 1:  // Up
                    if (y - 2 > 0 && maze[x, y - 2] == 0)
                    {
                        maze[x, y - 1] = 1;  // Carve path
                        RecursiveDFS(x, y - 2);
                    }
                    break;
                case 2:  // Down
                    if (y + 2 < height - 1 && maze[x, y + 2] == 0)
                    {
                        maze[x, y + 1] = 1;
                        RecursiveDFS(x, y + 2);
                    }
                    break;
                case 3:  // Left
                    if (x - 2 > 0 && maze[x - 2, y] == 0)
                    {
                        maze[x - 1, y] = 1;
                        RecursiveDFS(x - 2, y);
                    }
                    break;
                case 4:  // Right
                    if (x + 2 < width - 1 && maze[x + 2, y] == 0)
                    {
                        maze[x + 1, y] = 1;
                        RecursiveDFS(x + 2, y);
                    }
                    break;
            }
        }
    }

    // Set start and goal positions
    void SetStartAndGoal()
    {
        // Set start position in the lower-left corner
        startPos = new Vector2(1, 1);

        // Set goal position in the upper-right corner
        goalPos = new Vector2(width - 3, height - 3);

        // Ensure both start and goal are on paths
        maze[(int)startPos.x, (int)startPos.y] = 1;
        maze[(int)goalPos.x, (int)goalPos.y] = 1;

        // Instantiate start and goal prefabs
        Instantiate(startPrefab, new Vector2(startPos.x, startPos.y), Quaternion.identity, transform);
        Instantiate(goalPrefab, new Vector2(goalPos.x, goalPos.y), Quaternion.identity, transform);
    }

    // Draw the maze using the wall and floor prefabs
    void DrawMaze()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (maze[x, y] == 0)
                {
                    // Instantiate wall prefab at the wall position
                    Instantiate(wallPrefab, new Vector2(x, y), Quaternion.identity, transform);
                }
                else
                {
                    // Instantiate floor prefab at the path position
                    Instantiate(floorPrefab, new Vector2(x, y), Quaternion.identity, transform);
                }
            }
        }
    }

    // Shuffle an array (used for randomizing directions)
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
