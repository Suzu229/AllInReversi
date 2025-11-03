using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Threading;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

public class ReversiScript : MonoBehaviour
{

    // Copy Reversi pieces in 8 by 8 arrangement and display them
    public GameObject ReversiSplite;
    public GameObject Cube;

    const int FIELD_SIZE_X = 8;
    const int FIELD_SIZE_Y = 8;

    const int CUBE_MIN_X = 0, CUBE_MAX_X = 7;
    const int CUBE_MIN_Y = 0, CUBE_MAX_Y = 7;
    const float CUBE_STEP = 1.01f;
    // Fix the cursor's height so it stays aligned with the board
    const float CUBE_FIXED_Y = -0.178f;

    int cube_gridX = 0, cube_gridY = 0;

    private List<(int, int)> _InfoList = new List<(int, int)>();

    public enum spriteState
    {
        None,
        White,
        Black
    }

    private spriteState _PlayerTurn = spriteState.Black;

    private spriteState[,] _FieldState = new spriteState[FIELD_SIZE_X, FIELD_SIZE_Y];
    // Make the spriteScript class objects available.
    private SpriteScript[,] _FieldSpriteState = new SpriteScript[FIELD_SIZE_X, FIELD_SIZE_Y];

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int x = 0; x < FIELD_SIZE_X; x++)
        {
            for (int y = 0; y < FIELD_SIZE_Y; y++)
            {
                var sprite = Instantiate(ReversiSplite, new Vector3(1.01f * x, 0, 1.01f * y), Quaternion.Euler(90, 0, 0));

                _FieldState[x, y] = spriteState.None;

                // Assign each spritefs component for SpriteScript class.
                _FieldSpriteState[x, y] = sprite.GetComponent<SpriteScript>();
            }
        }
        _FieldState[3, 3] = spriteState.Black;
        _FieldState[3, 4] = spriteState.White;
        _FieldState[4, 3] = spriteState.White;
        _FieldState[4, 4] = spriteState.Black;

        ApplyCubePosition();
    }

    // Update is called once per frame
    void Update()
    {
        var k = Keyboard.current;

        #region setting coordinates
        if (k.rightArrowKey.wasPressedThisFrame && cube_gridX < CUBE_MAX_X)
            cube_gridX++;
        else if (k.leftArrowKey.wasPressedThisFrame && cube_gridX > CUBE_MIN_X)
            cube_gridX--;
        else if (k.upArrowKey.wasPressedThisFrame && cube_gridY < CUBE_MAX_Y)
            cube_gridY++;
        else if (k.downArrowKey.wasPressedThisFrame && cube_gridY > CUBE_MIN_Y)
            cube_gridY--;
        #endregion

        ApplyCubePosition();

        var turncheck = false;
        if (k.enterKey.wasPressedThisFrame)
        {
            for (int i = 0; i <= 7; i++)
            {
                if (TurnCheck(i))
                {
                    turncheck = true;
                }
            }

            if (turncheck && _FieldState[cube_gridX, cube_gridY] == spriteState.None)
            {
                foreach (var info in _InfoList)
                {
                    var cube_gridX = info.Item1;
                    var cube_gridY = info.Item2;
                    _FieldState[cube_gridX, cube_gridY] = _PlayerTurn;
                }

                _FieldState[cube_gridX, cube_gridY] = _PlayerTurn;
                _PlayerTurn = _PlayerTurn == spriteState.Black ? spriteState.White : spriteState.Black;
                _InfoList.Clear();
            }

        }
        UpdateBoard();
    }

    private void ApplyCubePosition()
    {
        Cube.transform.localPosition = new Vector3(cube_gridX * CUBE_STEP, CUBE_FIXED_Y, cube_gridY * CUBE_STEP);
    }

    private void UpdateBoard()
    {
        int total = FIELD_SIZE_X * FIELD_SIZE_Y;
        var whiteCount = default(int);
        var blackCount = default(int);

        for (int x = 0; x < FIELD_SIZE_X; x++)
        {
            for (int y = 0; y < FIELD_SIZE_Y; y++)
            {
                _FieldSpriteState[x, y].SetState(_FieldState[x, y]);

                if (_FieldState[x, y] == spriteState.White)
                    whiteCount++;
                else if (_FieldState[x, y] == spriteState.Black)
                    blackCount++;
            }
        }
        if(whiteCount + blackCount == total)
            ResetBoard();

    }

    private void ResetBoard()
    {
        for (int x = 0; x < FIELD_SIZE_X; x++)
        {
            for (int y = 0; y < FIELD_SIZE_Y; y++)
            {
                _FieldState[x, y] = spriteState.None;
            }
        }
        _FieldState[3, 3] = spriteState.Black;
        _FieldState[3, 4] = spriteState.White;
        _FieldState[4, 3] = spriteState.White;
        _FieldState[4, 4] = spriteState.Black;

        _InfoList.Clear();
        _PlayerTurn = spriteState.Black;

        cube_gridX = 0;
        cube_gridY = 0;

        ApplyCubePosition();
    }

    private bool TurnCheck(int direction)
    {
        var turnCheck = false;

        var x = cube_gridX;
        var y = cube_gridY;

        var OpponentPlayerTurn = _PlayerTurn == spriteState.Black ? spriteState.White : spriteState.Black;
        var infoList = new List<(int, int)>();

        // Direction vectors
        (int dx, int dy) = direction switch
        {
            0 => (-1, 0), // Left
            1 => (1, 0), // Right
            2 => (0, -1), // Down
            3 => (0, 1), // Up
            4 => (1, 1), // Upper Right
            5 => (-1, -1), // Lower Left
            6 => (-1, 1), // Upper Right
            7 => (1, -1), // Lower Right
            _ => (0, 0),
        };

        // Check by moving along each direction
        while (true)
        {
            // Check if next position is outside the board before moving
            int nx = x + dx;
            int ny = y + dy;
            if (nx < 0 || nx >= FIELD_SIZE_X || ny < 0 || ny >= FIELD_SIZE_Y)
                break;

            x = nx; y = ny;
            var cell = _FieldState[x, y];

            // Opponent piece -> store as flip candidate
            if (cell == OpponentPlayerTurn)
            {
                infoList.Add((x, y));
                continue;
            }

            // If no flipped pieces yet and reached own piece or empty -> cannot place
            if (infoList.Count == 0 && (cell == _PlayerTurn || cell == spriteState.None))
                break;

            // Reached own piece -> success if opponent pieces were sandwiched
            if (cell == _PlayerTurn)
            {
                turnCheck = infoList.Count > 0;
                if (turnCheck)
                    _InfoList.AddRange(infoList); //Add to flip list
                break;
            }

            // Reached empty cell -> cannot place
            if (cell == spriteState.None)
                break;
        }
        return turnCheck;
    }
}