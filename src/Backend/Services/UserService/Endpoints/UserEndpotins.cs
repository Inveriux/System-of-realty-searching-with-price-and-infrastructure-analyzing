using BasicBuildingBlocks.BusinessResults.Extensoins;
using Microsoft.AspNetCore.Authentication.Google;
using BasicBuildingBlocks.Endpoints.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using UserService.UseCases.Users;
using Microsoft.AspNetCore.Mvc;
using UserService.Models.Enums;
using UserService.Filters;
using UserService.DTOs;
using MediatR;

namespace UserService.Endpoints;

public class UserEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var group = application.MapGroup("/api/users")
            .WithTags("Users");

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

    public async Task<IResult> SignInAsync(ISender sender, 
        SignInUserDto dto,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SignInCommand(dto), cancellationToken);
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
}
