using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridGenerator : MonoBehaviour
{
    [Header("Grid Setup")]
    public float horizontalScale;
    public int width, height;

    [Header("Procedural Generator")]
    public float side;
    public float alpha;
    public float beta;
    public Vector2 leftBottom;

    [Header("Hexagon Generator")]
    public GameObject nodePrefab;
    public GameObject hexagonPrefab;
    public GameObject[,] hexagonMatrix;
    public List<GameObject> nodes;

    public List<Color> colors;
    public List<Sprite> symbols;

    public Sprite starSprite;
    public Sprite bombSprite;

    [Header("Fill Gaps")]
    public float fallHeightOffset;
    public bool fillingGaps;
    public bool fillingEnded;
    public bool gapsFilled;

    [Header("Script Access")]
    public InputManager inputManager;
    public ScoreDetector scoreDetector;
    public UIController UIcontroller;

    private void Start() //initialisation
    {
        gapsFilled = false;
        fillingEnded = true;
        inputManager = FindObjectOfType<InputManager>();
        scoreDetector = FindObjectOfType<ScoreDetector>();
        UIcontroller = FindObjectOfType<UIController>();

        StartCoroutine(generateGridCoroutine()); //start generation
    }

    public void fillGaps() //fill gaps method
    {
        StartCoroutine(fillGapsCoroutine());
    }

    IEnumerator fillGapsCoroutine() //fill gaps coroutine
    {
        fillingGaps = true;
        gapsFilled = false;

        for (int j = 0; j < height; j++) //drag the gaps up
        {
            for (int i = 0; i < width; i++)
            {
                if (hexagonMatrix[i, j].GetComponent<Hexagon>().scored)
                {
                    for (int k = j + 1; k < height; k++)
                    {
                        if (!hexagonMatrix[i, k].GetComponent<Hexagon>().scored)
                        {
                            GameObject temp = hexagonMatrix[i, j];

                            temp.GetComponent<Hexagon>().transform.position = hexagonMatrix[i, k].transform.position;
                            temp.GetComponent<Hexagon>().xIndex = i;
                            temp.GetComponent<Hexagon>().yIndex = j;

                            float posX, posY;
                            posY = (j + 1) * 2 * beta - (i % 2) * beta;
                            posX = side + side * i + i * alpha;

                            hexagonMatrix[i, k].GetComponent<Hexagon>().moveToPosition(new Vector2(posX, posY) - leftBottom + (Vector2)transform.position, 0f);
                            hexagonMatrix[i, k].GetComponent<Hexagon>().xIndex = i;
                            hexagonMatrix[i, k].GetComponent<Hexagon>().yIndex = j;

                            hexagonMatrix[i, j] = hexagonMatrix[i, k];
                            hexagonMatrix[i, k] = temp;

                            gapsFilled = true;

                            break;
                        }
                    }
                }
            }
        }

        for (int i = 0; i < width; i++) //fill the gaps
        {
            for (int j = 0; j < height; j++)
            {
                Transform instantiationRoot = GameObject.Find("InstantiationRoot").transform;
                if (hexagonMatrix[i, j].GetComponent<Hexagon>().scored)
                {
                   hexagonMatrix[i, j].GetComponent<SpriteRenderer>().sprite = null;
                    hexagonMatrix[i, j].transform.GetChild(0).gameObject.SetActive(false);
                    hexagonMatrix[i, j].transform.GetChild(1).gameObject.SetActive(false);
                    Destroy(hexagonMatrix[i, j], 3f);

                    float posX, posY;
                    posY = (j + 1) * 2 * beta - (i % 2) * beta;
                    posX = side + side * i + i * alpha;

                    GameObject hexagon = Instantiate(hexagonPrefab, new Vector2(posX, posY) - leftBottom + (Vector2)transform.position + Vector2.up * fallHeightOffset, transform.rotation);
                    hexagon.GetComponent<Hexagon>().moveToPosition(new Vector2(posX, posY) - leftBottom + (Vector2)transform.position, 0f);

                    hexagon.name = "Hexagon (" + i.ToString() + ", " + j.ToString() + ")";
                    hexagon.transform.parent = instantiationRoot;
                    hexagon.transform.localScale *= side;
                    int colorIndex;

                    colorIndex = Random.Range(0, colors.Count);

                    hexagon.GetComponent<SpriteRenderer>().color = colors[colorIndex];
                    hexagon.GetComponent<Hexagon>().colorIndex = colorIndex;
                    hexagon.GetComponent<Hexagon>().symbol = symbols[colorIndex];
                    hexagon.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = symbols[colorIndex];
                    hexagon.transform.GetChild(0).gameObject.SetActive(UIcontroller.symbolState);

                    ParticleSystem.MainModule settings = hexagon.transform.GetChild(2).GetComponent<ParticleSystem>().main;
                    settings.startColor = new ParticleSystem.MinMaxGradient(colors[colorIndex]);

                    if (scoreDetector.score / 1000 > scoreDetector.numberOfSpawnedBombs)
                    {
                        hexagon.GetComponent<Hexagon>().isBomb = true;
                        scoreDetector.numberOfSpawnedBombs++;
                        hexagon.GetComponent<SpriteRenderer>().sprite = bombSprite;
                        hexagon.AddComponent<Text>();
                        hexagon.GetComponent<Text>().text = hexagon.GetComponent<Hexagon>().countdown.ToString();
                        hexagon.transform.GetChild(1).gameObject.SetActive(true);
                        hexagon.transform.GetChild(1).GetComponent<TextMesh>().text = hexagon.GetComponent<Hexagon>().countdown.ToString();
                    }
                    else
                    {
                        int hasStar = Random.Range(0, 25);
                        if (hasStar == 0)
                        {
                            hexagon.GetComponent<Hexagon>().hasStar = true;
                            hexagon.GetComponent<SpriteRenderer>().sprite = starSprite;
                        }
                    }

                    hexagon.GetComponent<Hexagon>().xIndex = i;
                    hexagon.GetComponent<Hexagon>().yIndex = j;

                    hexagonMatrix[i, j] = hexagon;
                }
            }
        }

        yield return new WaitForFixedUpdate();
        gapsFilled = false;
        fillingEnded = true;
        yield return new WaitForFixedUpdate();
    }

    IEnumerator generateGridCoroutine() //generate grid coroutine
    {
        float posX, posY;
        inputManager.clickEnabled = false;

        hexagonMatrix = new GameObject[width, height];
        nodes = new List<GameObject>();

        side = horizontalScale / (0.5f + width * 1.5f); //calculate side, alpha, beta lenghts & left bottom
        alpha = side * 0.5f;
        beta = side * 0.5f * Mathf.Sqrt(3f);
        leftBottom = new Vector3(horizontalScale / 2f, beta + height * beta);

        Transform instantiationRoot = GameObject.Find("InstantiationRoot").transform;
        for (int i = 0; i < width; i++) //spawn hexagons
        {
            for (int j = 0; j < height; j++)
            {
                posY = (j + 1) * 2 * beta - (i % 2) * beta;
                posX = side + side * i + i * alpha;

                GameObject hexagon = Instantiate(hexagonPrefab, new Vector2(posX, posY) - leftBottom + (Vector2)transform.position + Vector2.up * fallHeightOffset, transform.rotation);
                hexagon.GetComponent<Hexagon>().moveToPosition(new Vector2(posX, posY) - leftBottom + (Vector2)transform.position, ((i + j) * 0.1f));

                hexagon.name = "Hexagon (" + i.ToString() + ", " + j.ToString() + ")";
                hexagon.transform.parent = instantiationRoot;
                hexagon.transform.localScale *= side;
                int colorIndex;

                if ((i + j) % 2 == 0) //prevent score at start
                {
                    colorIndex = Random.Range(0, colors.Count / 2);
                }
                else
                {
                    colorIndex = Random.Range(colors.Count / 2, colors.Count);
                }

                hexagon.GetComponent<SpriteRenderer>().color = colors[colorIndex];
                hexagon.GetComponent<Hexagon>().colorIndex = colorIndex;
                hexagon.GetComponent<Hexagon>().symbol = symbols[colorIndex];
                hexagon.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = symbols[colorIndex];
                hexagon.transform.GetChild(0).gameObject.SetActive(UIcontroller.symbolState);

                ParticleSystem.MainModule settings = hexagon.transform.GetChild(2).GetComponent<ParticleSystem>().main;
                settings.startColor = new ParticleSystem.MinMaxGradient(colors[colorIndex]);

                int hasStar = Random.Range(0, 25); //does hexagon have star
                if (hasStar == 0)
                {
                    hexagon.GetComponent<Hexagon>().hasStar = true;
                    hexagon.GetComponent<SpriteRenderer>().sprite = starSprite;
                }

                hexagon.GetComponent<Hexagon>().xIndex = i;
                hexagon.GetComponent<Hexagon>().yIndex = j;

                hexagonMatrix[i, j] = hexagon;
            }
        }

        int count = 0;
        for (int i = 0; i < width; i++) //spawn nodes which hexagons will rotate around
        {
            for (int j = 0; j < height; j++)
            {
                if (i < width - 1 && !(j == 0 && i % 2 == 1) && !(j == height - 1 && i % 2 == 0))
                {
                    GameObject node = Instantiate(nodePrefab, hexagonMatrix[i, j].transform.position + new Vector3(side, -fallHeightOffset, 0f), transform.rotation);
                    node.transform.parent = instantiationRoot;
                    node.name = "Node (" + count.ToString() + ")";
                    node.transform.localScale *= side / 2f;


                    node.GetComponent<Node>().twoOnRight = true;

                    if (i % 2 == 0) //define connected hexagons
                    {
                        node.GetComponent<Node>().topX = i + 1;
                        node.GetComponent<Node>().topY = j + 1;

                        node.GetComponent<Node>().middleX = i;
                        node.GetComponent<Node>().middleY = j;

                        node.GetComponent<Node>().bottomX = i + 1;
                        node.GetComponent<Node>().bottomY = j;
                    }
                    else
                    {
                        node.GetComponent<Node>().topX = i + 1;
                        node.GetComponent<Node>().topY = j;

                        node.GetComponent<Node>().middleX = i;
                        node.GetComponent<Node>().middleY = j;

                        node.GetComponent<Node>().bottomX = i + 1;
                        node.GetComponent<Node>().bottomY = j - 1;
                    }

                    nodes.Add(node);
                    count++;
                }

                if (i < width - 1 && j < height - 1)
                {
                    GameObject node = Instantiate(nodePrefab, hexagonMatrix[i, j].transform.position + new Vector3(alpha, beta - fallHeightOffset, 0f), transform.rotation);
                    node.transform.parent = instantiationRoot;
                    node.name = "Node (" + count.ToString() + ")";
                    node.transform.localScale *= side / 2f;

                    node.GetComponent<Node>().twoOnRight = false;

                    if (i % 2 == 0) //define connected hexagons
                    {
                        node.GetComponent<Node>().topX = i;
                        node.GetComponent<Node>().topY = j + 1;

                        node.GetComponent<Node>().middleX = i + 1;
                        node.GetComponent<Node>().middleY = j + 1;

                        node.GetComponent<Node>().bottomX = i;
                        node.GetComponent<Node>().bottomY = j;
                    }
                    else
                    {
                        node.GetComponent<Node>().topX = i;
                        node.GetComponent<Node>().topY = j + 1;

                        node.GetComponent<Node>().middleX = i + 1;
                        node.GetComponent<Node>().middleY = j;

                        node.GetComponent<Node>().bottomX = i;
                        node.GetComponent<Node>().bottomY = j;
                    }

                    nodes.Add(node);
                    count++;
                }
            }
        }

        yield return new WaitForSeconds(0.1f * (width + height));
        inputManager.clickEnabled = true;

        yield return new WaitForEndOfFrame();
    }
}