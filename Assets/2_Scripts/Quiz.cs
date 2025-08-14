using TMPro;
using UnityEngine;

public class Quiz : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI questionText;
    [SerializeField] QuestionSO question;
    [SerializeField] TextMeshProUGUI[] answerTextArr;

    void Start()
    {
        questionText.text = question.GetQuestion();

        answerTextArr[0].text = question.GetAnswers(0);
        answerTextArr[1].text = question.GetAnswers(1);
        answerTextArr[2].text = question.GetAnswers(2);
        answerTextArr[3].text = question.GetAnswers(3);
    }
}
