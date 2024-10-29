using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject initialPathPrefab; // To show the solution path
    public GameObject updatedPathPrefab; // To show the solution path

    public float generationDelay = 0.1f; // Delay to visualize maze generation

    private int[,] maze;
    private bool[,] visited;
    private List<Vector2> initialSolutionPath = new List<Vector2>();
    private List<Vector2> updatedSolutionPath = new List<Vector2>();

    [SerializeField] private int deadEndsToOpen = 0;

    void Start()
    {
        // Se till att bredd och höjd är udda nummer för att undvika oönskade mönster
        if (width % 2 == 0) width++;
        if (height % 2 == 0) height++;

        maze = new int[width, height];
        visited = new bool[width, height];

        // Starta labyrintgenerering som en coroutine
        StartCoroutine(GenerateMaze());
    }

    IEnumerator GenerateMaze()
    {
        yield return StartCoroutine(RecursiveBacktrack(1, 1));
        DrawFullMaze();

        // Solve and time the initial solution
        float startInitialTime = Time.realtimeSinceStartup;
        if (SolveMaze(1, 1, initialSolutionPath))
        {
            float endInitialTime = Time.realtimeSinceStartup;
            UnityEngine.Debug.Log("Initial Solution Time: " + (endInitialTime - startInitialTime) * 1000 + " ms");
            DrawInitialSolution();
        }

        // Open dead ends with delay for visualization
        yield return StartCoroutine(OpenDeadEnds(deadEndsToOpen));

        // Solve and time the updated solution
        ResetVisitedArray();
        float startUpdatedTime = Time.realtimeSinceStartup;
        if (SolveMaze(1, 1, updatedSolutionPath))
        {
            float endUpdatedTime = Time.realtimeSinceStartup;
            UnityEngine.Debug.Log("Updated Solution Time: " + (endUpdatedTime - startUpdatedTime) * 1000 + " ms");
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

        foreach (var deadEnd in randomDeadEnds)
        {
            OpenDeadEnd(deadEnd.x, deadEnd.y);
            yield return new WaitForSeconds(generationDelay);
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



    // Ritar både gångar och väggar steg för steg
    void DrawMazeStep(int x, int y)
    {
        // Rita golv (gång) på denna position
        Vector2 position = new Vector2(x, y);
        Instantiate(floorPrefab, position, Quaternion.identity);

        // Rita väggar runt gångarna (där väggarna finns)
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (x + i >= 0 && x + i < width && y + j >= 0 && y + j < height && maze[x + i, y + j] == 0)
                {
                    Instantiate(wallPrefab, new Vector2(x + i, y + j), Quaternion.identity);
                }
            }
        }
    }

    // Depth-First Search för att hitta lösningsvägen
    bool SolveMaze(int x, int y, List<Vector2> solutionPath)
    {
        if (x == width - 2 && y == height - 2)
        {
            solutionPath.Add(new Vector2(x, y));
            return true;
        }

        if (IsValidMove(x, y))
        {
            visited[x, y] = true;
            solutionPath.Add(new Vector2(x, y));

            if (SolveMaze(x, y - 1, solutionPath)) return true;
            if (SolveMaze(x, y + 1, solutionPath)) return true;
            if (SolveMaze(x - 1, y, solutionPath)) return true;
            if (SolveMaze(x + 1, y, solutionPath)) return true;

            solutionPath.Remove(new Vector2(x, y));
        }

        return false;
    }

    bool IsValidMove(int x, int y)
    {
        return (x >= 0 && x < width && y >= 0 && y < height && maze[x, y] == 1 && !visited[x, y]);
    }


    // Räknar antalet gångar (öppna celler) som är grannar till en given cell
    int CountOpenNeighbors(int x, int y)
    {
        int openNeighbors = 0;
        if (maze[x + 1, y] == 1) openNeighbors++;
        if (maze[x - 1, y] == 1) openNeighbors++;
        if (maze[x, y + 1] == 1) openNeighbors++;
        if (maze[x, y - 1] == 1) openNeighbors++;
        return openNeighbors;
    }



    // Ritar hela labyrinten när den är klar
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

    // Ritar lösningsvägen med hjälp av pathPrefab
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
    // Slumpar ordningen av riktningar
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
