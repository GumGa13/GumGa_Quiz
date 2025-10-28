// Assets/2_Scripts/EndScreen.cs
using TMPro;
using UnityEngine;

public class EndScreen : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI finalScoreText;
    [SerializeField] ScoreKeeper scoreKeeper;

    public void ShowFinalScore()
    {
        finalScoreText.text = "결과\n" +
            $"점수: {scoreKeeper.CalculateScore()}%";
    }

    public void OnReplayButton()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RestartGame();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void onReply()
    {
        GameManager.Instance.OnReplayLevel1();
    }
}
