using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{
    int correctAnswers = 0;
    int questionSeen = 0;

    public int GetCorrectAnswers()
    {
        return correctAnswers;
    }

    public void IncrementCorrectAnswers()
    {
        correctAnswers++;
    }

    public int GetQuestionsSeen()
    {
        return questionSeen;
    }

    public void IncrementQuestionsSeen()
    {
        questionSeen++;
    }

    public int CalculateScore()
    {
        return Mathf.RoundToInt((float)correctAnswers / questionSeen * 100f);
    }
}
