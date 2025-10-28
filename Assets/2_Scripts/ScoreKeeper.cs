using UnityEngine;
using TMPro;

public class ScoreKeeper : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("라운드 설정")]
    [SerializeField] private int questionsPerRound = 5; // 총 문제 수
    [SerializeField] private int maxScore = 100;        // 만점

    private int currentScore = 0;
    private int questionsSeen = 0;

    void Start()
    {
        UpdateScoreText();
    }

    // 문제 본 횟수 누적
    public void IncrementQuestionsSeen()
    {
        questionsSeen++;
    }

    // 정답 시 점수 증가
    public void IncrementCorrectAnswers(float _ = 1f)
    {
        int perQuestion = Mathf.RoundToInt((float)maxScore / Mathf.Max(1, questionsPerRound));
        currentScore = Mathf.Min(maxScore, currentScore + perQuestion);
        UpdateScoreText();
    }

    // 파라미터 없는 오버로드 유지
    public void IncrementCorrectAnswers()
    {
        IncrementCorrectAnswers(1f);
    }

    // 현재 점수(0~100)
    public int CalculateScore()
    {
        return Mathf.Clamp(currentScore, 0, maxScore);
    }

    // 점수 초기화
    public void ResetScore()
    {
        currentScore = 0;
        questionsSeen = 0;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {CalculateScore()}점";
    }

    // 외부에서 총 문제 수 지정 가능
    public void SetQuestionsPerRound(int count)
    {
        questionsPerRound = Mathf.Max(1, count);
        UpdateScoreText();
    }

    public void DebugLogState()
    {
        Debug.Log($"ScoreKeeper - currentScore: {currentScore}, questionsSeen: {questionsSeen}, score: {CalculateScore()}점");
    }
}
