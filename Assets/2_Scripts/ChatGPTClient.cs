// Assets/2_Scripts/ChatGPTClient.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable] public class ChatGPTRequest { public string model = "gpt-4.1-nano"; public Message[] messages; public float temperature = 1.0f; public int max_completion_tokens = 800; }
[Serializable] public class Message { public string role; public string content; }
[Serializable] public class ChatGPTResponse { public Choice[] choices; }
[Serializable] public class Choice { public Message message; }
[Serializable] public class QuizData { public QuizQuestion[] questions; }
[Serializable] public class QuizQuestion { public string question; public string[] answers; public int correctAnswerIndex; }

public class ChatGPTClient : MonoBehaviour
{
    private const string API_URL = "https://api.openai.com/v1/chat/completions";
    private string apiKey;

    public delegate void QuizGenerateHandler(List<QuestionSO> questions);
    public event QuizGenerateHandler quizGenerateHandler;

    public event Action<bool> quizRequestFinished; // true=성공, false=실패

    void Awake()
    {
        apiKey = LoadFromResources();
        if (string.IsNullOrWhiteSpace(apiKey))
            Debug.LogWarning("OpenAI API Key 가 비어있음. Assets/Resources/config 에 'OPENAI_API_KEY=...' 추가.");
    }

    string LoadFromResources()
    {
        try
        {
            TextAsset cfg = Resources.Load<TextAsset>("config");
            if (cfg != null)
            {
                foreach (var line in cfg.text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var t = line.Trim();
                    if (t.StartsWith("OPENAI_API_KEY="))
                        return t.Substring("OPENAI_API_KEY=".Length).Trim();
                }
            }
        }
        catch (Exception e) { Debug.LogError($"config 로드 실패: {e.Message}"); }
        return "";
    }

    public void GenerateQuizQuestions(int count = 3, string topic = "일반상식")
    {
        StartCoroutine(RequestQuizQuestions(count, topic));
    }

    IEnumerator RequestQuizQuestions(int count, string topic)
    {
        string prompt =
$@"다음 조건에 맞는 객관식 퀴즈를 {count}개 생성:
주제: {topic}
조건:
- 문제와 보기는 20자 이내
- 문제 앞에 'Q. ' 접두사
- 4지선다, 중복 보기/정답 금지
- 상식 퀴즈만, '예술'·'문학' 제외
- 문제 내에 정답 노출 금지
- 정답은 0~3 인덱스
- 응답은 아래 JSON만:
{{
  ""questions"": [
    {{
      ""question"": ""문제 내용"",
      ""answers"": [""선택지1"", ""선택지2"", ""선택지3"", ""선택지4""],
      ""correctAnswerIndex"": 0
    }}
  ]
}}";

        var req = new ChatGPTRequest { messages = new[] { new Message { role = "user", content = prompt } } };
        var json = JsonUtility.ToJson(req);

        using (var www = new UnityWebRequest(API_URL, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            yield return www.SendWebRequest();

            bool ok = false;
            try
            {
                if (www.result == UnityWebRequest.Result.Success)
                {
                    string raw = www.downloadHandler.text;
                    var res = SafeFromJson<ChatGPTResponse>(raw);

                    string content = null;
                    if (res?.choices != null && res.choices.Length > 0)
                        content = res.choices[0]?.message?.content;

                    if (string.IsNullOrWhiteSpace(content))
                        content = ExtractContentFromRawResponse(raw);

                    if (string.IsNullOrWhiteSpace(content))
                        throw new Exception("content 없음");

                    content = TrimCodeFence(content);

                    var qd = SafeFromJson<QuizData>(content);
                    if (qd == null || qd.questions == null || qd.questions.Length == 0)
                        throw new Exception("QuizData 파싱 실패");

                    var list = CreateQuestionSOs(qd.questions);
                    quizGenerateHandler?.Invoke(list);
                    ok = true;
                }
                else
                {
                    Debug.LogError($"ChatGPT 요청 실패: {www.error} code={www.responseCode}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"응답 파싱 오류: {e.Message}");
                Debug.LogError($"raw: {www.downloadHandler.text}");
            }
            finally
            {
                quizRequestFinished?.Invoke(ok);
            }
        }
    }

    static T SafeFromJson<T>(string s) where T : class
    {
        try { return JsonUtility.FromJson<T>(s); } catch { return null; }
    }

    static string TrimCodeFence(string s)
    {
        var t = s.Trim();
        if (t.StartsWith("```json")) t = t.Substring(7);
        if (t.EndsWith("```")) t = t.Substring(0, t.Length - 3);
        return t.Trim();
    }

    string ExtractContentFromRawResponse(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return null;
        int idx = raw.IndexOf("\"content\"");
        if (idx < 0) return null;
        int colon = raw.IndexOf(':', idx);
        if (colon < 0) return null;
        int startQuote = raw.IndexOf('"', colon + 1);
        if (startQuote < 0) return null;
        startQuote++;

        var sb = new StringBuilder();
        bool esc = false;
        for (int i = startQuote; i < raw.Length; i++)
        {
            char c = raw[i];
            if (esc)
            {
                switch (c)
                {
                    case '"': sb.Append('"'); break;
                    case '\\': sb.Append('\\'); break;
                    case '/': sb.Append('/'); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case 'n': sb.Append('\n'); break;
                    case 'r': sb.Append('\r'); break;
                    case 't': sb.Append('\t'); break;
                    case 'u':
                        if (i + 4 < raw.Length)
                        {
                            string hex = raw.Substring(i + 1, 4);
                            if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int code))
                            { sb.Append((char)code); i += 4; }
                        }
                        break;
                    default: sb.Append(c); break;
                }
                esc = false; continue;
            }
            if (c == '\\') { esc = true; continue; }
            if (c == '"') return sb.ToString();
            sb.Append(c);
        }
        return null;
    }

    List<QuestionSO> CreateQuestionSOs(QuizQuestion[] src)
    {
        var list = new List<QuestionSO>();
        foreach (var q in src)
        {
            var so = ScriptableObject.CreateInstance<QuestionSO>();
            so.hideFlags = HideFlags.None;

            var answers = q.answers != null && q.answers.Length == 4
                ? q.answers
                : new[] { "보기1", "보기2", "보기3", "보기4" };

            int idx = Mathf.Clamp(q.correctAnswerIndex, 0, 3);
            so.SetData(q.question ?? "Q. 빈 문제", answers, idx);
            list.Add(so);
        }
        return list;
    }

    public void SetApiKey(string key)
    {
        apiKey = key;
        PlayerPrefs.SetString("OpenAI_API_Key", key);
        PlayerPrefs.Save();
    }
}
