using System;

[Serializable]
public class Feedback
{
    private const string DateTimeOffsetFormatString = "yyyy-MM-dd HH:mm:ss";

    public string timestamp;
    public string name;
    public string message;
    public string final;
    public string type;

    public Feedback(string p_feedback, string p_message, string p_final, string p_type)
    {
        timestamp = DateTime.Now.ToString(DateTimeOffsetFormatString);
        name = p_feedback;
        message = p_message;
        final = p_final;
        type = p_type;
    }
}