using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Colimn : MonoBehaviour
{
    [SerializeField]
    public List<Card> cards = new List<Card>();
    public int ID = 52;
    public Transform top;


    private void OnEnable()
    {
        ScreenOrientation.OnRotateScreen += CalculateSpacing;
        ScreenOrientation.OnRotateScreen += SetCardNumberSize;
    }

    private void OnDestroy()
    {
        // Also unsubscribe in OnDestroy to ensure cleanup if the object is destroyed.
        ScreenOrientation.OnRotateScreen -= CalculateSpacing;
        ScreenOrientation.OnRotateScreen -= SetCardNumberSize;
    }
    public List<CardDummy> AddCards(List<Card> cards, bool isVidible, ref bool reveal, bool primeParent = false)
    {

        foreach (Card card in cards)
        {
            card.SetVisible(isVidible);


            card.SetColumn(ID);

            /*if (cards.Count > 0)
                card.SetSpacing(cards[cards.Count - 1].IsVisible() ? GameManager.instance.spacing : GameManager.instance.spacing / 4);
            else
                card.SetSpacing(0);*/

            this.cards.Add(card);
            if (primeParent)
                card.PrimeParent(this);
            else
                card.transform.SetParent(this.transform);



        }
        CalculateSpacing();
        DarkenUnacssesableCards();

        return CheckPile(ref reveal);
    }

    public void SetCardNumberSize()
    {
        foreach (Card card in cards)
            card.ChangeSize(ScreenOrientation.instance.isVertical);
    }

    public List<CardDummy> AddCards(List<Card> cards, bool isVidible, bool primeParent = false)
    {
        bool throwAway = true;
        return AddCards(cards, isVidible, ref throwAway, primeParent);
    }
    /*
        void Update()
        {
            CalculateSpacing();
        }*/
    public void CalculateSpacing()
    {
        if (cards.Count == 0) return;

        // Assume each card has a RectTransform and they all have the same height.

        float cardHeight = cards[0].rect.rect.height;

        // First, calculate the total spacing offset (unscaled).
        float totalOffset = 0f;
        for (int i = 1; i < cards.Count - (ScreenOrientation.instance.isVertical ? 0 : 1); i++)
        {
            // Use a different spacing if the previous card isn’t visible.
            float spacing = GameManager.instance.spacing * (cards[i - 1].IsVisible() ? 1f : 0.3f);
            totalOffset += spacing;
        }

        // Since the first card sits at y = 0 and its full height is below that (assuming a top pivot),
        // the full column height is the height of the top card plus the cumulative spacing offset.
        float columnHeight = cardHeight + totalOffset + (ScreenOrientation.instance.isVertical ? 600.41f : 0);

        // Compute the scale multiplier so the column will fit the screen height.
        float ScaleMultiply = columnHeight > (GameManager.instance.canvasRect.rect.height)
        ? (GameManager.instance.canvasRect.rect.height - cardHeight - (ScreenOrientation.instance.isVertical ? 600.41f : 0)) / totalOffset
        : 1;
        //if (ID == 0) Debug.Log("columnHeight: " + columnHeight + " -- screenHeight: " + GameManager.instance.canvasRect.rect.height + " -- ofset:" + totalOffset + " -- static: " + (cardHeight + (ScreenOrientation.instance.isVertical ? 460.41f : 0)));
        // Now reposition cards with the scaled spacing.
        float yPos = 0f;
        cards[0].SetPosition(new Vector2(0, 0));
        for (int i = 1; i < cards.Count; i++)
        {
            float spacing = GameManager.instance.spacing
            * (cards[i - 1].IsVisible() ?
            1f :
            (ScreenOrientation.instance.isVertical ? 0.86f : 0.3f));
            yPos -= spacing * ScaleMultiply;
            cards[i].SetPosition(new Vector2(0, yPos));
        }


    }



    public List<Card> DragCards(int indexInColumn)
    {

        List<Card> cardsToRemove = new List<Card>();
        List<Card> cardsToStay = new List<Card>();
        int number = cards[indexInColumn].number;
        for (int i = 0; i < cards.Count; i++)
        {
            if (i < indexInColumn)
            {
                cardsToStay.Add(cards[i]);
            }
            else
            {
                if (number-- != cards[i].number)
                {
                    cardsToRemove.Clear();
                    return null;
                }


                cardsToRemove.Add(cards[i]);
            }
        }

        foreach (Card card in cardsToRemove)
        {
            card.SnapToParent();
            card.transform.SetParent(GameManager.instance.moveObj.transform);
        }

        cards = cardsToStay;
        DarkenUnacssesableCards();
        CalculateSpacing();
        return cardsToRemove;

    }

    public List<Card> RemoveCards(int removeAfterIndexInclusive/*, ref bool reveal*/)
    {
        List<Card> cardsToRemove = new List<Card>();
        while (removeAfterIndexInclusive < cards.Count)
        {
            cardsToRemove.Add(cards[removeAfterIndexInclusive]);
            Destroy(cards[removeAfterIndexInclusive].gameObject);
            cards.RemoveAt(removeAfterIndexInclusive);

        }
        if (cards.Count == 0) return cardsToRemove;
        //reveal = (cards.Last().numberGraphic.enabled);
        cards.Last().SetVisible(true);
        DarkenUnacssesableCards();
        CalculateSpacing();
        return cardsToRemove;
    }
    /*
        public List<Card> RemoveCards(int indexInColumn)
        {
            bool throwAway = false;
            return RemoveCards(indexInColumn, ref throwAway);
        }*/
    public List<CardDummy> CheckPile(ref bool revealed)
    {
        if (cards.Count < 13) return null;

        List<CardDummy> completedSequence = new List<CardDummy>();
        int check = 1;
        for (int i = cards.Count - 1; i >= 0; i--)
        {
            if (!cards[i].IsVisible()) { Debug.Log("Invisibel [ " + i + "]: " + cards[i]); return null; }
            if (cards[i].number != check) { Debug.Log("invalid mumber at [" + i + "]: " + cards[i].number + " != check"); return null; }

            completedSequence.Add(new CardDummy(cards[i].number, cards[i].sign));

            if (check == 13)
            {
                revealed = false;
                if (i - 1 >= 0)
                    revealed = !cards[i - 1].numberGraphic.enabled;
                RemoveCards(i);
                //Debug.Log("done reveal??:" + revealed);
                GameManager.instance.CheckWin();
                completedSequence.Reverse();
                AudioManager.Play("pile", 0.75f);
                Background.Instance.Flash(1f);
                return completedSequence;
            }
            else
            {
                if (i - 1 >= 0)
                    if (!GameManager.instance.gameMode.Check(cards[i].sign, cards[i - 1].sign)) return null;
            }
            check++;
        }
        return null;
    }

    public void RecalculateOrder()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].transform.SetSiblingIndex(i);
        }
    }

    public void DarkenUnacssesableCards()
    {
        if (cards.Count == 0) return;
        else if (cards.Count == 1) cards[0].SetDarken(false);

        int numberCheck = cards[cards.Count - 1].number;
        cards[cards.Count - 1].SetDarken(false);
        for (int i = cards.Count - 2; i >= 0; i--)
        {
            if (!cards[i].IsVisible()) return;
            if (numberCheck > 13) numberCheck = -52; // set check number to -52 if we need to set all cards to be darkend

            if (cards[i].number - 1 == numberCheck && GameManager.instance.gameMode.Check(cards[i].sign, cards[i + 1].sign))
            {
                cards[i].SetDarken(false);
                numberCheck++;
            }
            else
            {
                numberCheck = -52; // set check number to -52 if we need to set all cards to be darkend
                cards[i].SetDarken(true);
            }

        }

    }
}
