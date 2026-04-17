using Backend.IntegrationTests.Common;
using FluentAssertions;
using Xunit;

namespace Backend.IntegrationTests.UserServiceTests;

[Trait("Category", "Integration")]
public sealed class UserAuthIntegrationTests : IClassFixture<UserServiceFactory>
{
    private readonly UserServiceFactory _factory;

    public UserAuthIntegrationTests(UserServiceFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    [Trait("Category", "Smoke")]
    public async Task RootEndpoint_ShouldReturnOk()
    {
        await _factory.EnsureInitializedAsync();
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task SignUp_ThenVerify_ThenSignIn_ShouldReturnJwtToken()
    {
        await _factory.EnsureInitializedAsync();
        using var client = _factory.CreateClient();
        var email = $"user-{Guid.NewGuid():N}@example.com";
        const string password = "strongpass1";

        var signUpResponse = await client.PostAsJsonAsync("/api/users/signup", new
        {
            name = "Test User",
            email,
            password,
            accountImageUrl = (string?)null
        });

        signUpResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var verifyResponse = await client.PostAsJsonAsync("/api/users/email-verification/verify", new
        {
            email,
            code = _factory.EmailVerification.GetCode(email)
        });

        verifyResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var signInResponse = await client.PostAsJsonAsync("/api/users/signin", new
        {
            email,
            password
        });

        signInResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var payload = await signInResponse.Content.ReadFromJsonAsync<ResultEnvelope<SignInPayload>>();
        payload!.Data!.Token.Should().NotBeNullOrWhiteSpace();
        payload.Data.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    [Trait("Category", "Smoke")]
    public async Task GetCurrentUserInfo_WithoutToken_ShouldReturnUnauthorized()
    {
        await _factory.EnsureInitializedAsync();
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/users/info");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUserInfo_WithToken_ShouldReturnCurrentUserProfile()
    {
        var (token, email) = await _factory.RegisterAndLoginAsync();
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/users/info");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<ResultEnvelope<UserInfoPayload>>();
        payload!.Data!.Email.Should().Be(email);
        payload.Data.EmailConfirmed.Should().BeTrue();
    }

    private sealed class SignInPayload
    {
        public string Token { get; init; } = string.Empty;
        public string RefreshToken { get; init; } = string.Empty;
    }

    private sealed class UserInfoPayload
    {
        public string Email { get; init; } = string.Empty;
        public bool EmailConfirmed { get; init; }
    }
}
