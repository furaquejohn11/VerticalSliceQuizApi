using Carter;
using MediatR;
using SimpleQuiz.Api.Abstractions;
using SimpleQuiz.Api.Abstractions.Authorizations;
using SimpleQuiz.Api.Abstractions.Operations;
using SimpleQuiz.Api.Entities;
using SimpleQuiz.Api.Services;
using SimpleQuiz.Api.Shared;

namespace SimpleQuiz.Api.Features.Questions;

public class DeleteQuestion
{
    public record Command(int id) : ICommand;

    internal sealed class Handler : ICommandHandler<Command>
    {
        private readonly IRepository<Question> _questionsRepository;
        private readonly IAuthorizationService<int> _authorizationService;

        public Handler(
            IRepository<Question> questionsRepository,
            IAuthorizationService<int> authorizationService)
        {
            _questionsRepository = questionsRepository;
            _authorizationService = authorizationService;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var isAuthorized = await _authorizationService
                                    .IsUserAuthorizedToModifyAsync<QuestionAuthorizationStrategy>(request.id);

            if (!isAuthorized)
            {
                return Result.Failure(new Error("Authorization.Failure", "User is not authorized to delete a question on this quiz."));
            }

            var question = await _questionsRepository.GetByIdAsync(request.id);

            if (question is null)
            {
                return Result.Failure(new Error("No Question Found","Question not found"));
            }

            await _questionsRepository.DeleteAsync(question);

            return Result.Success();
        }
    }
}

public class DeleteAnswerOptionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/question/delete", async (IMediator mediator, int id) =>
        {
            var result = await mediator.Send(new DeleteQuestion.Command(id));

            return result.IsSuccess
                   ? Results.Ok()
                   : Results.BadRequest(result.Error);
        })
        .WithTags("Questions");
    }
}
