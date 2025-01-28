namespace SimpleQuiz.Api.Abstractions.Authorizations;

public interface IAuthorizationService<TId>
{
    Task<bool> IsUserAuthorizedToModifyAsync<TEntity>(TId id);
}
