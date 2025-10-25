// Assets/2_Scripts/Quiz.cs
using System;
using System.Collections;
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

    [Header("보기")]
    [SerializeField] GameObject[] answerButtons;

    [Header("버튼 색깔")]
    [SerializeField] Sprite defaultAnswerSprite;
    [SerializeField] Sprite correctAnswerSprite;

    [Header("타이머")]
    [SerializeField] private Image timerImage;
    [SerializeField] private Sprite problemTimerSprite;
    [SerializeField] private Sprite solutionTimerSprite;
    [SerializeField] private Timer timer;
    [SerializeField] TextMeshProUGUI timerText;
    bool chooseAnswer = false;

    [Header("점수")]
    [SerializeField] private ScoreKeeper scoreKeeper;
    [SerializeField] TextMeshProUGUI scoreText;

    [Header("Progress Bar")]
    [SerializeField] Slider progressBar;

    [Header("ChatGPTClient")]
    [SerializeField] ChatGPTClient chatGPTClient;
    [SerializeField] int questionCount = 5;
    [SerializeField] TextMeshProUGUI loadingText;

    [Header("Round 설정")]
    [SerializeField] int questionsPerRound = 5;
    int answeredQuestions = 0;

    bool isGeneratingQuestions = false;
    bool questionsReady = false;

    [Header("재요청/타임아웃")]
    [SerializeField] int maxFetchRetries = 2;
    int currentRetry = 0;
    int pendingNeeded = 0;
    [SerializeField] float requestTimeoutSeconds = 12f;

    string lastRequestedTopic = "";

    void OnEnable()
    {
        BindClients();
    }

    void OnDisable()
    {
        UnbindClients();
        StopAllCoroutines();
    }

    void Start()
    {
        BindClients();

        if (questionsPerRound <= 0) questionsPerRound = 5;
        questionCount = questionsPerRound;

        if (timer == null)
        {
            timer = FindObjectOfType<Timer>();
            if (timer == null) Debug.LogError("Quiz: Timer를 찾을 수 없습니다. 씬에 Timer 오브젝트가 있는지 확인하세요.");
        }

        if (scoreKeeper == null)
        {
            scoreKeeper = FindObjectOfType<ScoreKeeper>();
            if (scoreKeeper == null) Debug.LogError("Quiz: ScoreKeeper를 찾을 수 없습니다. 씬에 ScoreKeeper 오브젝트가 있는지 확인하세요.");
        }

        RestartRound();
    }

    void BindClients()
    {
        if (chatGPTClient == null)
            chatGPTClient = FindObjectOfType<ChatGPTClient>();

        if (chatGPTClient != null)
        {
            chatGPTClient.quizGenerateHandler -= QuizGeneratedHandler;
            chatGPTClient.quizRequestFinished -= QuizRequestFinished;

            chatGPTClient.quizGenerateHandler += QuizGeneratedHandler;
            chatGPTClient.quizRequestFinished += QuizRequestFinished;
        }
    }

    void UnbindClients()
    {
        if (chatGPTClient != null)
        {
            chatGPTClient.quizGenerateHandler -= QuizGeneratedHandler;
            chatGPTClient.quizRequestFinished -= QuizRequestFinished;
        }
    }

    public void RestartRound()
    {
        // 상태 초기화
        StopAllCoroutines();
        isGeneratingQuestions = false;
        questionsReady = false;
        answeredQuestions = 0;
        currentRetry = 0;
        pendingNeeded = 0;
        lastRequestedTopic = "";
        questions.Clear();

        scoreKeeper?.ResetScore();

        if (scoreText != null) scoreText.text = "";
        if (loadingText != null) loadingText.text = "";
        if (progressBar != null) progressBar.value = 0;

        GenerateQuestionsifNeeded();
    }

    private void GenerateQuestionsifNeeded()
    {
        if (isGeneratingQuestions) return;

        isGeneratingQuestions = true;
        questionsReady = false;
        currentRetry = 0;
        pendingNeeded = questionsPerRound;

        if (loadingText != null) loadingText.text = "문제 생성 중...";
        if (GameManager.Instance != null) GameManager.Instance.ShowLoadingScene();

        lastRequestedTopic = (GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.Instance.GetTopic()))
            ? GameManager.Instance.GetTopic()
            : GetTrendingTopic();

        if (chatGPTClient != null)
        {
            questionCount = pendingNeeded;
            chatGPTClient.GenerateQuizQuestions(questionCount, lastRequestedTopic);
            Debug.Log($"GenerateQuestionsifNeeded: 요청 {questionCount}개, 주제: {lastRequestedTopic}");
            // chatGPTClient will invoke quizRequestFinished eventually
        }
        else
        {
            Debug.LogError("Quiz: ChatGPTClient가 할당되지 않았습니다. 폴백으로 문제 생성.");
            CreateFallbackQuestions(questionsPerRound);
            isGeneratingQuestions = false;
            questionsReady = true;
            if (loadingText != null) loadingText.text = "";
            if (GameManager.Instance != null) GameManager.Instance.ShowQuizScene();
            InitializeProgressBar();
            GetNextQuestion();
        }
    }

    private string GetTrendingTopic()
    {
        string[] topics = new[] { "과학", "역사", "음악", "영화", "스포츠", "기술", "문학", "예술", "지리", "음식" };
        int randomindex = UnityEngine.Random.Range(0, topics.Length);
        return topics[randomindex];
    }

    void QuizRequestFinished(bool success)
    {
        // chatGPT 요청 완료 콜백 (성공 여부)
        if (!success)
        {
            Debug.LogWarning("QuizRequestFinished: 문제 생성 실패. 폴백으로 채웁니다.");
            CreateFallbackQuestions(pendingNeeded > 0 ? pendingNeeded : questionsPerRound);
            pendingNeeded = 0;
            isGeneratingQuestions = false;
            questionsReady = true;
            if (loadingText != null) loadingText.text = "";
            if (GameManager.Instance != null) GameManager.Instance.ShowQuizScene();
            InitializeProgressBar();
            GetNextQuestion();
        }
        // 성공 시 실제 문제는 QuizGeneratedHandler에서 처리됨
    }

    void QuizGeneratedHandler(List<QuestionSO> GeneratedQuestions)
    {
        Debug.Log($"QuizGeneratedHandler: 받은 문제 수 = {GeneratedQuestions?.Count ?? 0}");
        StopAllCoroutines(); // 안전하게 타임아웃 등 중단
        isGeneratingQuestions = false;

        int got = GeneratedQuestions?.Count ?? 0;
        if (got > 0)
        {
            questions.Clear();
            // 한 판 개수만 사용
            for (int i = 0; i < Mathf.Min(questionsPerRound, GeneratedQuestions.Count); i++)
                questions.Add(GeneratedQuestions[i]);
        }

        // 부족하면 폴백 채움
        if (questions.Count < questionsPerRound)
        {
            int remain = questionsPerRound - questions.Count;
            Debug.LogWarning($"QuizGeneratedHandler: 부족({questions.Count}/{questionsPerRound}) 폴백 {remain}개 생성");
            CreateFallbackQuestions(remain);
        }

        questionsReady = true;
        answeredQuestions = 0;

        if (loadingText != null) loadingText.text = "";
        if (GameManager.Instance != null) GameManager.Instance.ShowQuizScene();
        else
        {
            // 폴백으로 씬에서 loading 이름 포함 오브젝트 비활성화
            HideLoadingCanvasDirectly();
        }

        InitializeProgressBar();
        GetNextQuestion();
    }

    private void HideLoadingCanvasDirectly()
    {
        try
        {
            var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var root in roots)
                DisableLoadingRecursively(root.transform);
        }
        catch { }
    }

    private void DisableLoadingRecursively(Transform t)
    {
        if (t == null) return;
        if (t.name.IndexOf("loading", StringComparison.OrdinalIgnoreCase) >= 0)
            t.gameObject.SetActive(false);
        for (int i = 0; i < t.childCount; i++)
            DisableLoadingRecursively(t.GetChild(i));
    }

    private void CreateFallbackQuestions(int count)
    {
        if (count <= 0) return;
        for (int i = 0; i < count; i++)
        {
            QuestionSO qs = ScriptableObject.CreateInstance<QuestionSO>();
            qs.hideFlags = HideFlags.None;
            string q = $"Q. 임시문제 {i + 1}";
            string[] answers = new[] { "보기 A", "보기 B", "보기 C", "보기 D" };
            qs.SetData(q, answers, 0);
            questions.Add(qs);
        }
    }

    private void InitializeProgressBar()
    {
        if (progressBar != null)
        {
            progressBar.maxValue = questionsPerRound;
            progressBar.value = answeredQuestions;
        }
    }

    private void Update()
    {
        if (timer == null) return;

        if (timer.isProblemTime)
        {
            if (timerImage != null) timerImage.sprite = problemTimerSprite;
        }
        else
        {
            if (timerImage != null) timerImage.sprite = solutionTimerSprite;
        }

        if (timerImage != null) timerImage.fillAmount = timer.fillAmount;
        if (timerText != null) timerText.text = timer.remainingSeconds.ToString();

        if (timer.loadNextQuestion)
        {
            timer.loadNextQuestion = false;
            if (!questionsReady)
            {
                Debug.Log("Quiz: 질문 준비 중 - 대기");
                return;
            }

            if (answeredQuestions >= questionsPerRound)
            {
                EndRound();
            }
            else
            {
                GetNextQuestion();
            }
        }

        if (timer.isProblemTime == false && chooseAnswer == false && questionsReady)
        {
            DisplaySolution(-1);
        }
    }

    private void GetNextQuestion()
    {
        if (!questionsReady)
        {
            Debug.Log("GetNextQuestion: questionsReady == false, 대기");
            return;
        }

        if (answeredQuestions >= questionsPerRound)
        {
            EndRound();
            return;
        }

        if (questions.Count == 0)
        {
            Debug.Log("GetNextQuestion: 질문 목록 비어있음, 폴백 채움");
            CreateFallbackQuestions(questionsPerRound - answeredQuestions);
            if (questions.Count == 0)
            {
                EndRound();
                return;
            }
        }

        timer?.ResetProblemTimer();
        GameManager.Instance?.ShowQuizScene();

        chooseAnswer = false;
        SetButtonState(true);
        SetDefaultButtonSprites();
        GetRandomQuestion();
        OnDisplayQuestion();

        scoreKeeper?.IncrementQuestionsSeen();
    }

    private void GetRandomQuestion()
    {
        if (questions == null || questions.Count == 0) return;
        int RandomIndex = UnityEngine.Random.Range(0, questions.Count);
        currentQeustion = questions[RandomIndex];
        questions.RemoveAt(RandomIndex);
    }

    private void SetDefaultButtonSprites()
    {
        foreach (var obj in answerButtons)
        {
            if (obj == null) continue;
            var img = obj.GetComponent<Image>();
            if (img != null) img.sprite = defaultAnswerSprite;
        }
    }

    private void OnDisplayQuestion()
    {
        if (currentQeustion == null)
        {
            Debug.LogWarning("OnDisplayQuestion: currentQeustion이 null입니다.");
            return;
        }

        questionText.text = currentQeustion.GetQuestion();
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] == null) continue;
            var txt = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = currentQeustion.GetAnswers(i);
        }
    }

    public void OnAnswerButtonClicked(int index)
    {
        chooseAnswer = true;

        if (timer == null || currentQeustion == null)
        {
            Debug.LogWarning("OnAnswerButtonClicked: timer 또는 currentQeustion이 null입니다.");
            return;
        }

        int bonus = 0;
        float frac = timer.fillAmount;
        if (frac >= 0.66f) bonus = 2;
        else if (frac >= 0.33f) bonus = 1;

        if (index == currentQeustion.GetCorrectAnswerIndex())
        {
            scoreKeeper?.IncrementCorrectAnswers(1f + bonus);
            var img = answerButtons[index].GetComponent<Image>();
            if (img != null) img.sprite = correctAnswerSprite;
        }

        DisplaySolution(index);

        timer?.CancleTimer();
        SetButtonState(false);

        answeredQuestions++;
        if (progressBar != null) progressBar.value = answeredQuestions;

        if (answeredQuestions >= questionsPerRound) EndRound();
        else Invoke(nameof(GetNextQuestion), 0.3f);

        if (scoreText != null && scoreKeeper != null) scoreText.text = $"Score : {scoreKeeper.CalculateScore()}%";
    }

    private void DisplaySolution(int index)
    {
        if (currentQeustion == null)
        {
            questionText.text = "";
            return;
        }

        if (index == currentQeustion.GetCorrectAnswerIndex()) questionText.text = "정답!";
        else questionText.text = $"오답! 정답: {currentQeustion.GetCorrectAnswer()}";

        SetButtonState(false);
    }

    private void SetButtonState(bool state)
    {
        foreach (var obj in answerButtons)
        {
            if (obj == null) continue;
            var btn = obj.GetComponent<Button>();
            if (btn != null) btn.interactable = state;
        }
    }

    private void EndRound()
    {
        Debug.Log("한 판 종료: 결과 화면으로 이동");
        isGeneratingQuestions = false;
        questionsReady = false;
        GameManager.Instance?.ShowEndScene();
    }
}
