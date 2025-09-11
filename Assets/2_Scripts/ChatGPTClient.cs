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

        StartCoroutine((IEnumerator)GenerateWithDelay());
    }

    private IEnumerable GenerateWithDelay()
    {
        yield return new WaitForSeconds(3f);
        quizGenerateHandler?.Invoke(new List<QuestionSO>());
        Debug.Log("Finished generating questions.");
    }
}
