using UnityEngine;
using UnityEngine.UIElements;

public class InputManager : MonoBehaviour
{
    public Node targetNode;

    public bool clickEnabled;

    Vector3 clickBegan;
    Vector3 clickEnded;

    public GameObject activeOverlay;
    public GameObject rightOverlay;
    public GameObject leftOverlay;

    GridGenerator gridGenerator;

    private void Start()
    {
        clickEnabled = true;
        gridGenerator = FindObjectOfType<GridGenerator>();
    }

    private void Update()
    {
        if(clickEnabled && Input.GetMouseButtonDown((int)MouseButton.LeftMouse)) //if mouse is down register the point
        {
            Vector3 clickPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0f, 0f, 10f));

            if (clickPoint.x > -gridGenerator.leftBottom.x + gridGenerator.transform.position.x
               &&
               clickPoint.y > -(gridGenerator.leftBottom.y + gridGenerator.transform.position.y)
               && 
               clickPoint.x < gridGenerator.leftBottom.x + gridGenerator.transform.position.x
               &&
               clickPoint.y < (gridGenerator.leftBottom.y + gridGenerator.transform.position.y))
            {
                clickBegan = clickPoint;
            }
        }

        if(clickEnabled && Input.GetMouseButtonUp((int)MouseButton.LeftMouse)) //if mouse is up, if position changed select node, if not rotate object
        {
            Vector3 clickPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0f, 0f, 10f));

            if (clickPoint.x > (-gridGenerator.leftBottom.x + gridGenerator.transform.position.x) * 1.1f
               &&
               clickPoint.y > -(gridGenerator.leftBottom.y + gridGenerator.transform.position.y) * 1.1f
               &&
               clickPoint.x < (gridGenerator.leftBottom.x + gridGenerator.transform.position.x) *1.1f
               &&
               clickPoint.y < (gridGenerator.leftBottom.y + gridGenerator.transform.position.y) * 1.1f)
            {
                clickEnded = clickPoint;
                if (Vector3.Distance(clickBegan, clickEnded) < 0.5f)
                {
                    float distance = Mathf.Infinity;
                    foreach (GameObject node in gridGenerator.nodes) //find closest node
                    {
                        if (Vector3.Distance(node.transform.position, clickBegan) < distance)
                        {
                            distance = Vector3.Distance(node.transform.position, clickBegan);
                            targetNode = node.GetComponent<Node>();

                            rightOverlay.transform.position = targetNode.transform.position + Vector3.back * 0.1f;
                            rightOverlay.transform.localScale = Vector3.one * gridGenerator.side / targetNode.transform.localScale.x;
                            leftOverlay.transform.position = targetNode.transform.position + Vector3.back * 0.1f;
                            leftOverlay.transform.localScale = Vector3.one * gridGenerator.side / targetNode.transform.localScale.x;

                            rightOverlay.transform.parent = targetNode.transform;
                            leftOverlay.transform.parent = targetNode.transform;
                        }
                    }

                    if (targetNode.twoOnRight) //set overlay state
                    {
                        leftOverlay.SetActive(false);
                        rightOverlay.SetActive(true);
                        activeOverlay = rightOverlay;
                    }
                    else
                    {
                        leftOverlay.SetActive(true);
                        rightOverlay.SetActive(false);
                        activeOverlay = leftOverlay;
                    }
                }
                else
                {
                    if (targetNode != null) //rotate hexagons
                    {
                        if (Vector3.SignedAngle(clickEnded - clickBegan, targetNode.transform.position - clickBegan, Vector3.forward) > 0)
                        {
                            targetNode.rotateHexagons(false);
                        }
                        else
                        {
                            targetNode.rotateHexagons(true);
                        }
                    }
                }
            }
        }
    }
}