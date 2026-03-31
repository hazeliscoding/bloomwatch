using MediatR;

namespace BloomWatch.SharedKernel.CQRS;

/// <summary>
/// Marker interface for commands that do not return a value.
/// </summary>
public interface ICommand : IRequest;

/// <summary>
/// Marker interface for commands that return a <typeparamref name="TResponse"/>.
/// </summary>
public interface ICommand<out TResponse> : IRequest<TResponse>;
