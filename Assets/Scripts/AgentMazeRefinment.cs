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
    public int agentSteps = 50;  // Number of steps each agent will take
    public float stepDelay = 0.001f; // Delay between each agent step for animation
    private Vector2 startPos;
    private Vector2 goalPos;

    private List<GameObject> agentObjects = new List<GameObject>(); // List to keep track of agent objects

    void Start()
    {
        // Assuming the DFS-generated maze is already created (replace with your generation code)
        maze = new int[width, height];
        GenerateMazeUsingDFS();
        DrawMaze();

        // Start the agent-based maze refinement with animation
        StartCoroutine(RefineMazeWithAgentsAnimated());
    }

    // Maze generation using DFS (placeholder - you can replace this with your DFS generation code)
    void GenerateMazeUsingDFS()
    {
        // Fill the maze array with walls (0) and paths (1)
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                maze[x, y] = 0;  // Initialize all as walls
            }
        }

        // Carve out the maze using DFS (this is a placeholder, replace with your DFS code)
        RecursiveDFS(1, 1);
    }

    // Recursive DFS algorithm (as before)
    void RecursiveDFS(int x, int y)
    {
        maze[x, y] = 1;  // Mark cell as part of the path
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

    // Coroutine to animate agent-based maze refinement
    IEnumerator RefineMazeWithAgentsAnimated()
    {
        for (int i = 0; i < numAgents; i++)
        {
            // Each agent starts at a random point
            Vector2Int agentPos = new Vector2Int(Random.Range(1, width - 2), Random.Range(1, height - 2));

            // Create a visual agent prefab to follow the agent
            GameObject agentObj = Instantiate(agentPrefab, new Vector2(agentPos.x, agentPos.y), Quaternion.identity);
            agentObjects.Add(agentObj); // Keep track of the agent object

            // Each agent takes a number of steps
            for (int j = 0; j < agentSteps; j++)
            {
                // Get a random neighboring position
                Vector2Int nextPos = GetRandomNeighbor(agentPos);

                // Modify the maze at the current agent's position intelligently
                ModifyMazeIntelligently(agentPos, nextPos);

                // Move the visual agent to the new position
                agentObj.transform.position = new Vector2(nextPos.x, nextPos.y);

                // Wait for a short delay before the next step
                yield return new WaitForSeconds(stepDelay);

                // Move agent to the next position
                agentPos = nextPos;

                // Redraw the maze to reflect the modification
                DrawMaze();
            }
        }
        SetStartAndGoal();
    }

    // Get a random neighboring position for the agent to move to
    Vector2Int GetRandomNeighbor(Vector2Int pos)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        // Ensure neighbors are within bounds
        if (pos.x > 1) neighbors.Add(new Vector2Int(pos.x - 1, pos.y));  // Left
        if (pos.x < width - 2) neighbors.Add(new Vector2Int(pos.x + 1, pos.y));  // Right
        if (pos.y > 1) neighbors.Add(new Vector2Int(pos.x, pos.y - 1));  // Down
        if (pos.y < height - 2) neighbors.Add(new Vector2Int(pos.x, pos.y + 1));  // Up

        // Pick a random neighbor
        return neighbors[Random.Range(0, neighbors.Count)];
    }

    // Intelligently modify the maze at the agent's current and next position
    void ModifyMazeIntelligently(Vector2Int currentPos, Vector2Int nextPos)
    {
        // If nextPos is a wall, carve it into a path
        if (maze[nextPos.x, nextPos.y] == 0)
        {
            maze[nextPos.x, nextPos.y] = 1;  // Carve path
        }
        else
        {
            // Purposefully close the path (create a dead end) with a certain probability
            if (IsDeadEnd(currentPos) && Random.value > 0.7f)
            {
                maze[nextPos.x, nextPos.y] = 0;  // Create a wall, making it a dead end
            }
            // Purposefully connect to create loops if there are enough nearby paths
            else if (IsNearAnotherPath(nextPos) && Random.value > 0.5f)
            {
                maze[nextPos.x, nextPos.y] = 1;  // Create a loop by connecting paths
            }
        }
    }

    // Check if the current position is a dead end
    bool IsDeadEnd(Vector2Int pos)
    {
        int pathCount = 0;

        // Check all 4 neighbors (up, down, left, right)
        if (pos.x > 1 && maze[pos.x - 1, pos.y] == 1) pathCount++;
        if (pos.x < width - 2 && maze[pos.x + 1, pos.y] == 1) pathCount++;
        if (pos.y > 1 && maze[pos.x, pos.y - 1] == 1) pathCount++;
        if (pos.y < height - 2 && maze[pos.x, pos.y + 1] == 1) pathCount++;

        // If only one neighboring path, it is a dead end
        return pathCount == 1;
    }

    // Check if the next position is near another path (for creating loops)
    bool IsNearAnotherPath(Vector2Int pos)
    {
        int pathCount = 0;

        // Check all 4 neighbors (up, down, left, right)
        if (pos.x > 1 && maze[pos.x - 1, pos.y] == 1) pathCount++;
        if (pos.x < width - 2 && maze[pos.x + 1, pos.y] == 1) pathCount++;
        if (pos.y > 1 && maze[pos.x, pos.y - 1] == 1) pathCount++;
        if (pos.y < height - 2 && maze[pos.x, pos.y + 1] == 1) pathCount++;

        // If there are 2 or more paths nearby, it’s a good candidate for creating a loop
        return pathCount >= 2;
    }

    // Draw the maze using wall and floor prefabs
    void DrawMaze()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject); // Clear the old maze
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (maze[x, y] == 0)
                {
                    Instantiate(wallPrefab, new Vector2(x, y), Quaternion.identity, transform);  // Wall
                }
                else
                {
                    Instantiate(floorPrefab, new Vector2(x, y), Quaternion.identity, transform);  // Path
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
}
