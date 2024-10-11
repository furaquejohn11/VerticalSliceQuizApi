namespace SimpleQuiz.Api.Entities;

public class Quiz
{
    public Quiz(
        Guid quizId,
        Guid userId,
        string title,
        string description,
        Boolean isPublic)
    {
        QuizId = quizId;
        UserId = userId;
        Title = title;
        Description = description;
        IsPublic = isPublic;
    }
    public Guid QuizId { get; private set; }
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public Boolean IsPublic { get; private set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();


    public static Quiz Create(Guid userId, string title, string description, Boolean isPublic)
    {
        var quiz = new Quiz(Guid.NewGuid(), userId, title, description, isPublic);

        return quiz;
    }

    public void Update(string title, string description, Boolean isPublic)
    {
        Title = title;
        Description = description;
        IsPublic = isPublic;
    }

}
