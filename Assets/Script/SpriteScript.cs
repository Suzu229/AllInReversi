using Unity.Mathematics;
using UnityEngine;

public class SpriteScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetState(ReversiScript.spriteState spriteState)
    {
        gameObject.SetActive(spriteState != ReversiScript.spriteState.None);

        if (spriteState == ReversiScript.spriteState.White)
            gameObject.transform.rotation = Quaternion.Euler(90, 0, 0);
        else if (spriteState == ReversiScript.spriteState.Black)
            gameObject.transform.rotation = Quaternion.Euler(270, 0, 0);

    }
}
