using SimpleQuiz.Api.Abstractions;
using SimpleQuiz.Api.Abstractions.Authorizations;
using SimpleQuiz.Api.Database;

public class AuthorizationService<TId> : IAuthorizationService<TId>
{
    private readonly AppDbContext _appDbContext;
    private readonly IUserContextService _userContextService;
    private readonly Dictionary<Type, IAuthorizationStrategy<TId>> _strategies;

    public AuthorizationService(
        AppDbContext appDbContext,
        IUserContextService userContextService,
        IEnumerable<IAuthorizationStrategy<TId>> strategies)
    {
        _appDbContext = appDbContext;
        _userContextService = userContextService;

        // Dynamically resolve strategies from the DI container
        _strategies = strategies.ToDictionary(
            strategy => strategy.GetType(),
            strategy => strategy);
    }

    public async Task<bool> IsUserAuthorizedToModifyAsync<TEntity>(TId id)
    {
        var entityType = typeof(TEntity);

        if (_strategies.TryGetValue(entityType, out var strategy))
        {
            return await strategy.IsAuthorizedToModifyAsync(id, _appDbContext, _userContextService);
        }

        throw new InvalidOperationException($"No authorization strategy found for type {entityType.Name}");
    }
}
