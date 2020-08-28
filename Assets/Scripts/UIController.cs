using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public GameObject dropDownMenu;
    public GameObject menuButton;
    public GameObject endMenu;

    public GameObject UIScore;
    public GameObject UIEndScore;
    public GameObject UIBestScore;
    public GameObject UIHexagonCount;
    public GameObject UIStarCount;
    public GameObject UIBombCount;
    public GameObject UIMoves;

    public bool symbolState;

    InputManager inputManager;
    GridGenerator gridGenerator;

    private void Start()
    {
        inputManager = FindObjectOfType<InputManager>();
        gridGenerator = FindObjectOfType<GridGenerator>();

    }

    public void toggleMenu() //toggel menu active state
    {
        dropDownMenu.SetActive(!dropDownMenu.activeSelf);
        inputManager.clickEnabled = !dropDownMenu.activeSelf;
    }
    public void showEndMenu() //toggel menu active state
    {
        endMenu.SetActive(true);
        menuButton.SetActive(false);
        inputManager.clickEnabled = false;
    }

    public void toggleSymbols() //toggle hexagon symbols
    {
        symbolState = !symbolState;
        foreach (GameObject hexagon in gridGenerator.hexagonMatrix)
        {
            hexagon.GetComponent<Hexagon>().setSymbol(symbolState);
        }
    }

    public void reloadScene() //reload game (new game)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

    public void setMoves(int moves) //set moves UI
    {
        UIMoves.GetComponent<Text>().text = moves.ToString();
    }

    public void setScore(int score) //set score ui
    {
        UIScore.GetComponent<Text>().text = score.ToString();
    }

    public void setEndScore(int endscore) //set endscore ui
    {
        UIEndScore.GetComponent<Text>().text = endscore.ToString();
    }

    public void setBestScore(int bestscore) //set bestscore ui
    {
        UIBestScore.GetComponent<Text>().text = "Bestscore : " + bestscore.ToString();
    }

    public void setHexagonCount(int hexagoncount) //set hexagon count ui
    {
        UIHexagonCount.GetComponent<Text>().text = hexagoncount.ToString();
    }

    public void setStarCount(int starcount) //set star count ui
    {
        UIStarCount.GetComponent<Text>().text = starcount.ToString();
    }

    public void setBombCount(int bombcount) //set bomb count ui
    {
        UIBombCount.GetComponent<Text>().text = bombcount.ToString();
    }
}