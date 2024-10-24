using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using SimpleQuiz.Api.Abstractions.Operations;
using SimpleQuiz.Api.Database;
using SimpleQuiz.Api.Entities;
using SimpleQuiz.Api.Shared;
using System.Security.Claims;

namespace SimpleQuiz.Api.Features.Quizzes;

public static class GetAllQuizOfUser
{
    public record Query(Guid id) : IQuery<List<Quiz>>;
    public class Validate : AbstractValidator<Query>
    {
        public Validate()
        {
            RuleFor(q => q.id).NotEmpty();
        }
    }
    internal sealed class Handler : IQueryHandler<Query, List<Quiz>>
    {
        private readonly AppDbContext _appDbContext;
        private readonly IValidator<Query> _validator;

        public Handler(AppDbContext appDbContext, IValidator<Query> validator)
        {
            _appDbContext = appDbContext;
            _validator = validator;
        }

        public async Task<Result<List<Quiz>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                return Result.Failure<List<Quiz>>(new Error(
                    "Quiz.Validation",
                    validationResult.ToString()));
            }

            var result = await _appDbContext.Quizzes
                                            .Where(q => q.UserId == request.id)
                                            .ToListAsync();

            return Result.Success(result);
        }
    }
}

public class GetAllQuizOfUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/quiz/user", async (IMediator mediator, HttpContext httpContext) =>
        {
            // Look for either Sub claim or nameidentifier claim
            var userIdClaim = httpContext.User.Claims.FirstOrDefault(c =>
                c.Type == JwtRegisteredClaimNames.Sub ||
                c.Type == ClaimTypes.NameIdentifier);  // Add this alternative

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                //Console.WriteLine("User ID claim not found or invalid.");
                return Results.Unauthorized();
            }

            //Console.WriteLine($"User ID from token: {userId}");
            var results = await mediator.Send(new GetAllQuizOfUser.Query(userId));
            return results.IsSuccess
                  ? Results.Ok(results.Value)
                  : Results.BadRequest(results.Error);
        })
        .RequireAuthorization()
        .WithTags("Quizzes");
    }
}
