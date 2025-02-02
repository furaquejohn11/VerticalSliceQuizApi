using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SimpleQuiz.Api.Abstractions.Operations;
using SimpleQuiz.Api.Database;
using SimpleQuiz.Api.Shared;
using static SimpleQuiz.Api.Features.Questions.CreateQuestion;

namespace SimpleQuiz.Api.Features.Questions;

public static class GetQuestionByQuizId
{
    public record QuestionDto(
        string Text,
        string Type,
        string CorrectAnswer,
        List<AnswerOptionDto> AnswerOptions);

    public record Query(Guid id) : IQuery<List<QuestionDto>>;

    public class Validator : AbstractValidator<Query>
    {
        public Validator() 
        {
            RuleFor(c => c.id).NotEmpty();
        }
    }

    internal sealed class Handler : IQueryHandler<Query,List<QuestionDto>>
    {
        private readonly AppDbContext _appDbContext;
        private readonly IValidator<Query> _validator;

        public Handler(AppDbContext appDbContext, IValidator<Query> validator)
        {
            _appDbContext = appDbContext;
            _validator = validator;
        }

        public async Task<Result<List<QuestionDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                return Result.Failure<List<QuestionDto>>(new Error(
                    "Quiz.Validation",
                    validationResult.ToString()));
            }

            /*
            var result = await _appDbContext.Questions
                        .Where(q => q.QuizId == request.id)
                        .Select(q => new QuestionDto(
                            q.Text,
                            q.Type,
                            q.CorrectAnswer,
                            _appDbContext.AnswerOptions
                                         .Where(a => a.QuestionId == q.QuestionId)
                                         .Select(a => new AnswerOptionDto(
                                            a.Text,
                                            a.IsCorrect
                                            ))
                                         .ToList()
                            ))
                        .ToListAsync();
            
            */
            
            var result = await _appDbContext.Questions
                                            .Include(q => q.Options)
                                            .Where(q => q.QuizId == request.id)
                                            .Select(q => new QuestionDto
                                            (
                                                q.Text,
                                                q.Type,
                                                q.CorrectAnswer,
                                                q.Options.Select(o => new AnswerOptionDto(
                                                    o.Text,
                                                    o.IsCorrect)).ToList()
                                            )).ToListAsync();
            
            return Result.Success(result);

        }
    }
}

public class GetQuestionByQuizIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/question/{id:guid}", 
            async (IMediator mediator, Guid id) =>
        {
            var results = await mediator.Send(new GetQuestionByQuizId.Query(id));

            return results.IsSuccess
                  ? Results.Ok()
                  : Results.BadRequest(results.Error);
        })
        .WithTags("Questions");
    }
}
