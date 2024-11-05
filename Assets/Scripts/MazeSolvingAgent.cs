using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Import if using TextMeshPro

public class MazeSolvingAgent : MonoBehaviour
{
    public float movementSpeed = 2f;
    public TextMeshProUGUI stepText; // Reference for steps and time display
    private int[,] maze;
    private Vector2Int start;
    private Vector2Int goal;
    private int width;
    private int height;

    private Stack<Vector2Int> pathStack = new Stack<Vector2Int>();
    private HashSet<Vector2Int> visitedCells = new HashSet<Vector2Int>();
    private bool goalReached = false;

    private int stepsTaken = 0; // Count steps taken
    private float timer = 0f; // Timer to track elapsed time
    private bool timerStarted = true;

    public void InitializeWithMazeData(int[,] maze, Vector2Int start, Vector2Int goal, int width, int height)
    {
        this.maze = maze;
        this.start = start;

        // Set the goal position to the upper-right corner
        this.goal = new Vector2Int(width - 2, height - 2); // Adjust for any outer walls if needed

        this.width = width;
        this.height = height;

        transform.position = new Vector3(start.x, start.y, -1);
        pathStack.Push(start);
        visitedCells.Add(start);
        stepText = FindObjectOfType<TextMeshProUGUI>();
        StartCoroutine(SolveMaze());
    }

    private void Update()
    {
        if (timerStarted)
        {
            timer += Time.deltaTime;
            UpdateDisplayText(); // Update UI every frame
        }
    }

    private void UpdateDisplayText()
    {
        if (stepText != null)
        {
            stepText.text = $"Steps: {stepsTaken} | Time: {timer:F2}s";
        }
    }

    private float StopTimer()
    {
        timerStarted = false;
        return timer;
    }

    IEnumerator SolveMaze()
    {
        while (pathStack.Count > 0 && !goalReached)
        {
            Vector2Int currentPos = pathStack.Peek();

            if (currentPos == goal)
            {
                goalReached = true;
                StopTimer(); // Stop the timer when goal is reached
                Debug.Log("Goal Reached!");
                yield break;
            }

            if (IsGoalInLineOfSight(currentPos))
            {
                yield return StartCoroutine(MoveToPosition(new Vector3(goal.x, goal.y, -1)));
                goalReached = true;
                StopTimer();
                Debug.Log("Goal Reached via line of sight!");
                yield break;
            }

            List<Vector2Int> unvisitedNeighbors = GetUnvisitedNeighbors(currentPos);

            if (unvisitedNeighbors.Count > 0)
            {
                Vector2Int nextPos = unvisitedNeighbors[Random.Range(0, unvisitedNeighbors.Count)];
                pathStack.Push(nextPos);
                visitedCells.Add(nextPos);

                stepsTaken++; // Increment step count for each move
                yield return StartCoroutine(MoveToPosition(new Vector3(nextPos.x, nextPos.y, -1)));
            }
            else
            {
                pathStack.Pop();
                if (pathStack.Count > 0)
                {
                    Vector2Int backtrackPos = pathStack.Peek();
                    stepsTaken++; // Increment step count for each backtrack move
                    yield return StartCoroutine(MoveToPosition(new Vector3(backtrackPos.x, backtrackPos.y, -1)));
                }
            }
        }

        if (!goalReached)
        {
            Debug.Log("No solution found!");
        }
    }

    IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
    }

    private List<Vector2Int> GetUnvisitedNeighbors(Vector2Int cell)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        // Prioritize directions based on proximity to the goal (upper-right preference)
        Vector2Int[] directions = goal.x > cell.x && goal.y > cell.y
            ? new Vector2Int[] { Vector2Int.right, Vector2Int.up, Vector2Int.left, Vector2Int.down }
            : new Vector2Int[] { Vector2Int.up, Vector2Int.right, Vector2Int.left, Vector2Int.down };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighbor = cell + dir;
            if (IsInBounds(neighbor) && maze[neighbor.x, neighbor.y] == 1 && !visitedCells.Contains(neighbor))
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    private bool IsInBounds(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < width && cell.y >= 0 && cell.y < height;
    }

    private bool IsGoalInLineOfSight(Vector2Int currentPos)
    {
        if (currentPos.x == goal.x)
        {
            int minY = Mathf.Min(currentPos.y, goal.y);
            int maxY = Mathf.Max(currentPos.y, goal.y);

            for (int y = minY + 1; y < maxY; y++)
            {
                if (maze[currentPos.x, y] != 1) return false;
            }
            return true;
        }
        else if (currentPos.y == goal.y)
        {
            int minX = Mathf.Min(currentPos.x, goal.x);
            int maxX = Mathf.Max(currentPos.x, goal.x);

            for (int x = minX + 1; x < maxX; x++)
            {
                if (maze[x, currentPos.y] != 1) return false;
            }
            return true;
        }

        return false;
    }
}
