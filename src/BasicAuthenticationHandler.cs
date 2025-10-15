namespace AiFoundryAgent;


public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Determine if request should be authenticated
        bool shouldAuthenticate = Context.Request.Path.StartsWithSegments("/chat");
        
        // Special handling for /auth/validate - it needs authentication to verify the token
        if (Context.Request.Path.StartsWithSegments("/auth/validate"))
        {
            shouldAuthenticate = true;  // This specific endpoint requires authentication
        }
        else if (Context.Request.Path.StartsWithSegments("/auth"))
        {
            // Other auth endpoints like /auth/login, /auth/logout should not be authenticated
            return AuthenticateResult.NoResult();
        }

        if (!shouldAuthenticate)
        {
            return AuthenticateResult.NoResult();
        }

        if (!Context.Request.Headers.ContainsKey("Authorization"))
        {
            return AuthenticateResult.NoResult();
        }

        string authHeader = Context.Request.Headers["Authorization"].ToString();

        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        string token = authHeader.Substring("Bearer ".Length).Trim();

        // Make the token validation properly async
        var (isValid, username) = await ValidateTokenAsync(token);

        if (isValid)
        {
            var claims = new[] { new Claim(ClaimTypes.Name, username) };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
        else
        {
            return AuthenticateResult.Fail("Invalid token");
        }
    }

    private async Task<(bool isValid, string username)> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenData = Convert.FromBase64String(token);
            var decodedString = Encoding.UTF8.GetString(tokenData);
            var parts = decodedString.Split('|');  // Using | as separator to avoid conflicts with datetime format

            if (parts.Length != 2)
                return (false, string.Empty);

            var tokenUsername = parts[0];
            var expiryString = parts[1];

            if (!DateTime.TryParse(expiryString, out DateTime expiry))
                return (false, string.Empty);

            if (DateTime.UtcNow >= expiry)
                return (false, string.Empty);

            if (tokenUsername == "admin") // This matches the hardcoded username in AuthController
            {
                string username = tokenUsername;

                return (true, username);
            }

            return (false, string.Empty);
        }
        catch
        {
            return (false, string.Empty);
        }
    }
}

