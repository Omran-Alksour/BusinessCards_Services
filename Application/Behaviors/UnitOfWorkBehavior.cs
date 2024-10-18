using Application.Abstractions.Messaging;
using Domain.Abstractions;
using MediatR;
using System.Transactions;

namespace Application.Behaviors;

public sealed class UnitOfWorkBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly IUnitOfWork _unitOfWork;

    public UnitOfWorkBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ICommand && !request.GetType().GetInterfaces().Any(x =>
                x.IsGenericType &&
                x.GetGenericTypeDefinition() == typeof(ICommand<>))) //only handle for commands
        {
            return await next();
        }
        using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);
        TResponse? response = await next();
        _ = await _unitOfWork.SaveChangesAsync(cancellationToken);
        transactionScope.Complete();
        return response;
    }
}