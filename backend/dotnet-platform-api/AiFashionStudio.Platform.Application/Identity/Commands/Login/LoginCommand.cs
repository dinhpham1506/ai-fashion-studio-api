using AiFashionStudio.Platform.Application.Common.Dtos;
using MediatR;

namespace AiFashionStudio.Platform.Application.Identity.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<LoginResult>;
