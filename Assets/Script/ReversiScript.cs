using System.Diagnostics.Tracing;
using UnityEngine;

public class ReversiScript : MonoBehaviour
{

    // Copy Reversi pieces in 8 by 8 arrangement and display them
    public GameObject ReversiSplite;

    const int FIELD_SIZE_X = 8;
    const int FIELD_SIZE_Y = 8;

    public enum spriteState
    {
        None,
        White,
        Black
    }

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
                var sprite = Instantiate(ReversiSplite, new Vector3(1.01f * x, 0, 1.01f * y), Quaternion.Euler(90, 0,0));

                _FieldState[x, y] = spriteState.None;

                // Assign each spritefs component for SpriteScript class.
                _FieldSpriteState[x, y] = sprite.GetComponent<SpriteScript>();
            }
        }
        _FieldState[3, 3] = spriteState.Black;
        _FieldState[3, 4] = spriteState.White;
        _FieldState[4, 3] = spriteState.White;
        _FieldState[4, 4] = spriteState.Black;
    }

    // Update is called once per frame
    void Update()
    {
        for (int x = 0; x < FIELD_SIZE_X; x++)
        {
            for (int y = 0; y < FIELD_SIZE_X; y++)
            {
                _FieldSpriteState[x, y].SetState(_FieldState[x, y]);
            }
        }
    }
}
