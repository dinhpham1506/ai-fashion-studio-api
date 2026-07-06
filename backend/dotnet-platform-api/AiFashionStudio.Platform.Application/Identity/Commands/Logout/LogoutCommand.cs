using MediatR;

namespace AiFashionStudio.Platform.Application.Identity.Commands.Logout;

public record LogoutCommand(string RefreshToken) : IRequest;
