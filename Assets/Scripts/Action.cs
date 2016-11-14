using System;

[Serializable]
public class Action
{
    private const string DateTimeOffsetFormatString = "yyyy-MM-dd HH:mm:ss";

    public string timestamp;
    public string action;
    public string score;
    public float mark;

    public Action (string p_action, string p_score, float p_mark)
    {
        timestamp = DateTime.Now.ToString(DateTimeOffsetFormatString);
        action = p_action;
        score = p_score;
        mark = p_mark;
    }
}

