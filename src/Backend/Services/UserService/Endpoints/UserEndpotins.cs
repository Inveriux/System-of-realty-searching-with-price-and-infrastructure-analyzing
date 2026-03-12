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
        var group = application.MapGroup("/api/users");

        group.MapGet("/info/{email}", GetUserInfoByEmailAsync)
            .AddEndpointFilter<OwnerOrAdminAccessEndpointFilter>()
            .RequireAuthorization(PolicyNames.OwnerOrAdminAccess);

        group.MapGet("/info", GetUserOwnInfoAsync)
            .AddEndpointFilter<OwnerOrAdminAccessEndpointFilter>()
            .RequireAuthorization();

        group.MapPost("/signup", SignUpAsync);
        group.MapPost("/signin", SignInAsync);
        
        group.MapPost("/google/signup", SignUpByGoogleAsync);
        group.MapGet("/google/result", GetGoogleAuthResultAsync);
        group.MapPost("/google/signin", SignInByGoogleAsync);

        group.MapPost("/refresh-token", RefreshTokenAsync);

        group.MapPost("/change-password", ChangePasswordAsync)
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
