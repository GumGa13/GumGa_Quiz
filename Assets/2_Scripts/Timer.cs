using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] float problemTime = 10f;
    [SerializeField] float solutionTime = 3f;
    float time = 0f;

    [HideInInspector] public bool isProblemTime;
    [HideInInspector] public float fillAmount;
    [HideInInspector] public bool loadNextQuestion;

    private void Start()
    {
        time = problemTime;
        loadNextQuestion = true;
    }

    private void Update()
    {
        TimerCountDown();
        UpdateFillAmount();
    }

    private void UpdateFillAmount()
    {
        if (isProblemTime)
        {
            fillAmount = time / problemTime;
        }
        else
        {
            fillAmount = time / solutionTime;
        }
    }

    private void TimerCountDown()
    {
        Debug.Log("Time remaining: " + time);  
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
            Debug.Log("Timer over");
        }
    }

    public void CancleTimer()
    {
        time = 0f;
    }

}
