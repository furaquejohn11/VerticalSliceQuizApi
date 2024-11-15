namespace SimpleQuiz.Api.Entities;

public class Question
{
    private Question(
        Guid quizId,
        string text,
        string type,
        string correctAnswer
        )
    {
        QuizId = quizId;
        Text = text;
        Type = type;
        CorrectAnswer = correctAnswer;
    }
    public int QuestionId { get; private set; }
    public Guid QuizId { get; private set; }
    public string Text { get; private set; } = null!;
    public string Type { get; private set; } = null!;
    public string CorrectAnswer { get; private set; } = null!;

    public virtual Quiz Quiz { get; set; } = null!;
    public virtual ICollection<AnswerOption> Options { get; set; } = new List<AnswerOption>();



    public static Question Create(Guid quizId, string text, string type, string correctAnswer)
    {
        var question = new Question(quizId,text,type,correctAnswer);

        return question;
    }
    public void Update(string text, string type, string correctAnswer)
    {
        Text = text;
        Type = type;
        CorrectAnswer = correctAnswer;
    }

}
