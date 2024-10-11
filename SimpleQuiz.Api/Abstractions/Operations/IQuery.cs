using MediatR;
using SimpleQuiz.Api.Shared;

namespace SimpleQuiz.Api.Abstractions.Operations;

public interface IQuery : IRequest<Result>
{
}
public interface IQuery<TQuery> : IRequest<Result<TQuery>>
{

}
