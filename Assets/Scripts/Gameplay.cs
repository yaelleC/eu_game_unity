using System;
using System.Collections.Generic;
using System.Globalization;

[Serializable]
public class Gameplay
{
    private const string DateTimeOffsetFormatString = "yyyy-MM-dd HH:mm:ss";

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

    // log of all feedback triggered in the game
    public List<Feedback> feedback;
    
    public string timeStarted;
    public string lastAction;
    public string timeEnded;

    // 'win', 'lose' or 'end'
    public string gpWon;

    // constructor
    public Gameplay()
    {
        player = new Player();
        actions = new List<Action>();
        feedback = new List<Feedback>();
        DateTime now = DateTime.Now;
        timeStarted = now.ToString(DateTimeOffsetFormatString);
        lastAction = now.ToString(DateTimeOffsetFormatString);
    }

    public void Start()
    {
        timeStarted = DateTime.Now.ToString(DateTimeOffsetFormatString);
    }
    public void Update()
    {
        lastAction = DateTime.Now.ToString(DateTimeOffsetFormatString);
    }

    public void End(string endingState)
    {
        gpWon = endingState;
        timeEnded = DateTime.Now.ToString(DateTimeOffsetFormatString);
    }

    public DateTime getTimeLastAction()
    {
        DateTime dt = DateTime.ParseExact(lastAction, DateTimeOffsetFormatString, CultureInfo.InvariantCulture);

        return dt;
    }
    
}

