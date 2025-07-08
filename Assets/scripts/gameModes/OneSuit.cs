using UnityEngine;

public class OneSuit : GameMode
{
    public int id { get { return 1; } }
    public bool Check(int bottomCardSign, int topCardSign)
    {
        return true;
    }
}
