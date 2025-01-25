using Carter;
using MediatR;
using SimpleQuiz.Api.Abstractions;
using SimpleQuiz.Api.Abstractions.Operations;
using SimpleQuiz.Api.Entities;
using SimpleQuiz.Api.Shared;

namespace SimpleQuiz.Api.Features.Quizzes;

public static class GetQuizById
{
    public record Query(Guid Id) : IQuery<Quiz>;

    internal sealed class Handler : IQueryHandler<Query, Quiz>
    {
        private readonly IRepository<Quiz> _quizRepository;
        public Handler(IRepository<Quiz> quizRepository)
        {
            _quizRepository = quizRepository;
        }
        public async Task<Result<Quiz>> Handle(Query request, CancellationToken cancellationToken)
        {
            var quiz = await _quizRepository.GetByIdAsync(request.Id);
            if (quiz is null)
            {
                return Result.Failure<Quiz>(new Error("Quiz.NotFound", "Quiz not found"));
            }
            return Result.Success(quiz);
        }

    }
}
public class GetQuizByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/quiz/{id:guid}",
            async (IMediator mediator, Guid id) =>
            {
                var results = await mediator.Send(new GetQuizById.Query(id));

                return results.IsSuccess
                      ? Result.Success(results.Value)
                      : Result.Failure(results.Error);
            })
        .WithTags("Quizzes");
    }
}
