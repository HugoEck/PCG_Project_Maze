using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DFSMazeGenerator : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public GameObject wallPrefab;
    public GameObject pathPrefab;

    private int[,] maze;
    private Stack<Vector2> pathStack = new Stack<Vector2>();

    // Start is called before the first frame update
    void Start()
    {
        maze = new int[width, height];
        GenerateMaze();
    }

    void GenerateMaze()
    {
        // Start at the top-left corner
        Vector2 startPos = new Vector2(1, 1);
        pathStack.Push(startPos);
        maze[(int)startPos.x, (int)startPos.y] = 1; // Mark as path

        while (pathStack.Count > 0)
        {
            Vector2 current = pathStack.Peek();
            List<Vector2> neighbors = GetUnvisitedNeighbors(current);

            if (neighbors.Count > 0)
            {
                Vector2 chosenNeighbor = neighbors[Random.Range(0, neighbors.Count)];
                pathStack.Push(chosenNeighbor);

                // Mark path between the current and chosen neighbor
                Vector2 inBetween = (current + chosenNeighbor) / 2;
                maze[(int)inBetween.x, (int)inBetween.y] = 1; // Mark as path
                maze[(int)chosenNeighbor.x, (int)chosenNeighbor.y] = 1; // Mark as path
            }
            else
            {
                pathStack.Pop();
            }
        }

        // Spawn the maze in the scene
        DrawMaze();
    }

    List<Vector2> GetUnvisitedNeighbors(Vector2 cell)
    {
        List<Vector2> neighbors = new List<Vector2>();

        if (cell.x > 1 && maze[(int)cell.x - 2, (int)cell.y] == 0)
            neighbors.Add(new Vector2(cell.x - 2, cell.y));
        if (cell.x < width - 2 && maze[(int)cell.x + 2, (int)cell.y] == 0)
            neighbors.Add(new Vector2(cell.x + 2, cell.y));
        if (cell.y > 1 && maze[(int)cell.x, (int)cell.y - 2] == 0)
            neighbors.Add(new Vector2(cell.x, cell.y - 2));
        if (cell.y < height - 2 && maze[(int)cell.x, (int)cell.y + 2] == 0)
            neighbors.Add(new Vector2(cell.x, cell.y + 2));

        return neighbors;
    }

    void DrawMaze()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x, y, 0);

                if (maze[x, y] == 0)
                {
                    Instantiate(wallPrefab, pos, Quaternion.identity);
                }
                else
                {
                    Instantiate(pathPrefab, pos, Quaternion.identity);
                }
            }
        }
    }
}
