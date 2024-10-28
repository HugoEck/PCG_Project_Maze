using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentMazeRefinement : MonoBehaviour
{
    public GameObject wallPrefab;  // Wall prefab
    public GameObject floorPrefab; // Floor prefab
    public GameObject agentPrefab; // Agent prefab for visualization
    public GameObject startPrefab;  // Start point prefab
    public GameObject goalPrefab;   // Goal point prefab

    private int[,] maze;  // Maze array from DFS generation
    public int width = 10;  // Maze width
    public int height = 10; // Maze height

    public int numAgents = 3;  // Number of agents to refine the maze
    public float stepDelay = 0.001f; // Delay between each agent step for animation
    private Vector2 startPos;
    private Vector2 goalPos;

    private List<Vector2Int> deadEnds = new List<Vector2Int>(); // List to store dead ends

    void Start()
    {
        maze = new int[width, height];
        GenerateMazeUsingDFS();
        DrawMaze();

        // Identify dead ends after DFS generation
        IdentifyDeadEnds();

        // Shuffle dead ends to randomize the selection
        ShuffleDeadEnds();

        SetStartAndGoal();

        // Remove the goal position from the list, if it somehow exists there
        deadEnds.Remove(new Vector2Int((int)startPos.x, (int)startPos.y));

        // Remove the goal position from the list, if it somehow exists there
        deadEnds.Remove(new Vector2Int((int)goalPos.x, (int)goalPos.y));

        // Assign each agent a dead end and close it
        StartCoroutine(AssignAgentsToCloseDeadEnds());
    }

    // Maze generation using DFS (placeholder)
    void GenerateMazeUsingDFS()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                maze[x, y] = 0;  // Initialize all as walls
            }
        }

        RecursiveDFS(1, 1);
    }

    // Recursive DFS algorithm for maze generation
    void RecursiveDFS(int x, int y)
    {
        maze[x, y] = 1;
        int[] directions = { 1, 2, 3, 4 };
        ShuffleArray(directions);
        foreach (int direction in directions)
        {
            switch (direction)
            {
                case 1:  // Up
                    if (y - 2 > 0 && maze[x, y - 2] == 0)
                    {
                        maze[x, y - 1] = 1;
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

    // Identify all dead ends in the maze
    void IdentifyDeadEnds()
    {
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                // Exclude the start and goal positions from the dead ends
                if (maze[x, y] == 1 && IsDeadEnd(pos) && pos != new Vector2Int((int)startPos.x, (int)startPos.y) && pos != new Vector2Int((int)goalPos.x, (int)goalPos.y))
                {
                    deadEnds.Add(pos);
                }
            }
        }
    }

    // Coroutine to assign agents to close dead ends
    IEnumerator AssignAgentsToCloseDeadEnds()
    {
        int assignedAgents = Mathf.Min(numAgents, deadEnds.Count);

        for (int i = 0; i < assignedAgents; i++)
        {
            Vector2Int deadEndPos = deadEnds[i];
            GameObject agentObj = Instantiate(agentPrefab, new Vector2(deadEndPos.x, deadEndPos.y), Quaternion.identity);

            yield return StartCoroutine(CloseDeadEnd(deadEndPos, agentObj));
            Destroy(agentObj);  // Remove the agent object after closing the dead end
        }

        SetStartAndGoal();
    }

    // Coroutine to close a dead end by filling it with walls
    IEnumerator CloseDeadEnd(Vector2Int startPos, GameObject agentObj)
    {
        Vector2Int currentPos = startPos;

        while (IsDeadEnd(currentPos))
        {
            maze[currentPos.x, currentPos.y] = 0;  // Turn path into a wall
            DrawMaze();  // Update the visual maze

            // Move the visual agent to the current position
            agentObj.transform.position = new Vector2(currentPos.x, currentPos.y);
            yield return new WaitForSeconds(stepDelay);

            // Move to the next tile in the dead end
            currentPos = GetNextInDeadEnd(currentPos);
            if (currentPos == Vector2Int.zero) break;  // Stop if no valid next step is found
        }
    }

    // Find the next tile in a dead end path (returns Vector2Int.zero if no valid next step)
    Vector2Int GetNextInDeadEnd(Vector2Int pos)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        if (pos.x > 1 && maze[pos.x - 1, pos.y] == 1) neighbors.Add(new Vector2Int(pos.x - 1, pos.y));
        if (pos.x < width - 2 && maze[pos.x + 1, pos.y] == 1) neighbors.Add(new Vector2Int(pos.x + 1, pos.y));
        if (pos.y > 1 && maze[pos.x, pos.y - 1] == 1) neighbors.Add(new Vector2Int(pos.x, pos.y - 1));
        if (pos.y < height - 2 && maze[pos.x, pos.y + 1] == 1) neighbors.Add(new Vector2Int(pos.x, pos.y + 1));

        return neighbors.Count == 1 ? neighbors[0] : Vector2Int.zero;
    }

    // Check if the current position is a dead end
    bool IsDeadEnd(Vector2Int pos)
    {
        int pathCount = 0;
        if (pos.x > 1 && maze[pos.x - 1, pos.y] == 1) pathCount++;
        if (pos.x < width - 2 && maze[pos.x + 1, pos.y] == 1) pathCount++;
        if (pos.y > 1 && maze[pos.x, pos.y - 1] == 1) pathCount++;
        if (pos.y < height - 2 && maze[pos.x, pos.y + 1] == 1) pathCount++;

        return pathCount == 1;
    }

    // Set start and goal positions
    void SetStartAndGoal()
    {
        startPos = new Vector2(1, 1);
        goalPos = new Vector2(width - 3, height - 3);
        maze[(int)startPos.x, (int)startPos.y] = 1;
        maze[(int)goalPos.x, (int)goalPos.y] = 1;
        Instantiate(startPrefab, new Vector2(startPos.x, startPos.y), Quaternion.identity, transform);
        Instantiate(goalPrefab, new Vector2(goalPos.x, goalPos.y), Quaternion.identity, transform);
    }

    // Draw the maze using wall and floor prefabs
    void DrawMaze()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

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

    // Shuffle the directions array (used in DFS)
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

    void ShuffleDeadEnds()
    {
        for (int i = deadEnds.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Vector2Int temp = deadEnds[i];
            deadEnds[i] = deadEnds[randomIndex];
            deadEnds[randomIndex] = temp;
        }
    }
}
