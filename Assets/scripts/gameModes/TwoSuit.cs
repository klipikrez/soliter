using UnityEngine;

public class TwoSuit : GameMode
{
    public int id { get { return 2; } }
    public bool Check(int bottomCardSign, int topCardSign)
    {
        return (bottomCardSign <= 1 && topCardSign <= 1) || (bottomCardSign >= 2 && topCardSign >= 2);
    }
}
