using AiFashionStudio.Platform.Application.Contents.Commands.UpsertAboutUsSection;
using AiFashionStudio.Platform.Application.Identities.Commands.Login;
using AiFashionStudio.Platform.Application.Identities.Commands.Register;
using AiFashionStudio.Platform.Application.Payments.Commands;
using AiFashionStudio.Platform.Application.Payments.Commands.CreatePayment;
using AiFashionStudio.Platform.Application.Users.Commands.UpdateMyProfile;
using AiFashionStudio.Platform.Application.Users.Commands.UploadMyAvatar;
using Xunit;

namespace AiFashionStudio.Platform.Tests.Validation;

public class FeatureValidatorsTests
{
    [Fact]
    public void RegisterValidator_Should_Reject_Invalid_Email_And_Weak_Password()
    {
        var validator = new RegisterCommandValidator();

        var result = validator.Validate(new RegisterCommand("bad-email", "short", "", null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorCode == "EMAIL_INVALID");
        Assert.Contains(result.Errors, error => error.ErrorCode == "PASSWORD_WEAK");
        Assert.Contains(result.Errors, error => error.ErrorCode == "FULL_NAME_REQUIRED");
    }

    [Fact]
    public void LoginValidator_Should_Reject_Missing_Credentials()
    {
        var validator = new LoginCommandValidator();

        var result = validator.Validate(new LoginCommand("", ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorCode == "EMAIL_REQUIRED");
        Assert.Contains(result.Errors, error => error.ErrorCode == "PASSWORD_REQUIRED");
    }

    [Fact]
    public void CreatePaymentValidator_Should_Reject_Invalid_Amount_And_Long_Description()
    {
        var validator = new CreatePaymentLinkCommandValidator();

        var result = validator.Validate(new CreatePaymentCommand(Guid.NewGuid(), 0, new string('x', 257)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorCode == "AMOUNT_INVALID");
        Assert.Contains(result.Errors, error => error.ErrorCode == "DESCRIPTION_TOO_LONG");
    }

    [Fact]
    public void UpdateProfileValidator_Should_Reject_Empty_FullName()
    {
        var validator = new UpdateMyProfileCommandValidator();

        var result = validator.Validate(new UpdateMyProfileCommand(Guid.NewGuid(), "", "123"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorCode == "FULL_NAME_REQUIRED");
    }

    [Fact]
    public void UploadAvatarValidator_Should_Reject_Empty_File()
    {
        var validator = new UploadMyAvatarCommandValidator();

        var result = validator.Validate(new UploadMyAvatarCommand(Guid.NewGuid(), [], "image/png", "avatar.png"));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpsertAboutUsValidator_Should_Reject_Invalid_Status()
    {
        var validator = new UpsertAboutUsSectionCommandValidator();

        var result = validator.Validate(
            new UpsertAboutUsSectionCommand("INTRO", "Title", "Content", null, "ARCHIVED", Guid.NewGuid()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Status");
    }
}
