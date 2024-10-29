using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject initialPathPrefab;
    public GameObject updatedPathPrefab;
    public GameObject agentPrefab;

    public float generationDelay = 0.1f;
    public float agentDelay = 0.1f;
    [SerializeField] private int deadEndsToOpen = 0;

    private int[,] maze;
    private bool[,] visited;
    private List<Vector2> initialSolutionPath = new List<Vector2>();
    private List<Vector2> updatedSolutionPath = new List<Vector2>();

    void Start()
    {
        if (width % 2 == 0) width++;
        if (height % 2 == 0) height++;

        maze = new int[width, height];
        visited = new bool[width, height];

        StartCoroutine(GenerateMaze());
    }

    IEnumerator GenerateMaze()
    {
        yield return StartCoroutine(RecursiveBacktrack(1, 1));
        DrawFullMaze();

        // Solve and count tiles for the initial solution
        int initialTileCount = SolveMazeBFS(1, 1, initialSolutionPath);
        if (initialTileCount > 0)
        {
            UnityEngine.Debug.Log("Initial Solution Tile Count: " + initialTileCount);
            DrawInitialSolution();
        }

        // Open dead ends with delay for visualization
        yield return StartCoroutine(OpenDeadEnds(deadEndsToOpen));

        // Solve and count tiles for the updated solution
        ResetVisitedArray();
        int updatedTileCount = SolveMazeBFS(1, 1, updatedSolutionPath);
        if (updatedTileCount > 0)
        {
            UnityEngine.Debug.Log("Updated Solution Tile Count: " + updatedTileCount);
            DrawUpdatedSolution();
        }
    }

    IEnumerator RecursiveBacktrack(int x, int y)
    {
        maze[x, y] = 1;
        DrawMazeStep(x, y);
        yield return new WaitForSeconds(generationDelay);

        int[] directions = { 1, 2, 3, 4 };
        ShuffleArray(directions);

        foreach (int direction in directions)
        {
            switch (direction)
            {
                case 1:
                    if (y - 2 > 0 && maze[x, y - 2] == 0)
                    {
                        maze[x, y - 1] = 1;
                        DrawMazeStep(x, y - 1);
                        yield return StartCoroutine(RecursiveBacktrack(x, y - 2));
                    }
                    break;
                case 2:
                    if (y + 2 < height - 1 && maze[x, y + 2] == 0)
                    {
                        maze[x, y + 1] = 1;
                        DrawMazeStep(x, y + 1);
                        yield return StartCoroutine(RecursiveBacktrack(x, y + 2));
                    }
                    break;
                case 3:
                    if (x - 2 > 0 && maze[x - 2, y] == 0)
                    {
                        maze[x - 1, y] = 1;
                        DrawMazeStep(x - 1, y);
                        yield return StartCoroutine(RecursiveBacktrack(x - 2, y));
                    }
                    break;
                case 4:
                    if (x + 2 < width - 1 && maze[x + 2, y] == 0)
                    {
                        maze[x + 1, y] = 1;
                        DrawMazeStep(x + 1, y);
                        yield return StartCoroutine(RecursiveBacktrack(x + 2, y));
                    }
                    break;
            }
        }
    }

    IEnumerator OpenDeadEnds(int numberOfDeadEndsToOpen)
    {
        List<Vector2Int> deadEnds = new List<Vector2Int>();

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (maze[x, y] == 1 && CountOpenNeighbors(x, y) == 1)
                {
                    deadEnds.Add(new Vector2Int(x, y));
                }
            }
        }

        var randomDeadEnds = deadEnds.OrderBy(x => Random.value).Take(numberOfDeadEndsToOpen).ToList();

        // Skapa en agent-instans
        GameObject agent = Instantiate(agentPrefab, Vector2.zero, Quaternion.identity);

        foreach (var deadEnd in randomDeadEnds)
        {
            agent.transform.position = new Vector2(deadEnd.x, deadEnd.y); // Flytta agenten till dead end
            yield return new WaitForSeconds(agentDelay); // Fördröjning för visualisering

            OpenDeadEnd(deadEnd.x, deadEnd.y);
        }

        Destroy(agent); // Ta bort agenten när arbetet är klart
    }

    void DrawMazeStep(int x, int y)
    {
        // Draw the floor (path) at this position
        Vector2 position = new Vector2(x, y);
        Instantiate(floorPrefab, position, Quaternion.identity, transform);

        // Draw walls around the paths where there are no paths
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (x + i >= 0 && x + i < width && y + j >= 0 && y + j < height && maze[x + i, y + j] == 0)
                {
                    Instantiate(wallPrefab, new Vector2(x + i, y + j), Quaternion.identity, transform);
                }
            }
        }
    }

    void OpenDeadEnd(int x, int y)
    {
        List<Vector2Int> possibleOpenings = new List<Vector2Int>();

        if (x > 1 && maze[x - 1, y] == 0 && maze[x - 2, y] == 1) possibleOpenings.Add(new Vector2Int(x - 1, y));
        if (x < width - 2 && maze[x + 1, y] == 0 && maze[x + 2, y] == 1) possibleOpenings.Add(new Vector2Int(x + 1, y));
        if (y > 1 && maze[x, y - 1] == 0 && maze[x, y - 2] == 1) possibleOpenings.Add(new Vector2Int(x, y - 1));
        if (y < height - 2 && maze[x, y + 1] == 0 && maze[x, y + 2] == 1) possibleOpenings.Add(new Vector2Int(x, y + 1));

        if (possibleOpenings.Count > 0)
        {
            var chosenOpening = possibleOpenings[Random.Range(0, possibleOpenings.Count)];
            maze[chosenOpening.x, chosenOpening.y] = 1;
            Instantiate(floorPrefab, new Vector2(chosenOpening.x, chosenOpening.y), Quaternion.identity);
        }
    }

    int SolveMazeBFS(int startX, int startY, List<Vector2> solutionPath)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        queue.Enqueue(new Vector2Int(startX, startY));
        cameFrom[new Vector2Int(startX, startY)] = new Vector2Int(-1, -1);

        Vector2Int end = new Vector2Int(width - 2, height - 2);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current == end)
            {
                while (current != new Vector2Int(-1, -1))
                {
                    solutionPath.Add(new Vector2(current.x, current.y));
                    current = cameFrom[current];
                }
                solutionPath.Reverse();
                return solutionPath.Count;
            }

            foreach (var direction in new Vector2Int[] {
                new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0) })
            {
                Vector2Int neighbor = current + direction;
                if (neighbor.x >= 0 && neighbor.x < width && neighbor.y >= 0 && neighbor.y < height &&
                    maze[neighbor.x, neighbor.y] == 1 && !cameFrom.ContainsKey(neighbor))
                {
                    queue.Enqueue(neighbor);
                    cameFrom[neighbor] = current;
                }
            }
        }

        return 0;
    }

    void DrawInitialSolution()
    {
        foreach (Vector2 position in initialSolutionPath)
        {
            Instantiate(initialPathPrefab, position, Quaternion.identity, transform);
        }
    }

    void DrawUpdatedSolution()
    {
        foreach (Vector2 position in updatedSolutionPath)
        {
            Instantiate(updatedPathPrefab, position, Quaternion.identity, transform);
        }
    }

    void ResetVisitedArray()
    {
        for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
                visited[i, j] = false;
    }

    int CountOpenNeighbors(int x, int y)
    {
        int openNeighbors = 0;
        if (maze[x + 1, y] == 1) openNeighbors++;
        if (maze[x - 1, y] == 1) openNeighbors++;
        if (maze[x, y + 1] == 1) openNeighbors++;
        if (maze[x, y - 1] == 1) openNeighbors++;
        return openNeighbors;
    }

    void DrawFullMaze()
    {
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
