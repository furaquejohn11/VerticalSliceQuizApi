using Carter;
using MediatR;
using SimpleQuiz.Api.Abstractions;
using SimpleQuiz.Api.Abstractions.Authorizations;
using SimpleQuiz.Api.Abstractions.Operations;
using SimpleQuiz.Api.Entities;
using SimpleQuiz.Api.Services;
using SimpleQuiz.Api.Shared;

namespace SimpleQuiz.Api.Features.AnswerOptions;

public class AddAnswerOption
{
    public record Command(
        int questionId,
        string Text,
        bool isCorrect) : ICommand;

    internal sealed class Handler : ICommandHandler<Command>
    {
        private readonly IRepository<AnswerOption> _answerRepository;
        private readonly IAuthorizationService<int> _authorizationService;

        public Handler(
            IRepository<AnswerOption> answerRepository,
            IAuthorizationService<int> authorizationService)
        {
            _answerRepository = answerRepository;
            _authorizationService = authorizationService;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var isAuthorized = await _authorizationService
                                    .IsUserAuthorizedToModifyAsync
                                    <QuestionAuthorizationStrategy>(request.questionId);

            if (!isAuthorized)
            {
                return Result.Failure(new Error("Authorization.Failure", "User is not authorized to add answer option on this question."));
            }

            var answer = new AnswerOption(
                             request.questionId,
                             request.Text,
                             request.isCorrect);

            await _answerRepository.AddAsync(answer);

            return Result.Success();
        }
    }
}

public class AddAnswerOptionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/answeroption/add", async (IMediator mediator, AddAnswerOption.Command command) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? Results.Ok()
                : Results.BadRequest(result.Error);
        })
        .WithTags("AnswerOptions");
    }
}
