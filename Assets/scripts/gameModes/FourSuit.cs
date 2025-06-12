using UnityEngine;

public class FourSuit : GameMode
{
    public override bool Check(int bottomCardSign, int topCardSign)
    {
        return bottomCardSign == topCardSign;
    }
}
