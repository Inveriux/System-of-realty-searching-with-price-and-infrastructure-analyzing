using BasicBuildingBlocks.BusinessResults.Extensoins;
using BasicBuildingBlocks.Services.Abstractions;
using Microsoft.AspNetCore.Authentication.Google;
using BasicBuildingBlocks.Endpoints.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using UserService.UseCases.Users;
using Microsoft.AspNetCore.Mvc;
using UserService.Models.Enums;
using UserService.Services.Images.Abstractions;
using UserService.Filters;
using UserService.DTOs;
using MediatR;
using System.Text.Json;

namespace UserService.Endpoints;

public class UserEndpoints : IEndpointRegistrar
{
    private static readonly JsonSerializerOptions UserJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var group = application.MapGroup("/api/users")
            .WithTags("Users");
        var agentsGroup = application.MapGroup("/api/agents")
            .WithTags("Agents");

        group.MapGet("/info/{email}", GetUserInfoByEmailAsync)
            .WithName("GetUserInfoByEmail")
            .WithSummary("Gets user information by email.")
            .WithDescription("Requires an authenticated user and passes the owner-or-admin access filter before returning the profile data.")
            .AddEndpointFilter<OwnerOrAdminAccessEndpointFilter>()
            .RequireAuthorization(PolicyNames.OwnerOrAdminAccess);

        group.MapGet("/info", GetUserOwnInfoAsync)
            .WithName("GetCurrentUserInfo")
            .WithSummary("Gets the current authenticated user's profile.")
            .WithDescription("Returns the profile of the user identified by the access token.")
            .AddEndpointFilter<OwnerOrAdminAccessEndpointFilter>()
            .RequireAuthorization();

        group.MapPost("/signup", SignUpAsync)
            .WithName("SignUp")
            .WithSummary("Registers a new local user.")
            .WithDescription("Creates a user with email and password authentication.");
        group.MapPost("/signup-with-avatar", SignUpWithAvatarAsync)
            .WithName("SignUpWithAvatar")
            .WithSummary("Registers a new local user with avatar upload.")
            .WithDescription("Accepts multipart/form-data with `user` JSON and one avatar file in `image`.")
            .Accepts<SignUpWithAvatarRequest>("multipart/form-data")
            .DisableAntiforgery();
        group.MapPost("/email-verification/verify", VerifyEmailAsync)
            .WithName("VerifyEmail")
            .WithSummary("Verifies a local user email with a six-digit code.")
            .WithDescription("Confirms the email after registration using a short-lived verification code.");
        group.MapPost("/email-verification/resend", ResendEmailVerificationCodeAsync)
            .WithName("ResendEmailVerificationCode")
            .WithSummary("Resends the email verification code.")
            .WithDescription("Resends a fresh verification code for an existing unverified local account.");
        group.MapPost("/signin", SignInAsync)
            .WithName("SignIn")
            .WithSummary("Signs in a local user.")
            .WithDescription("Validates credentials and returns an access token with a refresh token.");
        
        group.MapPost("/google/signup", SignUpByGoogleAsync)
            .WithName("GoogleSignUpStart")
            .WithSummary("Starts Google sign-up.")
            .WithDescription("Initiates the Google OAuth flow and redirects to Google authentication.");
        group.MapGet("/google/result", GetGoogleAuthResultAsync)
            .WithName("GoogleSignUpResult")
            .WithSummary("Completes Google sign-up.")
            .WithDescription("Consumes the external authentication result and creates a user from Google profile data.");
        group.MapPost("/google/signin", SignInByGoogleAsync)
            .WithName("GoogleSignIn")
            .WithSummary("Signs in through Google.")
            .WithDescription("Uses the authenticated Google external session and returns local JWT tokens.");

        group.MapPost("/refresh-token", RefreshTokenAsync)
            .WithName("RefreshToken")
            .WithSummary("Refreshes an expired access token.")
            .WithDescription("Accepts a refresh token and returns a new access token with a rotated refresh token.");

        group.MapPost("/change-password", ChangePasswordAsync)
            .WithName("ChangePassword")
            .WithSummary("Changes the current user's password.")
            .WithDescription("Requires authentication and validates the current password before applying the new password.")
            .AddEndpointFilter<OwnerOrAdminAccessEndpointFilter>()
            .RequireAuthorization();
        group.MapPut("/profile", UpdateOwnProfileAsync)
            .WithName("UpdateOwnProfile")
            .WithSummary("Updates the current authenticated user's public profile settings.")
            .WithDescription("Allows changing the display name and phone number for the authenticated user.")
            .RequireAuthorization();
        group.MapPost("/avatar/{email}", UploadAvatarAsync)
            .WithName("UploadUserAvatar")
            .WithSummary("Uploads a single avatar image for a user.")
            .WithDescription("Allows only the user owner or an admin to upload one avatar through ImageService.")
            .AddEndpointFilter<OwnerOrAdminAccessEndpointFilter>()
            .RequireAuthorization(PolicyNames.OwnerOrAdminAccess)
            .Accepts<UploadAvatarRequest>("multipart/form-data")
            .DisableAntiforgery();
        group.MapPost("/avatar", UploadOwnAvatarAsync)
            .WithName("UploadOwnUserAvatar")
            .WithSummary("Uploads a single avatar image for the current authenticated user.")
            .WithDescription("Uploads one avatar through ImageService for the current authenticated user.")
            .RequireAuthorization()
            .Accepts<UploadAvatarRequest>("multipart/form-data")
            .DisableAntiforgery();

        agentsGroup.MapGet("/{id:guid}/public-profile", GetSellerPublicProfileAsync)
            .WithName("GetSellerPublicProfile")
            .WithSummary("Gets the public seller profile by agent id.")
            .WithDescription("Returns a public-safe seller profile projection intended for BFF consumption.");

