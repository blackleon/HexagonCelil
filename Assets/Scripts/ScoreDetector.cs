using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreDetector : MonoBehaviour
{
    public int score;
    public int moves;

    public int numberOfSpawnedBombs = 0;

    public int numberOfDefaults = 0;
    public int numberOfStars = 0;
    public int numberOfBombs = 0;
    public int bombColorIndex = 0;

    public bool detectionEnded;
    public bool scoredOverall;
    public bool scoredInThisIteration;

    public bool gameEnded;

    GridGenerator gridGenerator;
    InputManager inputManager;
    UIController UIcontroller;

    private void Start()
    {
        gridGenerator = FindObjectOfType<GridGenerator>();
        inputManager = FindObjectOfType<InputManager>();
        UIcontroller = FindObjectOfType<UIController>();

        if (PlayerPrefs.HasKey("BestScore")) //check bestscore and show
        {
            UIcontroller.setBestScore(PlayerPrefs.GetInt("BestScore"));
        }
        else
        {
            UIcontroller.setBestScore(0);
        }

        score = 0;
        moves = 0;
    }

    public void DetectScore() //detect score method
    {
        StartCoroutine(DetectScoreCoroutine());
    }

    IEnumerator DetectScoreCoroutine() //detect score coroutine
    {
        List<GameObject> scoredObjects = new List<GameObject>();
        while (true)
        {
            scoredInThisIteration = false;
            foreach (GameObject nodeObject in gridGenerator.nodes)
            {
                if (nodeObject != null) //if all 3 nodes are the same colour set score and disable them
                {
                    Node node = nodeObject.GetComponent<Node>();
                    if (gridGenerator.hexagonMatrix[node.topX, node.topY].GetComponent<Hexagon>().colorIndex == gridGenerator.hexagonMatrix[node.middleX, node.middleY].GetComponent<Hexagon>().colorIndex
                        &&
                       gridGenerator.hexagonMatrix[node.topX, node.topY].GetComponent<Hexagon>().colorIndex == gridGenerator.hexagonMatrix[node.bottomX, node.bottomY].GetComponent<Hexagon>().colorIndex
                       &&
                       !gridGenerator.hexagonMatrix[node.topX, node.topY].GetComponent<Hexagon>().scored
                       &&
                       !gridGenerator.hexagonMatrix[node.bottomX, node.bottomY].GetComponent<Hexagon>().scored
                       &&
                       !gridGenerator.hexagonMatrix[node.middleX, node.middleY].GetComponent<Hexagon>().scored)
                    {
                        if(!scoredObjects.Contains(gridGenerator.hexagonMatrix[node.topX, node.topY]))
                        {
                            scoredObjects.Add(gridGenerator.hexagonMatrix[node.topX, node.topY]);
                        }

                        if (!scoredObjects.Contains(gridGenerator.hexagonMatrix[node.middleX, node.middleY]))
                        {
                            scoredObjects.Add(gridGenerator.hexagonMatrix[node.middleX, node.middleY]);
                        }

                        if (!scoredObjects.Contains(gridGenerator.hexagonMatrix[node.bottomX, node.bottomY]))
                        {
                            scoredObjects.Add(gridGenerator.hexagonMatrix[node.bottomX, node.bottomY]);
                        }

                        scoredInThisIteration = true;
                        scoredOverall = true;
                        inputManager.activeOverlay.SetActive(false);
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            int thisIterationDefaults = 0;
            int thisIterationStars = 0;
            int thisIterationBoms = 0;

            for (int i = scoredObjects.Count - 1; i > -1; i--)
            {
                if (scoredObjects[i].GetComponent<Hexagon>().hasStar)
                {
                    thisIterationStars++;
                }
                else if (scoredObjects[i].GetComponent<Hexagon>().isBomb)
                {
                    thisIterationBoms++;
                    bombColorIndex = scoredObjects[i].GetComponent<Hexagon>().colorIndex;
                }
                else
                {
                    thisIterationDefaults++;
                }
            }

            if (thisIterationStars > 0 && thisIterationBoms > 0) //if bombs and stars are gone remove the corresponding color
            {
                for (int i = gridGenerator.width; i > -1; i--)
                {
                    for (int j = gridGenerator.height; j > -1; j--)
                    {
                        if (gridGenerator.hexagonMatrix[i, j].GetComponent<Hexagon>().colorIndex == bombColorIndex)
                        {
                            if (gridGenerator.hexagonMatrix[i, j].GetComponent<Hexagon>().hasStar)
                            {
                                thisIterationDefaults++;
                                thisIterationStars++;
                            }
                            else
                            {
                                thisIterationDefaults++;
                            }
                            gridGenerator.hexagonMatrix[i, j].SetActive(false);
                            if (scoredObjects.Contains(gridGenerator.hexagonMatrix[i, j]))
                            {
                                scoredObjects.Remove(gridGenerator.hexagonMatrix[i, j]);
                            }
                        }
                    }
                }
            }
            else
            {
                //calculate score and bestscore
                score += (int)(thisIterationDefaults * 5 * Mathf.Pow(2, thisIterationStars));
                UIcontroller.setScore(score);

                if (PlayerPrefs.HasKey("BestScore"))
                {
                    if (PlayerPrefs.GetInt("BestScore") < score)
                    {
                        PlayerPrefs.SetInt("BestScore", score);
                    }
                }
                else
                {
                    PlayerPrefs.SetInt("BestScore", score);
                }

                UIcontroller.setBestScore(PlayerPrefs.GetInt("BestScore"));

                for (int i = scoredObjects.Count - 1; i > -1; i--)
                {
                    scoredObjects[i].transform.GetChild(2).GetComponent<ParticleSystem>().Play();
                    scoredObjects[i].GetComponent<Hexagon>().scored = true;
                    scoredObjects.RemoveAt(i);
                }
            }

            yield return new WaitForFixedUpdate();

            //if scored call fill in the gaps method
            if (scoredInThisIteration)
            {
                gridGenerator.fillingEnded = false;
                gridGenerator.fillGaps();
                yield return new WaitUntil(() => gridGenerator.fillingEnded);
            }
            else
            {
                if (gridGenerator.fillingEnded && !gridGenerator.gapsFilled)
                {
                    break;
                }
            }

            yield return new WaitForFixedUpdate();

            numberOfDefaults += thisIterationDefaults;
            numberOfStars += thisIterationStars;
            numberOfBombs += thisIterationBoms;
        }

        if(scoredOverall) //if there was score on this move add moves and reduce bomb tick
        {
            moves++;
            UIcontroller.setMoves(moves);

            foreach(GameObject hexagon in gridGenerator.hexagonMatrix)
            {
                if(hexagon.activeSelf && hexagon.GetComponent<Hexagon>().isBomb)
                {
                    hexagon.GetComponent<Hexagon>().countdown--;
                    hexagon.GetComponent<Text>().text = hexagon.GetComponent<Hexagon>().countdown.ToString();
                    hexagon.transform.GetChild(1).GetComponent<TextMesh>().text = hexagon.GetComponent<Hexagon>().countdown.ToString();

                    if (hexagon.GetComponent<Hexagon>().countdown <= 0) //gameover
                    {
                        foreach(GameObject hex in gridGenerator.hexagonMatrix)
                        {
                            hex.GetComponent<SpriteRenderer>().sprite = null;
                            hex.transform.GetChild(0).gameObject.SetActive(false);
                            hex.transform.GetChild(1).gameObject.SetActive(false);
                            hex.transform.GetChild(2).GetComponent<ParticleSystem>().Play();
                        }

                        inputManager.leftOverlay.SetActive(false);
                        inputManager.rightOverlay.SetActive(false);

                        yield return new WaitForFixedUpdate();

                        UIcontroller.setEndScore(score);
                        UIcontroller.setHexagonCount(numberOfDefaults);
                        UIcontroller.setStarCount(numberOfStars);
                        UIcontroller.setBombCount(numberOfBombs);

                        UIcontroller.showEndMenu();
                        gameEnded = true;
                    }
                }
            }
        }

        yield return new WaitForFixedUpdate();
        detectionEnded = true;
    }
}