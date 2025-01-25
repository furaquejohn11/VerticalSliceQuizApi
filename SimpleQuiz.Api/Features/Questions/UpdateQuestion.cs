using Carter;
using FluentValidation;
using MediatR;
using SimpleQuiz.Api.Abstractions;
using SimpleQuiz.Api.Abstractions.Operations;
using SimpleQuiz.Api.Entities;
using SimpleQuiz.Api.Shared;
using static SimpleQuiz.Api.Features.Questions.CreateQuestion;

namespace SimpleQuiz.Api.Features.Questions;

public static class UpdateQuestion
{
    public record Command(
        int Id,
        string Text,
        string Type,
        string CorrectAnswer,
        List<AnswerOptionDto> AnswerOptions) : ICommand;

    public class Validator : AbstractValidator<Command>
    {
        public Validator() 
        {
            RuleFor(c => c.Id).NotEmpty();
        }

    }

    internal sealed class Handler : ICommandHandler<Command>
    {
        private readonly IRepository<Question> _questionRepository;
        private readonly IValidator<Command> _validator;

        public Handler(
            IRepository<Question> questionRepository,
            IValidator<Command> validator)
        {
            _questionRepository = questionRepository;
            _validator = validator;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                return Result.Failure(new Error(
                    "Question.Validation",
                    validationResult.ToString()));
            }

            var question = await _questionRepository.GetByIdAsync(request.Id);
            if (question is null)
            {
                return Result.Failure(new Error("Question.NotFound", "Question not found"));
            }

            question.Update(
                request.Text,
                request.Type,
                request.CorrectAnswer
            );

            await _questionRepository.UpdateAsync(question);


            return Result.Success();
        }
    }
}

public class UpdateQuestionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/question/{id}", async (int id, IMediator mediator, UpdateQuestion.Command command) =>
        {
            if (id != command.Id)
            {
                return Results.BadRequest("Id in route must match Id in body");
            }

            var result = await mediator.Send(command);
            return result.IsSuccess
                   ? Results.Ok()
                   : Results.BadRequest(result.Error);
        })
        .WithTags("Questions")
        .WithName("UpdateQuestion");
    }
}

