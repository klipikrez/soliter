using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[SerializeField]
public class CardDummy
{
    [SerializeField]
    public int number;
    [SerializeField]
    public int symbol;
    public CardDummy(int number, int symbol)
    {
        this.number = number;
        this.symbol = symbol;
    }
}

public abstract class Move
{

    public abstract void Undo(GameManager manager);
}

public class DealMove : Move
{
    //List<int[]> fullSet = new List<int[]>();
    List<int> num;
    List<List<CardDummy>> fullSet = new List<List<CardDummy>>();

    public DealMove(List<List<CardDummy>> fullSet, List<int> num)
    {
        this.fullSet = fullSet;
        this.num = num;
    }
    public override void Undo(GameManager manager)
    {

        int i = 0;
        foreach (int number in num)
        {
            foreach (CardDummy dummy in fullSet[i++])
            {


                GameObject obj = GameObject.Instantiate(manager.cardPrefab);
                Card card = obj.GetComponent<Card>();

                CardDummy info = dummy;
                card.Inicialize(info.number, info.symbol, number, manager.columns[number].cards.Count);
                manager.columns[number].AddCards(new List<Card>() { card }, true, false);

                card.ResetSize();
            }

        }
        foreach (Colimn column in manager.columns.Reverse())
        {
            Card card = column.RemoveCards(column.cards.Count - 1)[0];
            CardDummy dummy = new CardDummy(card.number, card.sign);
            manager.nonRandomCardsToDeal.Push(dummy);
        }
        manager.UpdateDeckGraphic();
    }
}

public class CardMove : Move
{
    int fromColumn, toColumn;
    int fromIdex, toIndex;
    List<CardDummy> fullSet = new List<CardDummy>();

    bool revealed;
    bool clearRevealed;
    public CardMove(int fromColumn, int toColumn, int fromIdex, int toIndex, bool revealed, List<CardDummy> fullSet, bool clearRevealed = false)
    {
        this.fromColumn = fromColumn;
        this.toColumn = toColumn;
        this.fromIdex = fromIdex;
        this.toIndex = toIndex;
        this.revealed = revealed;
        this.fullSet = fullSet;
        this.clearRevealed = clearRevealed;
    }

    public override void Undo(GameManager manager)
    {

        if (fullSet != null)
        {
            if (clearRevealed)
            {
                manager.columns[toColumn].cards.Last().SetVisible(false);
            }
            List<Card> cardsToAdd = new List<Card>();
            foreach (CardDummy dummy in fullSet)
            {
                Debug.Log(dummy.number);
                GameObject obj = GameObject.Instantiate(manager.cardPrefab);
                Card card = obj.GetComponent<Card>();

                CardDummy info = dummy;
                card.Inicialize(info.number, info.symbol, toColumn, manager.columns[toColumn].cards.Count);
                manager.columns[toColumn].AddCards(new List<Card>() { card }, true, false);

                card.ResetSize();
                cardsToAdd.Add(card);
            }
        }
        List<Card> cards = manager.columns[toColumn].DragCards(toIndex, true);
        if (revealed)
        {
            manager.columns[fromColumn].cards.Last().SetVisible(false);
        }
        manager.columns[fromColumn].AddCards(cards, true);

    }
}