        return group;
    }

#region Base authentication

    public async Task<IResult> SignUpAsync(ISender sender, 
        SignUpUserDto dto,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SignUpCommand(dto), cancellationToken);
        return result.ToHttpResult();
    }

    public async Task<IResult> SignUpWithAvatarAsync(
        ISender sender,
        IImageServiceClient imageServiceClient,
        [FromForm] SignUpWithAvatarRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.User))
            return Results.BadRequest("Необходимо передать JSON-поле `user`.");

        SignUpUserDto? dto;
        try
        {
            dto = JsonSerializer.Deserialize<SignUpUserDto>(request.User, UserJsonOptions);
        }
        catch (JsonException ex)
        {
            return Results.BadRequest($"Invalid user json payload: {ex.Message}");
        }

        if (dto is null)
            return Results.BadRequest("Не удалось разобрать данные пользователя.");

        if (request.Image is not null)
        {
            var uploadResult = await imageServiceClient.UploadSingleAsync(request.Image, cancellationToken);
            if (!uploadResult.IsSuccess || uploadResult.Data is null)
                return uploadResult.ToHttpResult();

            dto = dto with { AccountImageUrl = uploadResult.Data.Url };
        }

        var result = await sender.Send(new SignUpCommand(dto), cancellationToken);
        return result.ToHttpResult();
    }

    public async Task<IResult> SignInAsync(ISender sender, 
        SignInUserDto dto,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SignInCommand(dto), cancellationToken);
        return result.ToHttpResult();
    }

    public async Task<IResult> VerifyEmailAsync(
        ISender sender,
        VerifyEmailCodeDto dto,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new VerifyEmailCodeCommand(dto), cancellationToken);
        return result.ToHttpResult();
    }

    public async Task<IResult> ResendEmailVerificationCodeAsync(
        ISender sender,
        ResendEmailVerificationCodeDto dto,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ResendEmailVerificationCodeCommand(dto), cancellationToken);
        return result.ToHttpResult();
    }

#endregion

#region Google authentication

    public async Task SignUpByGoogleAsync(HttpContext context)
    {
        var props = new AuthenticationProperties
        {
            RedirectUri = "/api/users/google/result"
        };

        await context.ChallengeAsync(GoogleDefaults.AuthenticationScheme, props);
    }

    public async Task<IResult> SignInByGoogleAsync(ISender sender, 
        HttpContext context,
        CancellationToken cancellationToken)
    {
        await context.AuthenticateAsync(IdentityConstants.ExternalScheme);
        var result = await sender.Send(new SignInByGoogleCommand(), cancellationToken);
        return result.ToHttpResult();
    }
    
    public async Task<IResult> GetGoogleAuthResultAsync(HttpContext context, 
        ISender sender, 
        CancellationToken cancellationToken)
    {
        await context.AuthenticateAsync(IdentityConstants.ExternalScheme);
        var result = await sender.Send(new SignUpByGoogleCommand(), cancellationToken);
        return result.ToHttpResult();
    }

#endregion

    public async Task<IResult> RefreshTokenAsync(ISender sender, 
        RefreshTokenRequestDto dto,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RefreshTokenCommnand(dto), cancellationToken);
        return result.ToHttpResult();
    }

    public async Task<IResult> GetUserOwnInfoAsync(ISender sender, 
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetUserOwnInfoQuery(), cancellationToken);
        return result.ToHttpResult();
    }

    public async Task<IResult> GetUserInfoByEmailAsync(ISender sender, 
        [FromRoute] string email,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetUserByEmailQuery(email), cancellationToken);
        return result.ToHttpResult();
    }

    public async Task<IResult> ChangePasswordAsync(ISender sender, 
        ChangePasswordDto dto,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ChangePasswordCommand(dto), cancellationToken);
        return result.ToHttpResult();
    }

    public async Task<IResult> UpdateOwnProfileAsync(
        ISender sender,
        UpdateOwnProfileDto dto,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateOwnProfileCommand(dto), cancellationToken);
        return result.ToHttpResult();
    }

    public async Task<IResult> UploadAvatarAsync(
        ISender sender,
        IImageServiceClient imageServiceClient,
        [FromRoute] string email,
        [FromForm] UploadAvatarRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Image is null)
            return Results.BadRequest("Файл изображения обязателен.");

        var uploadResult = await imageServiceClient.UploadSingleAsync(request.Image, cancellationToken);
        if (!uploadResult.IsSuccess || uploadResult.Data is null)
            return uploadResult.ToHttpResult();

        var result = await sender.Send(
            new UploadUserAvatarCommand(email, uploadResult.Data.Url),
            cancellationToken);

        return result.ToHttpResult();
    }

    public async Task<IResult> UploadOwnAvatarAsync(
        ISender sender,
        IImageServiceClient imageServiceClient,
        ICurrentUserProvider currentUserProvider,
        [FromForm] UploadAvatarRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Image is null)
            return Results.BadRequest("Файл изображения обязателен.");

        var uploadResult = await imageServiceClient.UploadSingleAsync(request.Image, cancellationToken);
        if (!uploadResult.IsSuccess || uploadResult.Data is null)
            return uploadResult.ToHttpResult();

        var result = await sender.Send(
            new UploadUserAvatarCommand(currentUserProvider.GetEmail(), uploadResult.Data.Url),
            cancellationToken);

        return result.ToHttpResult();
    }

    public async Task<IResult> GetSellerPublicProfileAsync(
        ISender sender,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSellerPublicProfileQuery(id), cancellationToken);
        return result.ToHttpResult();
    }
}
