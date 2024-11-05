using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SmartMazeSolvingAgent : MonoBehaviour
{
    public float movementSpeed = 2f;
    public TextMeshProUGUI stepText; // Text reference for displaying steps and timer
    private int[,] maze;
    private Vector2Int start;
    private Vector2Int goal;
    private int width;
    private int height;

    private int stepsTaken = 0;
    private float timer = 0f;
    private bool timerStarted = false;

    void Start()
    {
        // Check if stepText is assigned
        if (stepText == null)
        {
            Debug.LogWarning("stepText was not assigned in Inspector. Attempting to find it in the scene.");
            stepText = FindObjectOfType<TextMeshProUGUI>();
        }

        // Check if stepText was successfully found
        if (stepText != null)
        {
            Debug.Log("stepText found and assigned in Start.");
        }
        else
        {
            Debug.LogError("stepText could not be found in Start. Please check if a TextMeshProUGUI component is in the scene.");
        }
    }

    public void InitializeWithMazeData(int[,] maze, Vector2Int start, Vector2Int goal, int width, int height)
    {
        this.maze = maze;
        this.start = start;
        this.goal = goal;
        this.width = width;
        this.height = height;

        if (stepText == null)
        {
            stepText = FindObjectOfType<TextMeshProUGUI>();
        }

        if (stepText == null)
        {
            Debug.LogError("stepText is not assigned and could not be found in the scene.");
        }
        else
        {
            Debug.Log("stepText found and assigned.");
        }

        StartCoroutine(AStarSolveMaze());
    }

    private void Update()
    {
        if (timerStarted)
        {
            timer += Time.deltaTime;
            UpdateDisplayText();
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

    IEnumerator AStarSolveMaze()
    {
        Debug.Log("Starting A* pathfinding...");
        timerStarted = true; // Start the timer

        // Priority queue and dictionaries for A* algorithm
        var openSet = new PriorityQueue<Vector2Int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, int>();
        var fScore = new Dictionary<Vector2Int, int>();

        openSet.Enqueue(start, 0);
        gScore[start] = 0;
        fScore[start] = GetManhattanDistance(start, goal);

        while (openSet.Count > 0)
        {
            Vector2Int current = openSet.Dequeue();

            if (current == goal)
            {
                StopTimer();
                Debug.Log("Goal Reached!");
                var path = ReconstructPath(cameFrom, current);
                StartCoroutine(MoveAlongPath(path));
                yield break;
            }

            foreach (Vector2Int neighbor in GetNeighbors(current))
            {
                int tentativeGScore = gScore[current] + 1;

                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + GetManhattanDistance(neighbor, goal);

                    if (!openSet.Contains(neighbor))
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                }
            }

            stepsTaken++; // Update steps taken
            UpdateDisplayText(); // Force update display after each step
            yield return null;
        }

        Debug.Log("No Solution Found");
    }

    private int GetManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private IEnumerable<Vector2Int> GetNeighbors(Vector2Int cell)
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var dir in directions)
        {
            Vector2Int neighbor = cell + dir;
            if (IsInBounds(neighbor) && maze[neighbor.x, neighbor.y] == 1)
                yield return neighbor;
        }
    }

    private bool IsInBounds(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < width && cell.y >= 0 && cell.y < height;
    }

    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        var path = new List<Vector2Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }
        path.Reverse();
        return path;
    }

    private IEnumerator MoveAlongPath(List<Vector2Int> path)
    {
        foreach (var position in path)
        {
            Vector3 worldPosition = new Vector3(position.x, 0, position.y); // Convert to world position if needed
            while (Vector3.Distance(transform.position, worldPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, worldPosition, movementSpeed * Time.deltaTime);
                yield return null;
            }
            stepsTaken++;
            UpdateDisplayText();
        }
        Debug.Log("Agent has reached the goal.");
    }
}
