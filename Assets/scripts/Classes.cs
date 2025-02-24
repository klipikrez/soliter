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

    Dictionary<int, List<CardDummy>> completedSequences = new Dictionary<int, List<CardDummy>>();

    public DealMove(Dictionary<int, List<CardDummy>> completedSequences)
    {
        this.completedSequences = completedSequences;
    }
    public override void Undo(GameManager manager)
    {


        foreach (KeyValuePair<int, List<CardDummy>> completedSequence in completedSequences)
        {
            foreach (CardDummy dummy in completedSequence.Value)
            {
                if (dummy.number == 1) continue; //skip last card if it was used to complete sequence

                GameManager.instance.SpawnCard(dummy.number, dummy.symbol, completedSequence.Key, true);
            }

        }
        foreach (Colimn column in manager.columns.Reverse())
        {
            if (completedSequences.ContainsKey(column.ID))
            {//if the column had a completed sequence then we can just take the last card form that sequnce
                manager.nonRandomCardsToDeal.Push(completedSequences[column.ID].Last());
                continue;
            }
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
    int toIndex;
    List<CardDummy> fullSet = new List<CardDummy>();

    bool moveReveal, clearReveal;
    public CardMove(int fromColumn, int toColumn, int toIndex, bool moveReveal, List<CardDummy> fullSet, bool clearReveal)
    {
        this.fromColumn = fromColumn;
        this.toColumn = toColumn;
        this.toIndex = toIndex;
        this.moveReveal = moveReveal;
        this.clearReveal = clearReveal;
        this.fullSet = fullSet;
        Debug.Log("moveReveal: " + moveReveal + "  --  clearReveal: " + clearReveal);

    }

    public override void Undo(GameManager manager)
    {

        if (fullSet != null)
        {
            if (clearReveal)
            {
                manager.columns[toColumn].cards.Last().SetVisible(false);
            }

            if (moveReveal)
            {
                manager.columns[fromColumn].cards.Last().SetVisible(false);
            }

            foreach (CardDummy dummy in fullSet)
            {
                //place cards back in respective columns
                int addToColumn = manager.columns[toColumn].cards.Count >= toIndex ? fromColumn : toColumn;
                manager.SpawnCard(dummy.number, dummy.symbol, addToColumn, true);
            }
            return;
        }
        List<Card> cards = manager.columns[toColumn].DragCards(toIndex, true);
        if (moveReveal)
        {
            manager.columns[fromColumn].cards.Last().SetVisible(false);
        }
        manager.columns[fromColumn].AddCards(cards, true);

    }
}