using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading;

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
            for (int y = 0; y < FIELD_SIZE_X; y++)
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

        if (k.rightArrowKey.wasPressedThisFrame && cube_gridX < CUBE_MAX_X)
            cube_gridX++;
        else if (k.leftArrowKey.wasPressedThisFrame && cube_gridX > CUBE_MIN_X)
            cube_gridX--;
        else if (k.upArrowKey.wasPressedThisFrame && cube_gridY < CUBE_MAX_Y)
            cube_gridY++;
        else if (k.downArrowKey.wasPressedThisFrame && cube_gridY > CUBE_MIN_Y)
            cube_gridY--;

        ApplyCubePosition();

        if (k.enterKey.wasPressedThisFrame)
        {
            _FieldState[cube_gridX, cube_gridY] = _PlayerTurn;
            _PlayerTurn = _PlayerTurn == spriteState.Black ? spriteState.White : spriteState.Black;
        }
        for (int x = 0; x < FIELD_SIZE_X; x++)
        {
            for (int y = 0; y < FIELD_SIZE_X; y++)
            {
                _FieldSpriteState[x, y].SetState(_FieldState[x, y]);
            }
        }
    }

    void ApplyCubePosition()
    {
        Cube.transform.localPosition = new Vector3(cube_gridX * CUBE_STEP, CUBE_FIXED_Y, cube_gridY * CUBE_STEP);
    }
}
