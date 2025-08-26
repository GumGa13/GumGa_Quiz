using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Quiz : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI questionText;
    [SerializeField] QuestionSO question;
    //[SerializeField] TextMeshProUGUI[] answerTextArr;
    [SerializeField] GameObject[] answerButtons;
    [SerializeField] Sprite defaultAnswerSprite;
    [SerializeField] Sprite correctAnswerSprite;

    void Start()
    {
        GetNextQuestion();  
    }

    private void GetNextQuestion()
    {
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
        questionText.text = question.GetQuestion();

        for (int i = 0; i < answerButtons.Length; i++)
        {
            //answerTextArr[i].text = question.GetAnswers(i);
            answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = question.GetAnswers(i);
        }
    }

    public void OnAnswerButtonClicked(int index)
    {
        if (index == question.GetCorrectAnswerIndex())
        {
            questionText.text = "수입산 하리보만큼 끈질긴 것은 없습니다.";
            answerButtons[index].GetComponent<Image>().sprite = correctAnswerSprite;
        }
        else
        {
            questionText.text = "우매하구나.";
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
