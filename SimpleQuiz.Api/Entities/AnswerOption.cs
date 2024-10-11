using System.Text.Json.Serialization;

namespace SimpleQuiz.Api.Entities;

public class AnswerOption
{
    public AnswerOption(
        int questionId,
        string text,
        bool isCorrect)
    {
        QuestionId = questionId;
        Text = text;
        IsCorrect = isCorrect;
    }
    public int Id { get; private set; }
    public int QuestionId { get; private set; }
    public string Text { get; private set; } = null!;
    public bool IsCorrect { get; private set; }

    [JsonIgnore]
    public virtual Question Question { get; set; } = null!;
}
