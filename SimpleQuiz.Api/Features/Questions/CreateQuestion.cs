using Carter;
using FluentValidation;
using MediatR;
using SimpleQuiz.Api.Abstractions.Operations;
using SimpleQuiz.Api.Database;
using SimpleQuiz.Api.Entities;
using SimpleQuiz.Api.Shared;

namespace SimpleQuiz.Api.Features.Questions;

public static class CreateQuestion
{
    public record AnswerOptionDto(string Text, bool IsCorrect);

    public record Command(
        Guid QuizId,
        string Text,
        string Type,
        string CorrectAnswer,
        List<AnswerOptionDto> AnswerOptions) : ICommand;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.QuizId).NotEmpty();
            RuleFor(c => c.Text).NotEmpty();
            RuleFor(c => c.Type).NotEmpty();
            RuleFor(c => c.CorrectAnswer).NotEmpty();
        }
    }
    internal sealed class Handler : ICommandHandler<Command>
    {
        private readonly AppDbContext _appDbContext;
        private readonly IValidator<Command> _validator;

        public Handler(AppDbContext appDbContext, IValidator<Command> validator)
        {
            _appDbContext = appDbContext;
            _validator = validator;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                return Result.Failure<string>(new Error(
                    "Login.Validation",
                    validationResult.ToString()));
            }

            var question = new Question(
                request.QuizId,
                request.Text,
                request.Type,
                request.CorrectAnswer);

            await _appDbContext.Questions.AddAsync(question);
            await _appDbContext.SaveChangesAsync();

            var answerOptions = request.AnswerOptions
                                .Select(a => new AnswerOption(
                                    question.QuestionId,
                                    a.Text,
                                    a.IsCorrect));

            
            await _appDbContext.AnswerOptions.AddRangeAsync(answerOptions);      
            await _appDbContext.SaveChangesAsync();

            return Result.Success(question);
        }
    }
    
}

public class CreateQuestionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/question/create", async (IMediator mediator, CreateQuestion.Command command) =>
        {
            var results = await mediator.Send(command);

            return results.IsSuccess
                   ? Result.Success()
                   : Result.Failure(results.Error);

        })
        .WithTags("Questions");
        //.RequireAuthorization();
    }
}
