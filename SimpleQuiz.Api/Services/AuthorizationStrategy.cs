using Microsoft.EntityFrameworkCore;
using SimpleQuiz.Api.Abstractions;
using SimpleQuiz.Api.Abstractions.Authorizations;
using SimpleQuiz.Api.Database;

namespace SimpleQuiz.Api.Services;

public class QuizAuthorizationStrategy : IAuthorizationStrategy<Guid>
{
    public async Task<bool> IsAuthorizedToModifyAsync(
        Guid id,
        AppDbContext appDbContext,
        IUserContextService userContextService)
    {
        var userId = userContextService.GetUserId();

        var quiz = await appDbContext
                        .Quizzes
                        .FirstOrDefaultAsync(q => q.QuizId == id && q.UserId == userId);

        return quiz is not null;
    }
}
public class QuestionAuthorizationStrategy : IAuthorizationStrategy<int>
{
    public async Task<bool> IsAuthorizedToModifyAsync(
        int id,
        AppDbContext appDbContext,
        IUserContextService userContextService)
    {
        try
        {
            var userId = userContextService.GetUserId();
            var isAuthorized = await appDbContext.Quizzes
                .GroupJoin(
                    appDbContext.Questions.Where(q => q.QuestionId == id),
                    quiz => quiz.QuizId,
                    question => question.QuizId, 
                    (quiz, questions) => new { quiz.UserId, questions } 
                )
                .SelectMany(
                    x => x.questions.DefaultIfEmpty(), 
                    (x, question) => x.UserId 
                )
                .AnyAsync(id => id == userId);

            return isAuthorized;
        }
        catch (Exception)
        {

            return false;
        }
    }
}
public class AnswerOptionAuthorizationStrategy : IAuthorizationStrategy<int>
{
    public async Task<bool> IsAuthorizedToModifyAsync(
        int id,
        AppDbContext appDbContext,
        IUserContextService userContextService)
    {
        try
        {
            var userId = userContextService.GetUserId();
            var isAuthorized = await appDbContext.Quizzes
                .GroupJoin(
                    appDbContext.Questions,
                    quiz => quiz.QuizId,
                    question => question.QuizId,
                    (quiz, questions) => new { quiz, questions }
                )
                .SelectMany(
                    x => x.questions.DefaultIfEmpty(),
                    (x, question) => new { x.quiz, question }
                )
                .GroupJoin(
                    appDbContext.AnswerOptions.Where(ao => ao.Id == id),
                    temp => temp != null && temp.question != null ? temp.question.QuestionId : default,
                    answerOption => answerOption.QuestionId,
                    (temp, answerOptions) => new { temp.quiz.UserId, answerOptions } 
                )
                .SelectMany(
                    x => x.answerOptions.DefaultIfEmpty(), // Mimics RIGHT JOIN behavior for AnswerOptions
                    (x, answerOption) => x.UserId 
                )
                .AnyAsync(id => id == userId); 


            return isAuthorized;
        }
        catch (Exception)
        {

            return false;
        }
    }
   
}


