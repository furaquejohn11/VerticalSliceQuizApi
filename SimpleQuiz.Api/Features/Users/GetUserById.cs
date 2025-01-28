using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SimpleQuiz.Api.Abstractions.Operations;
using SimpleQuiz.Api.Database;
using SimpleQuiz.Api.Shared;

namespace SimpleQuiz.Api.Features.Users;

public static class GetUserById
{
    public class UserResponse
    {
        public string FirstName { get; set; } = string.Empty!;
        public string LastName { get; set; } = string.Empty!;
    }

    public record Query(Guid id) : IQuery<UserResponse>;

    internal sealed class Handler : IQueryHandler<Query, UserResponse>
    {
        private readonly AppDbContext _appDbContext;

        public Handler(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<Result<UserResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var user = await _appDbContext.Users
                .Where(u => u.Id == request.id)
                .Select(u => new
                {
                    u.FirstName,
                    u.LastName
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                return Result.Failure<UserResponse>
                    (new Error("User Not Found", "Invalid Credentials"));
            }

            var result = new UserResponse()
            {
                FirstName = user.FirstName,
                LastName = user.LastName
            };

            return Result.Success(result);
        }
    }
}

public class GetUserByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/users/{id:guid}", async (IMediator mediator, Guid id) =>
        {
            var result = await mediator.Send(new GetUserById.Query(id));

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);

        })
        .WithTags("Users")
        .RequireAuthorization();
    }
}
