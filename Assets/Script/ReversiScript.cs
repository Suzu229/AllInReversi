using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ReversiScript : MonoBehaviour
{

    // Copy Reversi pieces in 8 by 8 arrangement and display them
    public GameObject ReversiSprite;
    public GameObject Cube;
    public GameObject HighlightPrefab;
    public TextMeshProUGUI TurnText;

    private readonly List<GameObject> _highlights = new(); // pool

    const int FIELD_SIZE_X = 8;
    const int FIELD_SIZE_Y = 8;

    const int CUBE_MIN_X = 0, CUBE_MAX_X = 7;
    const int CUBE_MIN_Y = 0, CUBE_MAX_Y = 7;
    const float CUBE_STEP = 1.01f;
    // Fix the cursor's height so it stays aligned with the board
    const float CUBE_FIXED_Y = -0.178f;

    int cube_gridX = 0, cube_gridY = 0;

    private static readonly (int dx, int dy)[] DIRS = new (int, int)[]
    {
        (-1,0), (1,0),(0,-1),(0,1),(1,1),(-1,-1),(-1,1),(1,-1)
    };

    public enum spriteState
    {
        None,
        White,
        Black
    }

    private spriteState _PlayerTurn = spriteState.Black;
    // board state
    private spriteState[,] _FieldState = new spriteState[FIELD_SIZE_X, FIELD_SIZE_Y];
    // Make the spriteScript class objects available.
    // the piece object displayed on the screen
    private SpriteScript[,] _FieldSpriteState = new SpriteScript[FIELD_SIZE_X, FIELD_SIZE_Y];

    private bool _gameover = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int x = 0; x < FIELD_SIZE_X; x++)
        {
            for (int y = 0; y < FIELD_SIZE_Y; y++)
            {
                var sprite = Instantiate(ReversiSprite, new Vector3(CUBE_STEP * x, 0f, CUBE_STEP * y), Quaternion.Euler(90, 0, 0));
                // Assign each sprite’s component for SpriteScript class.
                _FieldSpriteState[x, y] = sprite.GetComponent<SpriteScript>();
                _FieldState[x, y] = spriteState.None;
            }
        }
        // starting position
        _FieldState[3, 3] = spriteState.Black;
        _FieldState[3, 4] = spriteState.White;
        _FieldState[4, 3] = spriteState.White;
        _FieldState[4, 4] = spriteState.Black;

        cube_gridX = 0;
        cube_gridY = 0;
        ApplyCubePosition();
        RefreshSprites();
        RefreshHighlights();
        UpdateTurnText();
    }

    // Update is called once per frame
    void Update()
    {
        var k = Keyboard.current;

        if (_gameover)
        {
            if (k.rKey.wasPressedThisFrame)
            {
                ResetBoard();
                _gameover = false;
                RefreshSprites();
                RefreshHighlights();
                UpdateTurnText();
            }
            return;
        }

        bool moved = false;
        bool placed = false;

        #region setting coordinates
        if (k.rightArrowKey.wasPressedThisFrame && cube_gridX < CUBE_MAX_X)
        {
            cube_gridX++;
            moved = true;
        }
        else if (k.leftArrowKey.wasPressedThisFrame && cube_gridX > CUBE_MIN_X)
        {
            cube_gridX--;
            moved = true;
        }
        else if (k.upArrowKey.wasPressedThisFrame && cube_gridY < CUBE_MAX_Y)
        {
            cube_gridY++;
            moved = true;
        }
        else if (k.downArrowKey.wasPressedThisFrame && cube_gridY > CUBE_MIN_Y)
        {
            cube_gridY--;
            moved = true;
        }
        #endregion

        if (moved)
        {
            ApplyCubePosition();
            RefreshHighlights();
        }

        // place a piece
        if (k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame)
            placed = PlaceAt(cube_gridX, cube_gridY);

        ApplyCubePosition();
        RefreshSprites();

        if (placed)
        {
            RefreshHighlights();
            UpdateTurnText();
            CheckEndConditions();
        }
    }

    #region Core
    /// <summary>
    /// Checks weather a piece of the given color can be legally placed at (x, y)
    /// If the placement is valid, this method collects the coordinates of opponent pieces
    /// that would be flipped and stores them in the 'flips' list
    /// </summary>
    /// <param name="x">Board coordinate X</param>
    /// <param name="y">Board coordinate Y</param>
    /// <param name="color">Piece color to place</param>
    /// <param name="flips">A list that will be populated with positions to flip if the move is valid</param>
    /// <returns>True if the move is legal; otherwise false</returns>
    private bool CanPlaceAt(int x, int y, spriteState color, List<(int, int)> flips)
    {
        if (_FieldState[x, y] != spriteState.None)
            return false;

        flips.Clear();
        foreach (var (dx, dy) in DIRS)
            GatherFlipsFrom(x, y, dx, dy, color, flips);

        return flips.Count > 0;
    }

    // the process of collecting the pieces that can be flipped in that direction(by one line)
    private void GatherFlipsFrom(int x, int y, int dx, int dy, spriteState color, List<(int, int)> flips)
    {
        // current position
        int cx = x, cy = y;
        var opp = (color == spriteState.Black) ? spriteState.White : spriteState.Black;

        List<(int, int)> tmp = new();

        while (true)
        {
            // the coordinates of the next square to examine
            int nx = cx + dx, ny = cy + dy;
            if (nx < 0 || nx >= FIELD_SIZE_X || ny < 0 || ny >= FIELD_SIZE_Y)
                return;

            var cell = _FieldState[nx, ny];

            if (cell == opp)
            {
                tmp.Add((nx, ny));
                cx = nx;
                cy = ny;
                continue;
            }

            if (cell == spriteState.None)
                return;

            // reached your own color: confirmed if at least one opponent piece is sandwiched
            if (cell == color)
            {
                if (tmp.Count > 0)
                    flips.AddRange(tmp);
            }
            return;
        }
    }

    private bool PlaceAt(int x, int y)
    {
        var flips = new List<(int, int)>();
        if (!CanPlaceAt(x, y, _PlayerTurn, flips))
            return false;

        _FieldState[x, y] = _PlayerTurn;

        // flip and place
        foreach (var p in flips)
            _FieldState[p.Item1, p.Item2] = _PlayerTurn;

        // switch turns
        _PlayerTurn = (_PlayerTurn == spriteState.Black) ? spriteState.White : spriteState.Black;

        // Pass check: if the opp has no valid moves, return the my turn;
        // if neither can move, end the game.
        bool oppHas = HasLegalMove(_PlayerTurn);
        bool meHas = HasLegalMove((_PlayerTurn == spriteState.Black) ? spriteState.White : spriteState.Black);

        if (!oppHas && meHas)
            // return the turn if the opp passes
            _PlayerTurn = (_PlayerTurn == spriteState.Black) ? spriteState.White : spriteState.Black;

        return true;
    }

    private bool HasLegalMove(spriteState color)
    {
        var flips = new List<(int, int)>();
        for (int x = 0; x < FIELD_SIZE_X; x++)
            for (int y = 0; y < FIELD_SIZE_Y; y++)
                if (CanPlaceAt(x, y, color, flips))
                    return true;
        return false;
    }

    private void CheckEndConditions()
    {
        bool blackHas = HasLegalMove(spriteState.Black);
        bool whiteHas = HasLegalMove(spriteState.White);

        var (w, b) = CountPieces();
        int total = FIELD_SIZE_X * FIELD_SIZE_Y;
        if ((!blackHas && !whiteHas) || w + b == total)
            EndGame(w, b);
    }

    private void EndGame(int white, int black)
    {
        _gameover = true;

        string msg =
            (black > white) ? $"Black wins! B:{black} W:{white}" :
            (white > black) ? $"White wins! W:{white} B:{black}" :
            $"Draw! B:{black} W:{white}";

        if (TurnText != null)
            TurnText.text = msg + "  (Press R to Restart)";

        // delete the highlight
        foreach (var go in _highlights) go.SetActive(false);
    }
    #endregion

    #region View / Utility
    private void ApplyCubePosition()
    {
        Cube.transform.localPosition = new Vector3(cube_gridX * CUBE_STEP, CUBE_FIXED_Y, cube_gridY * CUBE_STEP);
    }

    /// <summary>
    /// Update the appearance of the pieces on the screen according to the board state.
    /// </summary>
    private void RefreshSprites()
    {
        for (int x = 0; x < FIELD_SIZE_X; x++)
            for (int y = 0; y < FIELD_SIZE_Y; y++)
                _FieldSpriteState[x, y].SetState(_FieldState[x, y]);
    }

    private (int white, int black) CountPieces()
    {
        int w = 0;
        int b = 0;
        for (int x = 0; x < FIELD_SIZE_X; x++)
            for (int y = 0; y < FIELD_SIZE_Y; y++)
            {
                var s = _FieldState[x, y];
                if (s == spriteState.White)
                    w++;
                else if (s == spriteState.Black)
                    b++;
            }
        return (w, b);
    }

    private void ResetBoard()
    {
        for (int x = 0; x < FIELD_SIZE_X; x++)
            for (int y = 0; y < FIELD_SIZE_Y; y++)
                _FieldState[x, y] = spriteState.None;


        _FieldState[3, 3] = spriteState.Black;
        _FieldState[3, 4] = spriteState.White;
        _FieldState[4, 3] = spriteState.White;
        _FieldState[4, 4] = spriteState.Black;

        _PlayerTurn = spriteState.Black;

        cube_gridX = 0;
        cube_gridY = 0;

        ApplyCubePosition();
        RefreshSprites();
    }

    private void RefreshHighlights()
    {
        // Deactivate existing (Reuse)
        foreach (var go in _highlights) go.SetActive(false);

        var flips = new List<(int, int)>();
        for (int x = 0; x < FIELD_SIZE_X; x++)
            for (int y = 0; y < FIELD_SIZE_Y; y++)
            {
                // Check if placement is possible (reuse existing CanplaceAt)
                if (_FieldState[x, y] == spriteState.None && CanPlaceAt(x, y, _PlayerTurn, flips))
                {
                    var pos = new Vector3(x * CUBE_STEP, CUBE_FIXED_Y + 0.1f, y * CUBE_STEP);

                    // Generate if no objects remain in the pool
                    GameObject go = null;
                    for (int i = 0; i < _highlights.Count; i++)
                        if (!_highlights[i].activeSelf)
                        {
                            go = _highlights[i];
                            break;
                        }

                    if (go == null)
                    {
                        go = Instantiate(HighlightPrefab);
                        _highlights.Add(go);
                    }

                    // Lay the quad flat on the XZ plane
                    go.transform.SetPositionAndRotation(pos, Quaternion.Euler(90f, 0f, 0f));
                    // A quad uses X and Y for size; Z is ignored
                    go.transform.localScale = new Vector3(CUBE_STEP, CUBE_STEP, 1f);

                    go.SetActive(true);
                }
            }
    }

    private void UpdateTurnText()
    {
        if (TurnText == null) 
            return;
        TurnText.text = (_PlayerTurn == spriteState.Black) ? "Black Turn ●" : "White Turn ○";
    }


    #endregion
}