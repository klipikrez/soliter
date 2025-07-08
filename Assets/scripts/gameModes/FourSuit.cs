using UnityEngine;

public class FourSuit : GameMode
{
    public int id { get { return 4; } }

    public bool Check(int bottomCardSign, int topCardSign)
    {
        Debug.Log(bottomCardSign + " -- " + topCardSign);
        return bottomCardSign == topCardSign;
    }
}
