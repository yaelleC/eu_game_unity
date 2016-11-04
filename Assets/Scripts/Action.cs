using System;

[Serializable]
public class Action
{
    public DateTime timestamp;
    public string action;
    public string score;
    float mark;

    public Action (string p_action, string p_score, float p_mark)
    {
        timestamp = new DateTime();
        action = p_action;
        score = p_score;
        mark = p_mark;
    }
}

