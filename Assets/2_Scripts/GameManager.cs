using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Quiz quiz;
    [SerializeField] private EndScreen endScript;
    [SerializeField] private GameObject LoadingCanvas;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        //ShowQuizScene();
    }

    public void ShowQuizScene()
    {
        quiz.gameObject.SetActive(true);
        endScript.gameObject.SetActive(false);
        LoadingCanvas.SetActive(false);
    }

    public void ShowEndScene()
    {
        quiz.gameObject.SetActive(false);
        endScript.gameObject.SetActive(true);
        endScript.ShowFinalScore();
        LoadingCanvas.SetActive(false);
    }

    public void ShowLoadingScene()
    {
        quiz.gameObject.SetActive(false);
        endScript.gameObject.SetActive(false);
        LoadingCanvas.SetActive(true);
    }

    public void OnReplayLevel1()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
