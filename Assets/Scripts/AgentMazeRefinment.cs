using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentMazeRefinement : MonoBehaviour
{
    public GameObject agentPrefab;

    private int[,] maze;
    public int numAgents = 3;
    public float stepDelay = 0.001f;
    private Vector2Int startPos;
    private Vector2Int goalPos;
    private int width;
    private int height;

    private List<Vector2Int> deadEnds = new List<Vector2Int>();

    // Method to initialize maze data from MazeManager
    public void InitializeWithMazeData(int[,] maze, Vector2Int start, Vector2Int goal, int width, int height)
    {
        // Check for null references before assigning values
        if (maze == null)
        {
            Debug.LogError("Maze data is null in AgentMazeRefinement.");
            return;
        }

        this.maze = maze;
        this.startPos = start;
        this.goalPos = goal;
        this.width = width;
        this.height = height;

        IdentifyDeadEnds();

        // Shuffle dead ends to randomize the selection
        ShuffleDeadEnds();

        // Remove start and goal from dead ends
        deadEnds.Remove(startPos);
        deadEnds.Remove(goalPos);

        // Start the refinement process
        StartCoroutine(AssignAgentsToCloseDeadEnds());
    }

    // Identify all dead ends in the maze
    void IdentifyDeadEnds()
    {
        // Prevent null reference errors
        if (maze == null)
        {
            Debug.LogError("Maze not initialized in IdentifyDeadEnds.");
            return;
        }

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                // Identify dead ends (path cells with only one neighbor) and add to deadEnds list
                if (maze[x, y] == 1 && IsDeadEnd(pos))
                {
                    deadEnds.Add(pos);
                }
            }
        }

        // Ensure startPos and goalPos are not in the list of dead ends to fill
        deadEnds.Remove(startPos);
        deadEnds.Remove(goalPos);

        // Log the list of dead ends, start, and goal for debugging
        Debug.Log($"Dead Ends Count: {deadEnds.Count}");
        Debug.Log($"Start Position: {startPos}");
        Debug.Log($"Goal Position: {goalPos}");
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
            Destroy(agentObj);
        }

        // Once agents are done, place the start and goal markers
        MazeManager.Instance.PlaceStartAndGoal();
    }

    // Coroutine to close a dead end by filling it with walls
    IEnumerator CloseDeadEnd(Vector2Int startPos, GameObject agentObj)
    {
        Vector2Int currentPos = startPos;

        while (IsDeadEnd(currentPos))
        {
            maze[currentPos.x, currentPos.y] = 0;
            MazeManager.Instance.UpdateVisualMaze(maze);  // Update the visual maze through MazeManager

            agentObj.transform.position = new Vector2(currentPos.x, currentPos.y);
            yield return new WaitForSeconds(stepDelay);

            currentPos = GetNextInDeadEnd(currentPos);
            if (currentPos == Vector2Int.zero) break;
        }
    }

    // Get the next tile in the dead end path
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

    // Shuffle the dead ends
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
