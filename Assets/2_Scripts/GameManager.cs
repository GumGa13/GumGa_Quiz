using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Quiz quiz;
    [SerializeField] private EndScript endScript;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ShowQuizScene();
    }

    private void ShowQuizScene()
    {
        quiz.gameObject.SetActive(true);
        endScript.gameObject.SetActive(false);
    }

    private void ShowEndScene()
    {
        quiz.gameObject.SetActive(false);
        endScript.gameObject.SetActive(true);
        endScript.ShowFinalScore();
    }

    public void OnReplayLevel1()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
