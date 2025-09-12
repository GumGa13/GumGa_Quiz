using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ChatGPTClient;

public class ChatGPTClient : MonoBehaviour
{
    public delegate void QuizGeneratedHandler(List<QuestionSO>questions);
    public event QuizGeneratedHandler quizGenerateHandler;
    public void GenerateQuestions(int questionCount, string topicToUse)
    {
        Debug.Log($"Generating {questionCount} questions on the topic {topicToUse}");

        StartCoroutine(GenerateWithDelay());
    }

    private IEnumerator GenerateWithDelay()
    {
        yield return new WaitForSeconds(3f);
        List<QuestionSO> questions = new List<QuestionSO>();
        QuestionSO so1 = CreateQuestion("ChatGPT 생성질문1", new string[] { "답변1(정답)", "답변2", "답변3", "답변4" }, 0);
        questions.Add(so1);
        QuestionSO so2 = CreateQuestion("ChatGPT 생성질문1", new string[] { "답변1", "답변2(정답)", "답변3", "답변4" }, 1);
        questions.Add(so2);
        QuestionSO so3 = CreateQuestion("ChatGPT 생성질문1", new string[] { "답변1", "답변2", "답변3(정답)", "답변4" }, 2);
        questions.Add(so3);

        quizGenerateHandler?.Invoke(questions);
        Debug.Log("Finished generating questions.");
    }

    QuestionSO CreateQuestion(string q, string[] a, int correctIndex)
    {
        QuestionSO SO = ScriptableObject.CreateInstance<QuestionSO>();
        SO.SetData(q, a, correctIndex);

        return SO;
    }
}
