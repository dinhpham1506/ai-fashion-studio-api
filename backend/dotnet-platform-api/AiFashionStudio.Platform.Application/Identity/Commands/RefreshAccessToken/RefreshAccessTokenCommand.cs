using AiFashionStudio.Platform.Application.Common.Dtos;
using MediatR;

namespace AiFashionStudio.Platform.Application.Identity.Commands.RefreshAccessToken;

public record RefreshAccessTokenCommand(string RefreshToken) : IRequest<RefreshTokenResult>;
