using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentMazeRefinement : MonoBehaviour
{
    public GameObject agentPrefab;

    private DFSMazeGenerator mazeGenerator;
    private int[,] maze;
    public int numAgents = 3;
    public float stepDelay = 0.001f;
    private Vector2Int startPos;
    private Vector2Int goalPos;

    private List<Vector2Int> deadEnds = new List<Vector2Int>();

    void Start()
    {
        mazeGenerator = FindObjectOfType<DFSMazeGenerator>();

        // Access the generated maze and start/goal positions
        maze = mazeGenerator.maze;
        startPos = mazeGenerator.startPos;
        goalPos = mazeGenerator.goalPos;

        IdentifyDeadEnds();

        // Shuffle dead ends to randomize the selection
        ShuffleDeadEnds();

        // Remove start and goal from dead ends
        deadEnds.Remove(startPos);
        deadEnds.Remove(goalPos);

        // Assign each agent a dead end and close it
        StartCoroutine(AssignAgentsToCloseDeadEnds());
    }

    // Identify all dead ends in the maze
    void IdentifyDeadEnds()
    {
        for (int x = 1; x < mazeGenerator.width - 1; x++)
        {
            for (int y = 1; y < mazeGenerator.height - 1; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                if (maze[x, y] == 1 && IsDeadEnd(pos) && pos != startPos && pos != goalPos)
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
            Destroy(agentObj);
        }
        mazeGenerator.SetStartAndGoal();
    }

    // Coroutine to close a dead end by filling it with walls
    IEnumerator CloseDeadEnd(Vector2Int startPos, GameObject agentObj)
    {
        Vector2Int currentPos = startPos;

        while (IsDeadEnd(currentPos))
        {
            maze[currentPos.x, currentPos.y] = 0;
            mazeGenerator.DrawMaze();  // Update the visual maze

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
        if (pos.x < mazeGenerator.width - 2 && maze[pos.x + 1, pos.y] == 1) neighbors.Add(new Vector2Int(pos.x + 1, pos.y));
        if (pos.y > 1 && maze[pos.x, pos.y - 1] == 1) neighbors.Add(new Vector2Int(pos.x, pos.y - 1));
        if (pos.y < mazeGenerator.height - 2 && maze[pos.x, pos.y + 1] == 1) neighbors.Add(new Vector2Int(pos.x, pos.y + 1));

        return neighbors.Count == 1 ? neighbors[0] : Vector2Int.zero;
    }

    // Check if the current position is a dead end
    bool IsDeadEnd(Vector2Int pos)
    {
        int pathCount = 0;
        if (pos.x > 1 && maze[pos.x - 1, pos.y] == 1) pathCount++;
        if (pos.x < mazeGenerator.width - 2 && maze[pos.x + 1, pos.y] == 1) pathCount++;
        if (pos.y > 1 && maze[pos.x, pos.y - 1] == 1) pathCount++;
        if (pos.y < mazeGenerator.height - 2 && maze[pos.x, pos.y + 1] == 1) pathCount++;

        return pathCount == 1;
    }

    //// Set start and goal positions
    //void SetStartAndGoal()
    //{
    //    startPos = new Vector2(1, 1);
    //    goalPos = new Vector2(mazeGenerator.width - 3, mazeGenerator.height - 3);
    //    maze[(int)startPos.x, (int)startPos.y] = 1;
    //    maze[(int)goalPos.x, (int)goalPos.y] = 1;
    //    Instantiate(startPrefab, new Vector2(startPos.x, startPos.y), Quaternion.identity, transform);
    //    Instantiate(goalPrefab, new Vector2(goalPos.x, goalPos.y), Quaternion.identity, transform);
    //}

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
