using Carter;
using FluentValidation;
using MediatR;
using SimpleQuiz.Api.Abstractions;
using SimpleQuiz.Api.Abstractions.Authorizations;
using SimpleQuiz.Api.Abstractions.Operations;
using SimpleQuiz.Api.Entities;
using SimpleQuiz.Api.Services;
using SimpleQuiz.Api.Shared;

namespace SimpleQuiz.Api.Features.Quizzes;

public static class UpdateQuiz
{
    public record Command(
        Guid QuizId,
        string Title,
        string Description,
        bool IsPublic
        ) : ICommand;

    public class Validator : AbstractValidator<Command>
    {
        public Validator() 
        {
            RuleFor(c => c.QuizId).NotEmpty();
            RuleFor(c => c.Title).NotEmpty().MaximumLength(100);
            RuleFor(c => c.Description).MaximumLength(500);
        }
    }
    internal sealed class Handler : ICommandHandler<Command>
    {
        private readonly IRepository<Quiz> _quizRepository;
        private readonly IValidator<Command> _validator;
        private readonly IAuthorizationService<Guid> _authorizationService;

        public Handler(
            IRepository<Quiz> quizRepository, 
            IValidator<Command> validator,
            IAuthorizationService<Guid> authorizationService)
        {
            _quizRepository = quizRepository;
            _validator = validator;
            _authorizationService = authorizationService;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                return Result.Failure<string>(new Error(
                    "Quiz.Validation",
                    validationResult.ToString()));
            }

            var isAuthorized = await _authorizationService
                                    .IsUserAuthorizedToModifyAsync<QuizAuthorizationStrategy>(request.QuizId);
            if (!isAuthorized)
            {
                return Result.Failure(new Error("Authorization.Failure", "User is not authorized to update this quiz."));
            }

            var quiz = await _quizRepository.FindAsync(q => q.QuizId == request.QuizId);

            if (quiz is null)
            {
                return Result.Failure(new Error(
                    "Quiz.NotFound",
                    $"Quiz with ID {request.QuizId} was not found."));
            }

            quiz.Update(
                request.Title,
                request.Description,
                request.IsPublic);


            await _quizRepository.UpdateAsync(quiz);
            return Result.Success();
        }
    }
}

public class UpdateQuizEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/quiz/update", async (IMediator mediator, UpdateQuiz.Command command) =>
        {
            var result = await mediator.Send(command);

            return result.IsSuccess
                   ? Results.Ok()
                   : Results.BadRequest(result.Error);

        })
        .WithTags("Quizzes");
    }
}
