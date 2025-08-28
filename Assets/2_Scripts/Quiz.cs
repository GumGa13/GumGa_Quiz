using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Quiz : MonoBehaviour
{
    [Header("질문")]
    [SerializeField] TextMeshProUGUI questionText;
    [SerializeField] QuestionSO question;
    //[SerializeField] TextMeshProUGUI[] answerTextArr;

    [Header("보기")]
    [SerializeField] GameObject[] answerButtons;

    [Header("버튼 색깔")]
    [SerializeField] Sprite defaultAnswerSprite;
    [SerializeField] Sprite correctAnswerSprite;

    [Header("타이머")]
    [SerializeField] Image timerImage;
    [SerializeField] Sprite problemTimerSprite;
    [SerializeField] Sprite solutionTimerSprite;
    Timer timer;
    bool chooseAnswer = false;

    private void Start()
    {
        timer = FindFirstObjectByType<Timer>();
        GetNextQuestion();  
    }

     private void Update()
    {
        if (timer.isProblemTime)
        {
            timerImage.sprite = problemTimerSprite;
        }
        else
        {
            timerImage.sprite = solutionTimerSprite;
        }
        timerImage.fillAmount = timer.fillAmount;

        // 다음 질문 불러오기
        if (timer.loadNextQuestion)
        {
            timer.loadNextQuestion = false;
            GetNextQuestion();
        }

        // 제한시간이 끝났는데도 답을 고르지 않았을 때
        if (!timer.isProblemTime == false && !chooseAnswer == false)
        {
            DisplaySolution(-1);
        }
    }

    private void GetNextQuestion()
    {
        chooseAnswer = false;
        SetButtonState(true);
        SetDefaultButtonSprites();
        OnDisplayQuestion();
    }

    private void SetDefaultButtonSprites()
    {
        foreach (GameObject obj in answerButtons)
        {
            obj.GetComponent<Image>().sprite = defaultAnswerSprite;
        }
    }

    private void OnDisplayQuestion()
    {
        Debug.Log("Displaying question: " + question.GetQuestion());
        questionText.text = question.GetQuestion();

        for (int i = 0; i < answerButtons.Length; i++)
        {
            //answerTextArr[i].text = question.GetAnswers(i);
            answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = question.GetAnswers(i);
        }
    }

    public void OnAnswerButtonClicked(int index)
    {
        chooseAnswer = true;
        DisplaySolution(index);
        timer.CancleTimer();
    }

    private void DisplaySolution(int index)
    {
        if (index == question.GetCorrectAnswerIndex())
        {
            questionText.text = "수입산 하리보만큼 끈질긴 것은 없습니다.";
            answerButtons[index].GetComponent<Image>().sprite = correctAnswerSprite;
        }
        else
        {
            questionText.text = "우매한 것.";
        }
        SetButtonState(false);
    }

    private void SetButtonState(bool state)
    {
        foreach (GameObject obj in answerButtons)
        {
            obj.GetComponent<Button>().interactable = state;
        }
    }

}
