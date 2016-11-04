using System;
using System.Collections.Generic;

[Serializable]
public class Gameplay
{
    // id of the GP 
    // from engage server if internet was available,
    // its negative position (pos * -1) in the gameplays array otherwise
    public int idGP;
    
    // player playing the game
    public Player player;

    // scores JSON string
    public string scores;

    // log of all actions performed in the game
    public List<Action> actions;

    public DateTime timestarted;
    public DateTime lastActionTime;

    // constructor
    public Gameplay()
    {
        player = new Player();
        actions = new List<Action>();
    }

}

