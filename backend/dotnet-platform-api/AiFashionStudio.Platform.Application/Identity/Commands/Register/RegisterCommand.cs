using MediatR;

namespace AiFashionStudio.Platform.Application.Identity.Commands.Register;

public record RegisterCommand(string Email, string Password, string FullName, string? Phone) : IRequest;
