using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;



public class Card : MonoBehaviour
    , IPointerDownHandler
    , IPointerUpHandler
    , IDragHandler
    , IPointerEnterHandler
    , IPointerExitHandler
{
    public int number;
    public int sign;
    public int column { get; private set; }
    public int indexInColumn;
    public Image background;
    Sprite bg = null;
    public Image symbol;
    public Image bigSymbol;
    [SerializeField]
    public RectTransform rect;
    public TMP_Text numberGraphic;
    public Sprite back;
    public Sprite front;
    public bool dragging = false;
    Colimn primeParent = null;//when card gets to parent column set prime parent as parent

    Vector2 desiredPosition = Vector2.one;
    float distance;
    Vector2 previousPoint = Vector2.positiveInfinity;
    public void Inicialize(int num, int sig)
    {
        rect.localScale = Vector3.one;
        number = num;
        sign = sig;

        symbol.sprite = GameManager.instance.symbols[sign];
        bigSymbol.sprite = GameManager.instance.symbols[sign];
        if (sig >= 2)
        {
            symbol.color = GameManager.instance.SymbolColors[1];
            bigSymbol.color = GameManager.instance.SymbolColors[1];
        }
        else
        {
            symbol.color = GameManager.instance.SymbolColors[0];
            bigSymbol.color = GameManager.instance.SymbolColors[0];
        }

        switch (number)
        {
            case 10:
                {
                    numberGraphic.text = number.ToString();
                    numberGraphic.rectTransform.localScale = new Vector2(0.7f, 1);
                    break;
                }
            case 11:
                {
                    bigSymbol.gameObject.SetActive(false);
                    numberGraphic.text = "J";
                    background.sprite =
                   bg = GameManager.instance.imageBg[0];
                    break;
                }
            case 12:
                {
                    bigSymbol.gameObject.SetActive(false);
                    numberGraphic.text = "Q";
                    background.sprite =
                   bg = GameManager.instance.imageBg[1];
                    break;
                }
            case 13:
                {
                    bigSymbol.gameObject.SetActive(false);
                    numberGraphic.text = "K";
                    background.sprite =
                    bg = GameManager.instance.imageBg[2];
                    break;
                }
            default:
                {
                    numberGraphic.text = number.ToString();
                    break;
                }
        }

        rect.position = new Vector3(rect.position.x, rect.position.y, 0);


    }

    public void SetColumn(int column)
    {
        this.column = column;
        this.indexInColumn = GameManager.instance.columns[column].cards.Count;
    }

    public void PrimeParent(Colimn t)
    {
        this.primeParent = t;
    }
    public void ResetSize()
    {
        rect.offsetMin = new Vector2(0, rect.offsetMin.y);
        rect.offsetMax = new Vector2(0, rect.offsetMax.y);
        rect.localScale = new Vector2(1, 1);
    }
    public bool SetVisible(bool val)
    {
        if (val == symbol.enabled) return false;
        background.sprite = !val ? back : bg == null ? front : bg;
        if (number <= 10)
            bigSymbol.gameObject.SetActive(val);
        symbol.enabled = val;
        numberGraphic.enabled = val;
        background.raycastTarget = val;
        return true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {


        if (GameManager.instance.moveCards.Count != 0) return;
        if (symbol.enabled == false) return;
        List<Card> crds = GameManager.instance.columns[column].DragCards(indexInColumn);
        if (crds == null) return;
        previousPoint = eventData.position;
        GameManager.instance.StartDraging(crds);

    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragging)
            if (previousPoint != Vector2.positiveInfinity)
            {
                distance += Vector2.Distance(previousPoint, eventData.position);
                previousPoint = eventData.position;
            }
    }

    public void SnapToParent()
    {
        if (primeParent != null)
        {

            transform.SetParent(primeParent.transform);
            GameManager.instance.columns[column].RecalculateOrder();
            primeParent = null;
            SetSpeed(1);
            SetPosition(desiredPosition);
        }
    }



    private void Update()
    {



        if (primeParent != null)
        {

            Vector2 newPos = new Vector2(primeParent.top.position.x, primeParent.top.position.y) + (-desiredPosition / GameManager.instance.spacing) * GameManager.instance.GetDistanceBetweenCards();
            //Debug.Log(Vector2.Distance(newPos, rect.position));


            if (Vector2.Distance(newPos, rect.position) < 0.1f)
            {
                transform.SetParent(primeParent.transform);
                GameManager.instance.columns[column].RecalculateOrder();
                primeParent = null;
                SetSpeed(1);
            }
            rect.position = Vector2.Lerp(rect.position, newPos, Time.deltaTime * 10 * speedMultiply);
            return;
        }
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, desiredPosition, Time.deltaTime * 10 * speedMultiply);

    }


    float speedMultiply = 1;
    public void SetPosition(Vector2 pos)
    {

        desiredPosition = pos;
    }

    public void SetSpeed(float speedMultiply)
    {
        this.speedMultiply = speedMultiply;
    }

    public void SetPositionImeniate(Vector2 pos)
    {
        rect.position = pos;

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!dragging) return;
        if (distance < 10f)
        {
            Debug.Log("Optim");
            GameManager.instance.OptimalPlace();
            return;
        }
        Debug.Log("Norma");
        GameManager.instance.CheckClosestToPlace();
        previousPoint = Vector2.positiveInfinity;
        distance = 0;
    }



    public void OnPointerEnter(PointerEventData eventData)
    {
        //background.color = Color.green;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //background.color = Color.white;
    }


}