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
    
    // constructor
    public Logs()
    {
        player = new Player();
        offlinePlayers = new List<Player>();
        gameplays = new List<Gameplay>();
    }

    // saves the object as a json formatted string
    public string SaveToString()
    {
        return JsonUtility.ToJson(this);
    }
    // saves the object as a json formatted string
    public string SaveToPrettyString()
    {
        return JsonUtility.ToJson(this, true);
    }

    // log an action to a specific gameplay
    public void logAction (Action action, int p_idGP)
    {
        foreach(Gameplay gp in gameplays)
        {
            if (gp.idGP == p_idGP)
            {
                gp.actions.Add(action);
            }
        }
    }

    public void updateLastActionTime (int p_idGP)
    {
        foreach (Gameplay gp in gameplays)
        {
            if (gp.idGP == p_idGP)
            {
                gp.lastActionTime = new DateTime();
            }
        }
    }
}
