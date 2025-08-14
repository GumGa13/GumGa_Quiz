using UnityEngine;

[CreateAssetMenu(fileName = "New Question", menuName = "Quiz/Question")]

public class Question_SO : ScriptableObject
{
    [TextArea(2, 6)]
    [SerializeField] string questionText = "¿©±â¿¡ Áú¹®À» À¡À¡";
}
