using System.Collections;
using UnityEngine;

public class SpriteScript : MonoBehaviour
{

    public ReversiScript.spriteState CurrentState {  get; private set; }

    public void SetState(ReversiScript.spriteState spriteState)
    {
        CurrentState = spriteState;
        gameObject.SetActive(spriteState != ReversiScript.spriteState.None);

        if (spriteState == ReversiScript.spriteState.White)
            gameObject.transform.rotation = Quaternion.Euler(90, 0, 0); // White on top
        else if (spriteState == ReversiScript.spriteState.Black)
            gameObject.transform.rotation = Quaternion.Euler(270, 0, 0); // Black on top

    }

    public void PlayFlip()
    {
        StartCoroutine(FlipRoutine());
    }

    private IEnumerator FlipRoutine()
    {
        float durtion = 0.3f;
        float t = 0f;

        Vector3 start = transform.localEulerAngles;
        Vector3 end = start + new Vector3(180f, 0f, 0f);

        while (t < durtion)
        {
            float r = t / durtion;
            transform.localEulerAngles = Vector3.Lerp(start, end, r);

            t += Time.deltaTime;
            yield return null;
        }
        transform.localEulerAngles = end;

    }
}
