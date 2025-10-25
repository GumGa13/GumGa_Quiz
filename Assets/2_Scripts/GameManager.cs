// Assets/2_Scripts/GameManager.cs
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scene Refs")]
    [SerializeField] private Quiz quiz;
    [SerializeField] private EndScreen endScript;

    [Header("Loading UI")]
    [Tooltip("가능하면 인스펙터에 직접 할당")]
    [SerializeField] private GameObject LoadingCanvas;

    [Header("Audio (선택)")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioClip startBgm;
    [SerializeField] private AudioClip quizBgm;
    [SerializeField] private AudioClip endBgm;
    [SerializeField] private AudioSource sfxSource;

    [Header("Topic (시작 씬에서 설정 가능)")]
    [SerializeField] private string selectedTopic = "";

    void Awake()
    {
        if (!Application.isPlaying) return;

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        if (!Application.isPlaying) return;
        PlayBgm(startBgm);
        TryResolveSceneReferences();
        HookChatEvents();
    }

    void OnDestroy()
    {
        if (!Application.isPlaying) return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnhookChatEvents();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryResolveSceneReferences();
        HookChatEvents();

        var sk = UnityEngine.Object.FindFirstObjectByType<ScoreKeeper>(FindObjectsInactive.Include);
        if (sk != null) sk.ResetScore();
    }

    void HookChatEvents()
    {
        var chat = UnityEngine.Object.FindFirstObjectByType<ChatGPTClient>(FindObjectsInactive.Include);
        if (chat == null) return;

        chat.quizGenerateHandler -= OnQuizGenerated;
        chat.quizRequestFinished -= OnQuizRequestFinished;

        chat.quizGenerateHandler += OnQuizGenerated;
        chat.quizRequestFinished += OnQuizRequestFinished;
    }

    void UnhookChatEvents()
    {
        var chat = UnityEngine.Object.FindFirstObjectByType<ChatGPTClient>(FindObjectsInactive.Include);
        if (chat == null) return;
        chat.quizGenerateHandler -= OnQuizGenerated;
        chat.quizRequestFinished -= OnQuizRequestFinished;
    }

    void OnQuizGenerated(System.Collections.Generic.List<QuestionSO> _)
    {
        ShowQuizScene(); // 성공 시 로딩 종료
    }

    void OnQuizRequestFinished(bool ok)
    {
        if (!ok)
        {
            SetLoadingActive(false);
            if (endScript != null)
            {
                endScript.gameObject.SetActive(true);
                if (quiz != null) quiz.gameObject.SetActive(true); // 비활성화 유지 안 함
            }
        }
        else
        {
            ShowQuizScene();
        }
    }

    // ===== 로딩/씬 전환 =====
    public void ShowQuizScene()
    {
        TryResolveSceneReferences();

        if (quiz != null) quiz.gameObject.SetActive(true);
        if (endScript != null) endScript.gameObject.SetActive(false);

        SetLoadingActive(false);
        PlayBgm(quizBgm);
    }

    public void ShowEndScene()
    {
        TryResolveSceneReferences();

        if (quiz != null) quiz.gameObject.SetActive(false);
        if (endScript != null)
        {
            endScript.gameObject.SetActive(true);
            endScript.ShowFinalScore();
        }

        SetLoadingActive(false);
        PlayBgm(endBgm);
    }

    public void ShowLoadingScene()
    {
        TryResolveSceneReferences();

        // 퀴즈 비활성화하지 않음. 오버레이만 켜서 코루틴/이벤트 유지.
        if (endScript != null) endScript.gameObject.SetActive(false);
        SetLoadingActive(true);
    }

    public void OnReplayLevel1()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void RestartGame()
    {
        TryResolveSceneReferences();

        var sk = UnityEngine.Object.FindFirstObjectByType<ScoreKeeper>(FindObjectsInactive.Include);
        if (sk != null) sk.ResetScore();

        if (quiz != null)
        {
            quiz.RestartRound();
            ShowQuizScene();
            return;
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ===== 공용 =====
    public void PlayBgm(AudioClip clip)
    {
        if (bgmSource == null || clip == null) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlaySfx(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void SetTopic(string topic) => selectedTopic = topic ?? "";
    public string GetTopic() => selectedTopic ?? "";

    // ===== 내부 구현 =====
    void TryResolveSceneReferences()
    {
        if (quiz == null) quiz = UnityEngine.Object.FindFirstObjectByType<Quiz>(FindObjectsInactive.Include);
        if (endScript == null) endScript = UnityEngine.Object.FindFirstObjectByType<EndScreen>(FindObjectsInactive.Include);

        if (LoadingCanvas == null)
        {
            var tagged = GameObject.FindGameObjectsWithTag("LoadingCanvas").FirstOrDefault();
            if (tagged != null) LoadingCanvas = tagged;

            if (LoadingCanvas == null)
            {
                var all = Resources.FindObjectsOfTypeAll<GameObject>();
                LoadingCanvas = all.FirstOrDefault(go =>
                    go.name.IndexOf("loading", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    go.GetComponentInParent<Canvas>(true) != null);
            }
        }
    }

    void SetLoadingActive(bool active)
    {
        if (LoadingCanvas != null)
        {
            LoadingCanvas.SetActive(active);
            Debug.Log($"GameManager: LoadingCanvas {(active ? "활성화" : "비활성화")}");
            return;
        }

        // 폴백: 이름 포함 전체 비활성/활성
        var all = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var go in all)
        {
            if (!go) continue;
            if (go.name.IndexOf("loading", StringComparison.OrdinalIgnoreCase) >= 0)
                go.SetActive(active);
        }
        Debug.Log($"GameManager: 로딩 UI {(active ? "활성화" : "비활성화")} (폴백)");
    }
}
