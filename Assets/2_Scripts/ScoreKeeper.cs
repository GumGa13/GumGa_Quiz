using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{
    // 누적 점수(소수 허용)
    float totalPoints = 0f;
    // 누적 가능한 최대 포인트
    float maxPoints = 0f;

    public int GetCorrectAnswers()
    {
        return Mathf.FloorToInt(totalPoints);
    }

    // 문제 노출 시 호출: 한 문제당 기본 단위 1
    public void IncrementQuestionsSeen()
    {
        maxPoints += 1f;
    }

    public void IncrementQuestionsSeen(int dummy)
    {
        IncrementQuestionsSeen();
    }

    // 정답 시 포인트 추가 (기본 1.0f, 보너스 포함)
    public void IncrementCorrectAnswers(float points = 1f)
    {
        totalPoints += points;
    }

    // 기존 호출 호환성용 오버로드
    public void IncrementCorrectAnswers()
    {
        IncrementCorrectAnswers(1f);
    }

    public int GetQuestionsSeen()
    {
        return Mathf.FloorToInt(maxPoints);
    }

    public int CalculateScore()
    {
        if (maxPoints <= 0f) return 0;
        return Mathf.RoundToInt((totalPoints / maxPoints) * 100f);
    }

    // 재시작 / 씬 로드 시 상태 초기화용 (GameManager에서 호출)
    public void ResetScore()
    {
        totalPoints = 0f;
        maxPoints = 0f;
    }

    // 디버그용: 내부 상태 확인
    public void DebugLogState()
    {
        Debug.Log($"ScoreKeeper - totalPoints: {totalPoints}, maxPoints: {maxPoints}, percent: {CalculateScore()}%");
    }
}
