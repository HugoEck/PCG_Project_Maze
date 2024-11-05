using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AgentMazeRefinement : MonoBehaviour
{
    public GameObject agentPrefab;

    private int[,] maze;
    public int numAgents = 3;
    public float closeStepDelay = 0.1f;
    public float openStepDelay = 0.1f; 
    private Vector2Int startPos;
    private Vector2Int goalPos;
    private int width;
    private int height;
    public bool IsDone { get; private set; } = false;

    public enum ActionType
    {
        Close, 
        Open,          
        CloseAndOpen     
    }

    public ActionType actionType = ActionType.Close;

    private List<Vector2Int> initialDeadEnds = new List<Vector2Int>();
    private List<Vector2Int> secondaryDeadEnds = new List<Vector2Int>();


    public void InitializeWithMazeData(int[,] maze, Vector2Int start, Vector2Int goal)
    {
        if (maze == null)
        {
            Debug.LogError("Maze data is null in AgentMazeRefinement.");
            return;
        }

        numAgents = PlayerPrefs.GetInt("numAgents", 0);
        actionType = (ActionType)PlayerPrefs.GetInt("actionType", (int)ActionType.Close);

        this.maze = maze;
        this.startPos = start;
        this.goalPos = goal;
        this.width = maze.GetLength(0);
        this.height = maze.GetLength(1);

        IdentifyInitialDeadEnds();
        ShuffleDeadEnds(initialDeadEnds);
        initialDeadEnds.Remove(startPos);
        initialDeadEnds.Remove(goalPos);

        if (numAgents > 0)
        {
            IsDone = false;
            StartCoroutine(AssignAgentsToHandleDeadEnds());
        }
        else
        {
            IsDone = true; 
        }
    }

    void IdentifyInitialDeadEnds()
    {
        initialDeadEnds.Clear();
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (maze[x, y] == 1 && IsDeadEnd(pos))
                {
                    initialDeadEnds.Add(pos);
                }
            }
        }
        Debug.Log("Total deadends:" + initialDeadEnds.Count);
    }

    void IdentifySecondaryDeadEnds()
    {
        secondaryDeadEnds.Clear();
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (maze[x, y] == 1 && IsDeadEnd(pos))
                {
                    secondaryDeadEnds.Add(pos);
                }
            }
        }
    }

    IEnumerator AssignAgentsToHandleDeadEnds()
    {
        int assignedAgents = Mathf.Min(numAgents, initialDeadEnds.Count);

        Debug.Log($"Assigning {assignedAgents} agents to handle dead ends.");

        for (int i = 0; i < assignedAgents; i++)
        {
            Vector2Int deadEndPos = initialDeadEnds[i];
            GameObject agentObj = Instantiate(agentPrefab, new Vector2(deadEndPos.x, deadEndPos.y), Quaternion.identity);
            Debug.Log($"Agent {i} instantiated at position {deadEndPos}");

            // Use the enum to determine the action
            switch (actionType)
            {
                case ActionType.Close:
                    yield return StartCoroutine(CloseDeadEnd(deadEndPos, agentObj));
                    break;

                case ActionType.Open:
                    yield return StartCoroutine(OpenDeadEnd(deadEndPos, agentObj));
                    break;

                case ActionType.CloseAndOpen:
                    yield return StartCoroutine(CloseDeadEnd(deadEndPos, agentObj));
                    yield return new WaitForSeconds(closeStepDelay);
                    IdentifySecondaryDeadEnds();
                    if (secondaryDeadEnds.Count > i)
                        yield return StartCoroutine(OpenDeadEnd(secondaryDeadEnds[i], agentObj));
                    break;
            }

            Debug.Log($"Agent {i} finished its task.");
            Destroy(agentObj);
        }


        goalPos = MazeManager.Instance.FindFurthestPathFromStart(startPos);
        MazeManager.Instance.PlaceStartAndGoal();

        IsDone = true;
    }

    IEnumerator CloseDeadEnd(Vector2Int startPos, GameObject agentObj)
    {
        Vector2Int currentPos = startPos;

        while (IsDeadEnd(currentPos))
        {
            maze[currentPos.x, currentPos.y] = 0;
            MazeManager.Instance.UpdateVisualMaze(maze);
            agentObj.transform.position = new Vector2(currentPos.x, currentPos.y);
            Debug.Log($"Closing dead end at position {currentPos}");
            yield return new WaitForSeconds(closeStepDelay);

            currentPos = GetNextInDeadEnd(currentPos);
            if (currentPos == Vector2Int.zero) break;
        }
    }

    IEnumerator OpenDeadEnd(Vector2Int startPos, GameObject agentObj)
    {
        Vector2Int currentPos = startPos;

        while (IsDeadEnd(currentPos))
        {
            Vector2Int opening = GetOpeningForDeadEnd(currentPos);
            if (opening != Vector2Int.zero)
            {
                maze[opening.x, opening.y] = 1;
                MazeManager.Instance.UpdateVisualMaze(maze);
                agentObj.transform.position = new Vector2(opening.x, opening.y);
                Debug.Log($"Opening dead end at position {opening}");
                yield return new WaitForSeconds(openStepDelay);
            }
            else
            {
                break;
            }
            currentPos = opening;
        }
    }

    Vector2Int GetOpeningForDeadEnd(Vector2Int pos)
    {
        List<Vector2Int> possibleOpenings = new List<Vector2Int>();

        if (pos.x > 1 && maze[pos.x - 1, pos.y] == 0 && maze[pos.x - 2, pos.y] == 1)
            possibleOpenings.Add(new Vector2Int(pos.x - 1, pos.y));
        if (pos.x < width - 2 && maze[pos.x + 1, pos.y] == 0 && maze[pos.x + 2, pos.y] == 1)
            possibleOpenings.Add(new Vector2Int(pos.x + 1, pos.y));
        if (pos.y > 1 && maze[pos.x, pos.y - 1] == 0 && maze[pos.x, pos.y - 2] == 1)
            possibleOpenings.Add(new Vector2Int(pos.x, pos.y - 1));
        if (pos.y < height - 2 && maze[pos.x, pos.y + 1] == 0 && maze[pos.x, pos.y + 2] == 1)
            possibleOpenings.Add(new Vector2Int(pos.x, pos.y + 1));

        if (possibleOpenings.Count > 0)
        {
            return possibleOpenings[Random.Range(0, possibleOpenings.Count)];
        }
        return Vector2Int.zero;
    }

    Vector2Int GetNextInDeadEnd(Vector2Int pos)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        if (pos.x > 1 && maze[pos.x - 1, pos.y] == 1) neighbors.Add(new Vector2Int(pos.x - 1, pos.y));
        if (pos.x < width - 2 && maze[pos.x + 1, pos.y] == 1) neighbors.Add(new Vector2Int(pos.x + 1, pos.y));
        if (pos.y > 1 && maze[pos.x, pos.y - 1] == 1) neighbors.Add(new Vector2Int(pos.x, pos.y - 1));
        if (pos.y < height - 2 && maze[pos.x, pos.y + 1] == 1) neighbors.Add(new Vector2Int(pos.x, pos.y + 1));

        return neighbors.Count == 1 ? neighbors[0] : Vector2Int.zero;
    }

    bool IsDeadEnd(Vector2Int pos)
    {
        int pathCount = 0;
        if (pos.x > 1 && maze[pos.x - 1, pos.y] == 1) pathCount++;
        if (pos.x < width - 2 && maze[pos.x + 1, pos.y] == 1) pathCount++;
        if (pos.y > 1 && maze[pos.x, pos.y - 1] == 1) pathCount++;
        if (pos.y < height - 2 && maze[pos.x, pos.y + 1] == 1) pathCount++;

        return pathCount == 1;
    }

    void ShuffleDeadEnds(List<Vector2Int> deadEndsList)
    {
        for (int i = deadEndsList.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Vector2Int temp = deadEndsList[i];
            deadEndsList[i] = deadEndsList[randomIndex];
            deadEndsList[randomIndex] = temp;
        }
    }
}
