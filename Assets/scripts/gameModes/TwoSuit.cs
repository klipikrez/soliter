using UnityEngine;

public class TwoSuit : GameMode
{
    public override bool Check(int bottomCardSign, int topCardSign)
    {
        return (bottomCardSign <= 1 && topCardSign <= 1) || (bottomCardSign >= 2 && topCardSign >= 2);
    }
}
