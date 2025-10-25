// Assets/2_Scripts/Timer.cs
using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] float problemTime = 10f;
    [SerializeField] float solutionTime = 3f;
    float time = 0f;

    [HideInInspector] public bool isProblemTime;
    [HideInInspector] public float fillAmount;
    [HideInInspector] public bool loadNextQuestion;

    [HideInInspector] public Color uiColor = Color.green;
    [HideInInspector] public int remainingSeconds;

    [Header("색상 임계값")]
    [SerializeField] Color normalColor = Color.green;
    [SerializeField] Color warningColor = new Color(1f, 0.7f, 0f);
    [SerializeField] Color dangerColor = Color.red;

    void Start()
    {
        isProblemTime = true;
        time = problemTime;
        loadNextQuestion = true;
    }

    void Update()
    {
        TimerCountDown();
        UpdateFillAmount();
    }

    void UpdateFillAmount()
    {
        fillAmount = isProblemTime ? Mathf.Clamp01(time / problemTime)
                                   : Mathf.Clamp01(time / solutionTime);
        remainingSeconds = Mathf.CeilToInt(time);

        if (isProblemTime)
        {
            float f = fillAmount;
            if (f > 0.66f) uiColor = normalColor;
            else if (f > 0.33f) uiColor = warningColor;
            else uiColor = dangerColor;
        }
        else uiColor = normalColor;
    }

    void TimerCountDown()
    {
        time -= Time.deltaTime;
        if (time <= 0f)
        {
            if (isProblemTime)
            {
                isProblemTime = false;
                time = solutionTime;
            }
            else
            {
                isProblemTime = true;
                time = problemTime;
                loadNextQuestion = true;
            }
        }
    }

    public void CancleTimer() { time = 0f; }

    public void ResetProblemTimer()
    {
        isProblemTime = true;
        time = problemTime;
        loadNextQuestion = false;
    }
}
