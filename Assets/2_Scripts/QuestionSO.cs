// Assets/2_Scripts/QuestionSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "New Question", menuName = "Quiz/Question")]
public class QuestionSO : ScriptableObject
{
    [TextArea(2, 6)]
    [SerializeField] string question = "여기에 질문";
    [SerializeField] string[] answers = new string[4];
    [SerializeField] int correctAnswerIndex = 0;
    [SerializeField] string hint = "";

    public string GetQuestion() => question;
    public string GetAnswers(int index) => answers[index];
    public string GetCorrectAnswer() => answers[correctAnswerIndex];
    internal int GetCorrectAnswerIndex() => correctAnswerIndex;
    public string GetHint() => hint;

    public void SetData(string q, string[] a, int correctIndex, string hintText = "")
    {
        question = q;
        answers = a;
        correctAnswerIndex = Mathf.Clamp(correctIndex, 0, 3);
        hint = hintText;
    }
}
