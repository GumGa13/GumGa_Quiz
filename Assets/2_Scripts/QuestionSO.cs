using UnityEngine;

[CreateAssetMenu(fileName = "New Question", menuName = "Quiz/Question")]

public class QuestionSO : ScriptableObject
{
    [TextArea(2, 6)]
    [SerializeField] string question = "¿©±â¿¡ Áú¹®À» À¡À¡";
    [SerializeField] string[] answers = new string[4];
    [SerializeField] int correctAnswerIndex = 0;

    public string GetQuestion()
    {
        return question;
    }

    public string GetAnswers(int index)
    {
        return answers[index];
    }

    public string GetCorrectAnswer()
    {
        return answers[correctAnswerIndex];
    }
}
