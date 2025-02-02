using Carter;
using MediatR;
using SimpleQuiz.Api.Abstractions;
using SimpleQuiz.Api.Abstractions.Authorizations;
using SimpleQuiz.Api.Abstractions.Operations;
using SimpleQuiz.Api.Entities;
using SimpleQuiz.Api.Services;
using SimpleQuiz.Api.Shared;

namespace SimpleQuiz.Api.Features.AnswerOptions;

public static class DeleteAnswerOption
{
    public record Command(int id) : ICommand;

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

            var answer = await _answerRepository.GetByIdAsync(request.id);

            if (answer is null)
            {
                return Result.Failure(new Error("Invalid answer option id","No answer id has found"));
            }

            var isAuthorized = await _authorizationService
                                   .IsUserAuthorizedToModifyAsync
                                   <QuestionAuthorizationStrategy>(answer.QuestionId);

            if (!isAuthorized)
            {
                return Result.Failure(new Error("Authorization.Failure", "User is not authorized to delete answer option on this question."));
            }

            await _answerRepository.DeleteAsync(answer);

            return Result.Success();
        }
    }
}

public class DeleteAnswerOptionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/answeroption/delete", async (IMediator mediator, int id) =>
        {
            var result = await mediator.Send(new DeleteAnswerOption.Command(id));

            return result.IsSuccess
                   ? Results.Ok()
                   : Results.BadRequest(result.Error);
        })
        .WithTags("AnswerOptions");
    }
}
