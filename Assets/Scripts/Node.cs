using System.Collections;
using UnityEngine;

public class Node : MonoBehaviour
{
    public int topX, topY;
    public int middleX, middleY;
    public int bottomX, bottomY;

    public bool twoOnRight;

    public bool clockwise;

    GridGenerator gridGenerator;
    InputManager inputManager;
    ScoreDetector scoreDetector;

    private void Start()
    {
        gridGenerator = FindObjectOfType<GridGenerator>();
        inputManager = FindObjectOfType<InputManager>();
        scoreDetector = FindObjectOfType<ScoreDetector>();
    }

    public void rotateHexagons(bool _clockwise) //rotate hexagons method
    {
        inputManager.clickEnabled = false;
        clockwise = _clockwise;
        StartCoroutine(rotateSelfAndCheckScore());
    }

    public void rotateTopToBottom() //rotate hexagons top to bottom
    {
        GameObject temp;

        temp = gridGenerator.hexagonMatrix[bottomX, bottomY];
        gridGenerator.hexagonMatrix[bottomX, bottomY] = gridGenerator.hexagonMatrix[topX, topY];
        gridGenerator.hexagonMatrix[topX, topY] = gridGenerator.hexagonMatrix[middleX, middleY];
        gridGenerator.hexagonMatrix[middleX, middleY] = temp;

        Hexagon tempHexagon = GameObject.Find("Temp").GetComponent<Hexagon>();

        tempHexagon.swapHexagon(gridGenerator.hexagonMatrix[topX, topY].GetComponent<Hexagon>(), clockwise);
        gridGenerator.hexagonMatrix[topX, topY].GetComponent<Hexagon>().swapHexagon(gridGenerator.hexagonMatrix[bottomX, bottomY].GetComponent<Hexagon>(), clockwise);
        gridGenerator.hexagonMatrix[bottomX, bottomY].GetComponent<Hexagon>().swapHexagon(gridGenerator.hexagonMatrix[middleX, middleY].GetComponent<Hexagon>(), clockwise);
        gridGenerator.hexagonMatrix[middleX, middleY].GetComponent<Hexagon>().swapHexagon(tempHexagon, clockwise);
    }

    public void rotateBottomToTop() //rotate hexagons bottom to top
    {
        GameObject temp;

        temp = gridGenerator.hexagonMatrix[topX, topY];
        gridGenerator.hexagonMatrix[topX, topY] = gridGenerator.hexagonMatrix[bottomX, bottomY];
        gridGenerator.hexagonMatrix[bottomX, bottomY] = gridGenerator.hexagonMatrix[middleX, middleY];
        gridGenerator.hexagonMatrix[middleX, middleY] = temp;

        Hexagon tempHexagon = GameObject.Find("Temp").GetComponent<Hexagon>();

        tempHexagon.swapHexagon(gridGenerator.hexagonMatrix[bottomX, bottomY].GetComponent<Hexagon>(), clockwise);
        gridGenerator.hexagonMatrix[bottomX, bottomY].GetComponent<Hexagon>().swapHexagon(gridGenerator.hexagonMatrix[topX, topY].GetComponent<Hexagon>(), clockwise);
        gridGenerator.hexagonMatrix[topX, topY].GetComponent<Hexagon>().swapHexagon(gridGenerator.hexagonMatrix[middleX, middleY].GetComponent<Hexagon>(), clockwise);
        gridGenerator.hexagonMatrix[middleX, middleY].GetComponent<Hexagon>().swapHexagon(tempHexagon, clockwise);
    }

    IEnumerator rotateSelfAndCheckScore() //rotate self and check score coroutine
    {
        for (int x = 0; x < 3; x++) //repeat three times
        {
            if(gridGenerator.hexagonMatrix[topX, topY] != null
                &&
                gridGenerator.hexagonMatrix[middleX, middleY] != null
                &&
                gridGenerator.hexagonMatrix[bottomX, bottomY] != null
                )
            {
                //make hexagons to rotate child objects
                gridGenerator.hexagonMatrix[topX, topY].transform.parent = transform;
                gridGenerator.hexagonMatrix[middleX, middleY].transform.parent = transform;
                gridGenerator.hexagonMatrix[bottomX, bottomY].transform.parent = transform;

                for (int i = 0; i < 10; i++) //rotate hexagons
                {
                    if (clockwise)
                    {
                        transform.Rotate(new Vector3(0f, 0f, -360f / 30));
                    }
                    else
                    {
                        transform.Rotate(new Vector3(0f, 0f, 360f / 30));
                    }
                    yield return new WaitForFixedUpdate();
                }

                //change hexagons to rotate parents
                gridGenerator.hexagonMatrix[topX, topY].transform.parent = GameObject.Find("InstantiationRoot").transform;
                gridGenerator.hexagonMatrix[middleX, middleY].transform.parent = GameObject.Find("InstantiationRoot").transform;
                gridGenerator.hexagonMatrix[bottomX, bottomY].transform.parent = GameObject.Find("InstantiationRoot").transform;

                //fix hexagons rotation
                gridGenerator.hexagonMatrix[topX, topY].transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
                gridGenerator.hexagonMatrix[middleX, middleY].transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
                gridGenerator.hexagonMatrix[bottomX, bottomY].transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            }
            else
            {
                continue;
            }

            if (twoOnRight) //rotate checking if there are two nodes on right or left
            {
                if (!clockwise)
                {
                    rotateBottomToTop();
                }
                else
                {
                    rotateTopToBottom();
                }
            }
            else
            {
                if (!clockwise)
                {
                    rotateTopToBottom();
                }
                else
                {
                    rotateBottomToTop();
                }
            }

            yield return new WaitForSeconds(0.1f);

            //call score detector
            scoreDetector.scoredOverall = false;
            scoreDetector.scoredInThisIteration = false;
            scoreDetector.detectionEnded = false;
            scoreDetector.DetectScore();

            yield return new WaitUntil(() => scoreDetector.detectionEnded);

            //exit if score is detected keep rotating if not
            if(scoreDetector.scoredOverall) 
            {
                break;
            }else if (scoreDetector.detectionEnded)
            {
                continue;
            }
        }

        yield return new WaitForSeconds(0.1f);
        
        if(!scoreDetector.gameEnded)
        {
            inputManager.activeOverlay.SetActive(true);
            inputManager.clickEnabled = true;
        }
    }
}