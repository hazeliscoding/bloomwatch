using BloomWatch.SharedKernel.CQRS;

namespace BloomWatch.Modules.Identity.Application.UseCases.RevokeToken;

public sealed record RevokeTokenCommand(string RefreshToken) : ICommand;
