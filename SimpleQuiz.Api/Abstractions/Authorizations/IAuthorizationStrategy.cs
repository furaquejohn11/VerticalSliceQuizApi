using SimpleQuiz.Api.Database;
namespace SimpleQuiz.Api.Abstractions.Authorizations;

public interface IAuthorizationStrategy<TId>
{
    // It is set as generic since Quiz used Guid, Question used int, and AnswerOption used int
    Task<bool> IsAuthorizedToModifyAsync(
        TId id,
        AppDbContext appDbContext,
        IUserContextService userContextService);
}
