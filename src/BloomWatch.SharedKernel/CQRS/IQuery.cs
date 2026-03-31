using MediatR;

namespace BloomWatch.SharedKernel.CQRS;

/// <summary>
/// Marker interface for queries that return a <typeparamref name="TResponse"/>.
/// </summary>
public interface IQuery<out TResponse> : IRequest<TResponse>;
