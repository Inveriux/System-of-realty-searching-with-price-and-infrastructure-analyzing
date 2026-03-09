using BasicBuildingBlocks.Endpoints.Abstractions;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using BasicBuildingBlocks.Results;
using UserService.UseCases.Users;
using Microsoft.AspNetCore.Mvc;
using UserService.Models.Enums;
using UserService.DTOs;
using MediatR;

namespace UserService.Endpoints;

public class UserEndpoints : IEndpointRegistrar
{
    public RouteGroupBuilder MapEndpoints(WebApplication application)
    {
        var group = application.MapGroup("/api/users");

        group.MapGet("/info/{email}", GetUserInfoByEmail)
            .RequireAuthorization(UserRoles.User);

        group.MapPost("/signup", SignUp);
        group.MapPost("/signin", SignIn);
        
        group.MapGet("/google/signup", async context =>
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = "/api/users/google/result"
            };

            await context.ChallengeAsync(GoogleDefaults.AuthenticationScheme, props);
        });

        group.MapGet("/google/result", async (HttpContext context, ISender sender, 
            CancellationToken cancellationToken) =>
        {
            await context.AuthenticateAsync(IdentityConstants.ExternalScheme);
            return await sender.Send(new SignUpByGoogleCommand(), cancellationToken);
        });

        group.MapGet("/google/signin", async (HttpContext context, ISender sender, 
            CancellationToken cancellationToken) =>
        {
            await context.AuthenticateAsync(IdentityConstants.ExternalScheme);
            return await sender.Send(new SignInByGoogleCommand(), cancellationToken);
        });

        return group;
    }

    public async Task<Result<bool>> SignUp(ISender sender, 
        SignUpUserDto dto,
        CancellationToken cancellationToken)
    {
        return await sender.Send(new SignUpCommand(dto), cancellationToken);
    }

    public async Task<Result<SignInUserResponseDto>> SignIn(ISender sender, 
        SignInUserDto dto,
        CancellationToken cancellationToken)
    {
        return await sender.Send(new SignInCommand(dto), cancellationToken);
    }

    public async Task<Result<GetUserDto>> GetUserInfoByEmail(ISender sender, 
        [FromRoute] string email,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetUserByEmailQuery(email), cancellationToken);
        return result;
    }
}
