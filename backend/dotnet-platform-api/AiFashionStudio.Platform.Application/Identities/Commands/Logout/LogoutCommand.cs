using MediatR;

namespace AiFashionStudio.Platform.Application.Identities.Commands.Logout;

public record LogoutCommand(string RefreshToken) : IRequest;
