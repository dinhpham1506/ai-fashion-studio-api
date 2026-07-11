using AiFashionStudio.Platform.Application.Common.Dtos;
using MediatR;

namespace AiFashionStudio.Platform.Application.Identities.Commands.RefreshAccessToken;

public record RefreshAccessTokenCommand(string RefreshToken) : IRequest<RefreshTokenResult>;
