using System;
using System.Collections.Generic;

[Serializable]
public class Gameplay
{

    private const string DateTimeOffsetFormatString = "yyyy-MM-ddTHH:mm:sszzz";

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

    public DateTime timestarted;
    public DateTime lastActionTime;
    public DateTime timeEnded;
    public string timestarted_string;
    public string lastActionTime_string;
    public string timeEnded_string;

    // 'win', 'lose' or 'end'
    public string gpWon;

    // constructor
    public Gameplay()
    {
        player = new Player();
        actions = new List<Action>();
        feedback = new List<Feedback>();
        timestarted = DateTime.Now;
        timestarted_string = timestarted.ToString(DateTimeOffsetFormatString);
        lastActionTime = DateTime.Now;
        lastActionTime_string = lastActionTime.ToString(DateTimeOffsetFormatString);
    }

    public void Start()
    {
        timestarted = DateTime.Now;
        timestarted_string = timestarted.ToString(DateTimeOffsetFormatString);
    }
    public void Update()
    {
        lastActionTime = DateTime.Now;
        lastActionTime_string = timestarted.ToString(DateTimeOffsetFormatString);
    }

    public void End(string endingState)
    {
        gpWon = endingState;
        timeEnded = DateTime.Now;
        timeEnded_string = timeEnded.ToString(DateTimeOffsetFormatString);
    }

    
}

