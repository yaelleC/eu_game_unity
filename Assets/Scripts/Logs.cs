using UnityEngine;
using System.Collections.Generic;
using System;

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
    public void logAction(Action action, int p_idGP)
    {
        foreach (Gameplay gp in gameplays)
        {
            if (gp.idGP == p_idGP)
            {
                gp.actions.Add(action);
            }
        }
    }

    // log feedback to a specific gameplay
    public void logFeedback(Feedback feedback, int p_idGP)
    {
        foreach (Gameplay gp in gameplays)
        {
            if (gp.idGP == p_idGP)
            {
                gp.feedback.Add(feedback);
            }
        }
    }

    public bool feedbackHasBeenTriggered(string feedbackName, int p_idGP)
    {
        Gameplay gp = getGameplayByID(p_idGP);
        if (gp == null)
        {
            // TODO deal with error
            return false;
        }
        foreach (Feedback f in gp.feedback)
        {
            if (f.feedback.Equals(feedbackName))
            {
                return true;
            }
        }
        return false;
    }

    public void updateLastActionTime(int p_idGP)
    {
        getGameplayByID(p_idGP).Update();
    }
    public void updateScores(string scores, int p_idGP)
    {
        getGameplayByID(p_idGP).scores = scores;
    }
    public void endGameplay(string GPwon, int p_idGP)
    {
        getGameplayByID(p_idGP).End(GPwon);
    }

    public Gameplay getGameplayByID (int p_idGP)
    {
        foreach (Gameplay gp in gameplays)
        {
            if (gp.idGP == p_idGP)
            {
                return gp;
            }
        }
        return null;
    }
}
