using TMPro;
using UnityEngine;

public class EndScreen : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI finalScoreText;
    [SerializeField] ScoreKeeper scoreKeeper;

    public void ShowFinalScore()
    {
        finalScoreText.text = "당신이 점수를 높게 받았다 하더라도\r\n" +
            "인생에는 아무런 변화가 없답니다 ㅋㅋㄹㅃㅃ\r\n" +
            "그래도 아량을 베풀어 축하는 해드리죠 쿠쿠!\r\n" +
            $"{scoreKeeper.CalculateScore()}%";
    }
}
