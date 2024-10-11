using MediatR;
using SimpleQuiz.Api.Shared;

namespace SimpleQuiz.Api.Abstractions.Operations;

public interface IQueryHandler<TResponse> : IRequestHandler<TResponse, Result>
    where TResponse : IQuery
{

}
public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
