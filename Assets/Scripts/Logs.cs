using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class Logs
{
    // successfully logged in player
    public Player player;

    // if internet was not available but player played nontheless
    public List<Player> offlinePlayers;

    // last downloaded leaderboard (JSON string)
    public String leaderboard;

    // list of gameplays logged 
    public List<Gameplay> gameplays;

    // config file in json form for current player's version
    public String configFile;

    // saves the object as a json formatted string
    public string SaveToString()
    {
        return JsonUtility.ToJson(this);
    }
}
