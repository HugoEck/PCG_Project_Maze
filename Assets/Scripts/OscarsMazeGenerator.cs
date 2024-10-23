using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OscarsMazeGenerator : MonoBehaviour
{
    public int width;
    public int height;

    public Sprite wallSprite;
    public Sprite floorSprite;
    public Sprite goalSprite;

    public float tileScale = 1f;

    private bool[,] maze;
    private Stack<Vector2Int> stack = new Stack<Vector2Int>();
    private List<Vector2Int> directions = new List<Vector2Int>
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    private Vector2Int startPosition;
    private Vector2Int goalPosition;

    void Start()
    {
        GenerateMaze();
        RenderMaze();
        AdjustCameraToFitMaze();

        Debug.Log("Start Position: " + startPosition.x + ", " + startPosition.y);
        Debug.Log("Goal Position: " + goalPosition.x + ", " + goalPosition.y);

        // Pass maze data and goal position to the agent
        AgentController agentController = FindObjectOfType<AgentController>();
        if (agentController != null)
        {
            agentController.InitializeMaze(maze, goalPosition);
        }
    }

    void GenerateMaze()
    {
        maze = new bool[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                maze[x, y] = false;  // Set everything to walls initially
            }
        }

        if (width % 2 == 0) width--;
        if (height % 2 == 0) height--;

        startPosition = new Vector2Int(1, 1);  // Hardcoded to (1,1) as the valid start position
        maze[startPosition.x, startPosition.y] = true;  // Set start position to walkable

        stack.Push(startPosition);

        while (stack.Count > 0)
        {
            Vector2Int currentCell = stack.Peek();
            List<Vector2Int> validNeighbors = new List<Vector2Int>();

            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighbor = currentCell + dir * 2;  // Check 2 cells away
                if (IsInBounds(neighbor) && !maze[neighbor.x, neighbor.y])
                {
                    validNeighbors.Add(neighbor);
                }
            }

            if (validNeighbors.Count > 0)
            {
                Vector2Int chosenNeighbor = validNeighbors[Random.Range(0, validNeighbors.Count)];
                Vector2Int wall = currentCell + (chosenNeighbor - currentCell) / 2;

                maze[wall.x, wall.y] = true;
                maze[chosenNeighbor.x, chosenNeighbor.y] = true;
                stack.Push(chosenNeighbor);
            }
            else
            {
                stack.Pop();
            }
        }

        goalPosition = FindValidGoalPosition();
        maze[goalPosition.x, goalPosition.y] = true;
    }

    Vector2Int FindValidGoalPosition()
    {
        for (int x = width - 2; x > 0; x--)
        {
            for (int y = height - 2; y > 0; y--)
            {
                if (maze[x, y])
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return new Vector2Int(width - 2, height - 2);  // Default fallback position
    }

    bool IsInBounds(Vector2Int pos)
    {
        return pos.x > 0 && pos.y > 0 && pos.x < width - 1 && pos.y < height - 1;
    }

    void RenderMaze()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = new Vector3(x * tileScale, y * tileScale, 0);

                if (maze[x, y])
                {
                    CreateSpriteObject(floorSprite, position, 0);
                }
                else
                {
                    CreateSpriteObject(wallSprite, position, 0);
                }
            }
        }

        Vector3 goalPos = new Vector3(goalPosition.x * tileScale, goalPosition.y * tileScale, 0);
        CreateSpriteObject(goalSprite, goalPos, 1);
    }

    void CreateSpriteObject(Sprite sprite, Vector3 position, int sortingOrder)
    {
        GameObject obj = new GameObject("TileSprite");
        obj.transform.position = position;
        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
    }

    public Vector2Int GetStartPosition()
    {
        return startPosition;
    }

    public Vector2Int GetGoalPosition()
    {
        return goalPosition;
    }

    // Adjust the camera to fit the entire maze
    void AdjustCameraToFitMaze()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // Calculate the orthographic size to fit the maze height
            float mazeHeight = height * tileScale;
            float mazeWidth = width * tileScale;

            // Set the camera's orthographic size to fit the maze's height
            mainCamera.orthographicSize = mazeHeight / 2f;

            // Calculate the aspect ratio to ensure the width fits as well
            float screenAspect = (float)Screen.width / (float)Screen.height;
            float cameraWidth = mainCamera.orthographicSize * screenAspect;

            // Adjust orthographic size to fit the width if needed
            if (cameraWidth < mazeWidth / 2f)
            {
                mainCamera.orthographicSize = (mazeWidth / 2f) / screenAspect;
            }

            // Center the camera slightly above the maze's center
            float verticalShift = tileScale * -0.36f;  // Adjust this value to control how much to move upwards
            mainCamera.transform.position = new Vector3((width / 2f) * tileScale, (height / 2f) * tileScale + verticalShift, -10f);
        }
    }
}
