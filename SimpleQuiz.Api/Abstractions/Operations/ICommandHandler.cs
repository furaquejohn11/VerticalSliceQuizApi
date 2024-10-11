using MediatR;
using SimpleQuiz.Api.Shared;

namespace SimpleQuiz.Api.Abstractions.Operations;

public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>
        where TCommand : ICommand
{
}
public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}