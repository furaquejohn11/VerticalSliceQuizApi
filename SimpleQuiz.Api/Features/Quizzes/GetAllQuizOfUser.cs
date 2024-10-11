using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SimpleQuiz.Api.Abstractions.Operations;
using SimpleQuiz.Api.Database;
using SimpleQuiz.Api.Entities;
using SimpleQuiz.Api.Shared;
using static SimpleQuiz.Api.Features.Questions.GetQuestionByQuizId;

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
        app.MapGet("api/quiz/user/{id:guid}", async (IMediator mediator, Guid id) =>
        {
            var results = await mediator.Send(new GetAllQuizOfUser.Query(id));

            return results.IsSuccess
                  ? Result.Success(results.Value)
                  : Result.Failure(results.Error);
        })
        .WithTags("Quizzes");
    }
}
