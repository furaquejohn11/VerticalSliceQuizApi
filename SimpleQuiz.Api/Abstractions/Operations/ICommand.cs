using MediatR;
using SimpleQuiz.Api.Shared;

namespace SimpleQuiz.Api.Abstractions.Operations;

public interface ICommand : IRequest<Result>
{
}
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{

}
