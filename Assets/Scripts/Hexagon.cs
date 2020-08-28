using System.Collections;
using UnityEngine;

public class Hexagon : MonoBehaviour
{
    public int xIndex;
    public int yIndex;

    public Vector3 position;
    public int colorIndex;
    public Sprite symbol;

    public bool hasStar;

    public bool isBomb;
    public int countdown;

    public bool clockwise;

    public float moveDelay;
    public bool moving;

    public bool scored;

    public void swapHexagon(Hexagon target, bool _clockwise) //swap hexagon information
    {
        xIndex = target.xIndex;
        yIndex = target.yIndex;

        clockwise = _clockwise;
    }

    public void moveToPosition(Vector3 _position, float delay) //go to position with delay
    {
        position = _position;
        moveDelay = delay;
        StartCoroutine(moveCoroutine());
    }

    IEnumerator moveCoroutine() //move coroutine
    {
        moving = true;

        yield return new WaitForSeconds(moveDelay);
        while(transform.position != position)
        {
            if(Vector3.Distance(transform.position, position) > 0.1f)
            {
                transform.position = Vector3.Lerp(transform.position, position, 0.25f);
            }
            else
            {
                transform.position = position;
            }

            yield return new WaitForFixedUpdate();
        }

        yield return new WaitForEndOfFrame();
        moving = false;
    }

    public void setSymbol(bool state) //set symbol status
    {
        transform.GetChild(0).gameObject.SetActive(state);
    }
}