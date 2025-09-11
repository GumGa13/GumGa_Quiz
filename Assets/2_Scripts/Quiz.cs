using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Quiz : MonoBehaviour
{
    [Header("질문")]
    [SerializeField] TextMeshProUGUI questionText;
    [SerializeField] List<QuestionSO> questions = new List<QuestionSO>();
    QuestionSO currentQeustion;
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

    [Header("점수")]
    [SerializeField] TextMeshProUGUI scoreText;
    ScoreKeeper scoreKeeper;

    [Header("Progress Bar")]
    [SerializeField] Slider progressBar;

    [Header("ChatGPTClient")]
    [SerializeField] ChatGPTClient chatGPTClient;
    [SerializeField] int questionCount = 3;

    bool isGeneratingQuestions = false;

    void Start()
    {
        timer = FindFirstObjectByType<Timer>();
        scoreKeeper = FindFirstObjectByType<ScoreKeeper>();
        chatGPTClient.quizGenerateHandler += QuizGeneratedHandler;

        if (questions.Count == 0)
        {
            GenerateQuestionsifNeeded();
        }
        else
        {
            InitializeProgressBar();
        }
    }

    private void GenerateQuestionsifNeeded()
    {
        if (isGeneratingQuestions) return;

        isGeneratingQuestions = true;
        GameManager.Instance.ShowLoadingScene();

        string topicToUse = GetTrendingTopic();
        chatGPTClient.GenerateQuestions(questionCount, topicToUse);
        Debug.Log($"GenerateQuestionsifNeeded: {topicToUse}");
    }

    private string GetTrendingTopic()
    {
        string[] topics = new string[]
        {
            "과학",
            "역사",
            "음악",
            "영화",
            "스포츠",
            "기술",
            "문학",
            "예술",
            "지리",
            "음식"
        };
        int randomindex = UnityEngine.Random.Range(0, topics.Length);
        return topics[randomindex];
    }

    void QuizGeneratedHandler(List<QuestionSO> questions)
    {
        //questions = questions;
        Debug.Log($"QuizGeneratedHandler: {questions.Count} questions received.");
        isGeneratingQuestions = false;
    }

    private void InitializeProgressBar()
    {
        progressBar.maxValue = questions.Count;
        progressBar.value = 0;
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
            if (questions.Count == 0)
            {
                GenerateQuestionsifNeeded();
            }
            else
            {
                timer.loadNextQuestion = false;
                GetNextQuestion();
            }
        }

        // 제한시간이 끝났는데도 답을 고르지 않았을 때
        if (timer.isProblemTime == false && chooseAnswer == false)
        {
            DisplaySolution(-1);
        }
    }

    private void GetNextQuestion()
    {
        if (questions.Count == 0)
        {
            Debug.Log("더 이상 질문이 없습니다.");
            return;
        }

        GameManager.Instance.ShowQuizScene();
        chooseAnswer = false;
        SetButtonState(true);
        SetDefaultButtonSprites();
        GetRandomQuestion();
        OnDisplayQuestion();
        scoreKeeper.IncrementQuestionsSeen();
        progressBar.value++;
    }

    private void GetRandomQuestion()
    {
        int RandomIndex = UnityEngine.Random.Range(0, questions.Count);
        currentQeustion = questions[RandomIndex];
        questions.RemoveAt(RandomIndex);
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
        Debug.Log("Displaying question: " + currentQeustion.GetQuestion());
        questionText.text = currentQeustion.GetQuestion();

        for (int i = 0; i < answerButtons.Length; i++)
        {
            //answerTextArr[i].text = question.GetAnswers(i);
            answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = currentQeustion.GetAnswers(i);
        }
    }

    public void OnAnswerButtonClicked(int index)
    {
        chooseAnswer = true;
        DisplaySolution(index);
        timer.CancleTimer();
        SetButtonState(false);
        scoreText.text = $"Score : {scoreKeeper.CalculateScore()}%";
    }

    private void DisplaySolution(int index)
    {
        if (index == currentQeustion.GetCorrectAnswerIndex())
        {
            questionText.text = "뭐지ㅋㅋ 어케 맞혔지ㅋㅋ";
            answerButtons[index].GetComponent<Image>().sprite = correctAnswerSprite;
            scoreKeeper.IncrementCorrectAnswers();
        }
        else
        {
            questionText.text = "땡땡땡!! 빼액!";
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
