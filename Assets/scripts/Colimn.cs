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



    public List<CardDummy> AddCards(List<Card> cards, bool isVidible, ref bool reveal, bool check = true, bool primeParent = false)
    {

        foreach (Card card in cards)
        {
            card.SetVisible(isVidible);
            this.cards.Add(card);
            card.column = ID;
            card.indexInColumn = this.cards.Count - 1;
            if (primeParent)
                card.PrimeParent(this);
            else
                card.transform.SetParent(this.transform);
            card.SetPosition(new Vector2(0, -GameManager.instance.spacing * (this.cards.Count - 1)));

        }
        return check ? CheckPile(ref reveal) : null;
    }


    public List<CardDummy> AddCards(List<Card> cards, bool isVidible, bool check = true, bool primeParent = false)
    {

        foreach (Card card in cards)
        {
            card.SetVisible(isVidible);
            this.cards.Add(card);
            card.column = ID;
            card.indexInColumn = this.cards.Count - 1;

            if (primeParent)
                card.PrimeParent(this);
            else
                card.transform.SetParent(this.transform);
            card.SetPosition(new Vector2(0, -GameManager.instance.spacing * (this.cards.Count - 1)));

        }
        return check ? CheckPile() : null;
    }

    private void Update()
    {

        /*int i = 0;
        foreach (Card card in cards)
        {

            card.rect.anchoredPosition = new Vector3(card.rect.anchoredPosition.x, -GameManager.instance.spacing * i++, 0);
        }*/
    }

    public List<Card> DragCards(int indexInColumn, bool restrictedMovement = true)
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
                if (restrictedMovement)
                {
                    if (number-- != cards[i].number)
                    {
                        cardsToRemove.Clear();
                        return null;
                    }

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

        return cardsToRemove;

    }

    public List<Card> RemoveCards(int indexInColumn, ref bool reveal)
    {
        List<Card> cardsToRemove = new List<Card>();
        while (indexInColumn < cards.Count)
        {
            cardsToRemove.Add(cards[indexInColumn]);
            Destroy(cards[indexInColumn].gameObject);
            cards.RemoveAt(indexInColumn);

        }
        if (cards.Count == 0) return cardsToRemove;
        reveal = (cards.Last().numberGraphic.enabled);
        cards.Last().SetVisible(true);
        return cardsToRemove;
    }

    public List<Card> RemoveCards(int indexInColumn)
    {
        Debug.Log("aaaa");
        List<Card> cardsToRemove = new List<Card>();
        while (indexInColumn < cards.Count)
        {
            cardsToRemove.Add(cards[indexInColumn]);
            Destroy(cards[indexInColumn].gameObject);
            cards.RemoveAt(indexInColumn);

        }
        if (cards.Count > 0)
            cards.Last().SetVisible(true);
        return cardsToRemove;
    }
    public List<CardDummy> CheckPile(ref bool revealed)
    {

        List<CardDummy> cardsFullDummy = new List<CardDummy>();
        int removeAfterIndex = 52;
        int check = 13;
        for (int i = 0; i < cards.Count; i++)
        {
            if (!cards[i].symbol.enabled) continue;
            else if (cards[i].number == 13) removeAfterIndex = i;

            if (cards.Count - i < check)
            {//break if no chance to pile
                return null;
            }

            if (cards[i].number != check)
            {
                check = 13;
                removeAfterIndex = i;
                cardsFullDummy.Clear();
                continue;
            }
            cardsFullDummy.Add(new CardDummy(cards[i].number, cards[i].sign));

            if (check == 1)
            {
                bool reveal = false;
                RemoveCards(removeAfterIndex, ref reveal);
                revealed = reveal;
                GameManager.instance.CheckWin();
                return cardsFullDummy;
            }


            check--;


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

    public List<CardDummy> CheckPile()
    {

        List<CardDummy> cardsFullDummy = new List<CardDummy>();
        int removeAfterIndex = 52;
        int check = 13;
        for (int i = 0; i < cards.Count; i++)
        {
            if (!cards[i].symbol.enabled) continue;
            else if (cards[i].number == 13) removeAfterIndex = i;

            if (cards.Count - i < check)
            {//break if no chance to pile
                return null;
            }

            if (cards[i].number != check)
            {
                check = 13;
                removeAfterIndex = i;
                cardsFullDummy.Clear();
                continue;
            }
            cardsFullDummy.Add(new CardDummy(cards[i].number, cards[i].sign));

            if (check == 1)
            {

                RemoveCards(removeAfterIndex);
                GameManager.instance.CheckWin();
                return cardsFullDummy;
            }


            check--;


        }
        return null;
    }


}
