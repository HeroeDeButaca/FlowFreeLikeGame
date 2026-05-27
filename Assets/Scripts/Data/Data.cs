using System;
using System.Collections.Generic;

[Serializable]
public class Data
{
    public string PlayerId;
    public string PlayerName;
    
    public int IconId;
    public List<bool> IconsUnlocked;

    public Data() { }
    public Data(string playerName)
    {
        PlayerId = Guid.NewGuid().ToString("N");
        PlayerName = playerName;
        IconId = 0;
        IconsUnlocked = new List<bool>();
    }
}