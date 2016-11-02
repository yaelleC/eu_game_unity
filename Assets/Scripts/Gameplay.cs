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

    // log of all actions performed in the game
    public List<Action> actions;
}

