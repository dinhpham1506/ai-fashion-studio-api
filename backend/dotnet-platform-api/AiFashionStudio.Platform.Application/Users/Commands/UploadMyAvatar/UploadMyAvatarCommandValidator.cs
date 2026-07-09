using FluentValidation;
using System;
using System.Collections.Generic;

namespace AiFashionStudio.Platform.Application.Users.Commands.UploadMyAvatar
{
    public class UploadMyAvatarCommandValidator : AbstractValidator<UploadMyAvatarCommand>
    {
        // 5MB — đủ cho ảnh đại diện, tránh upload file quá lớn lên MinIO
        private const int MaxFileSizeBytes = 5 * 1024 * 1024;

        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        public UploadMyAvatarCommandValidator()
        {
            RuleFor(command => command.Content)
                .NotEmpty().WithErrorCode("AVATAR_FILE_REQUIRED").WithMessage("Avatar file is required");

            RuleFor(command => command.Content)
                .Must(content => content.Length <= MaxFileSizeBytes)
                .WithErrorCode("AVATAR_FILE_TOO_LARGE").WithMessage("Avatar file must be 5MB or smaller")
                .When(command => command.Content is { Length: > 0 });

            RuleFor(command => command.ContentType)
                .Must(contentType => AllowedContentTypes.Contains(contentType))
                .WithErrorCode("AVATAR_FILE_TYPE_INVALID").WithMessage("Avatar must be a JPEG, PNG or WebP image");
        }
    }
}
