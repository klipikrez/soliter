using UnityEngine;

public interface GameMode
{
    int id { get; }
    public abstract bool Check(int bottomCardSign, int topCardSign);


}
