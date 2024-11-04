using UnityEngine;

public class DFSGenerator : MonoBehaviour
{
    public bool[,] Generate(Vector2Int mazeSize)
    {
        bool[,] maze = new bool[mazeSize.x, mazeSize.y];

        // Initialize the maze with walls
        for (int x = 0; x < mazeSize.x; x++)
        {
            for (int y = 0; y < mazeSize.y; y++)
            {
                maze[x, y] = false; // False represents a wall
            }
        }

        // Starting position
        Vector2Int startPosition = new Vector2Int(1, 1);
        maze[startPosition.x, startPosition.y] = true;

        // Start DFS from the starting position
        GenerateDFS(maze, startPosition);

        return maze;
    }

    private void GenerateDFS(bool[,] maze, Vector2Int position)
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        ShuffleDirections(directions);

        foreach (var direction in directions)
        {
            Vector2Int newPos = position + direction * 2;

            if (IsInBounds(maze, newPos) && !maze[newPos.x, newPos.y])
            {
                maze[position.x + direction.x, position.y + direction.y] = true;
                maze[newPos.x, newPos.y] = true;

                GenerateDFS(maze, newPos);
            }
        }
    }

    private void ShuffleDirections(Vector2Int[] directions)
    {
        for (int i = 0; i < directions.Length; i++)
        {
            Vector2Int temp = directions[i];
            int randomIndex = Random.Range(i, directions.Length);
            directions[i] = directions[randomIndex];
            directions[randomIndex] = temp;
        }
    }

    private bool IsInBounds(bool[,] maze, Vector2Int position)
    {
        return position.x > 0 && position.x < maze.GetLength(0) - 1 &&
               position.y > 0 && position.y < maze.GetLength(1) - 1;
    }
}
