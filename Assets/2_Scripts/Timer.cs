using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] float problemTime = 10f;
    [SerializeField] float solutionTime = 3f;
    float time = 0f;

    [HideInInspector] bool isProblemTime = true;
    [HideInInspector] public float fillAmount;

    private void Start()
    {
        time = problemTime;
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
            }
            Debug.Log("Time Over");
        }
    }

}
