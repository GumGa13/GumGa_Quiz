using UnityEngine;

[CreateAssetMenu(fileName = "New Question", menuName = "Quiz/Question")]

public class QuestionSO : ScriptableObject
{
    [TextArea(2, 6)]
    [SerializeField] string question = "Q. 세상에서 가장 끈질긴 것은?";
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
