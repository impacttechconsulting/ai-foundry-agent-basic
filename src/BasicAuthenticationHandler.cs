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
        if (Context.Request.Path.StartsWithSegments("/api") || 
            Context.Request.Path.StartsWithSegments("/auth"))
        {
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

        return AuthenticateResult.NoResult();
    }

    private bool ValidateToken(string token, out string username)
    {
        username = string.Empty;
        try
        {
            var tokenData = Convert.FromBase64String(token);
            var decodedString = Encoding.UTF8.GetString(tokenData);
            var parts = decodedString.Split(':');
            
            if (parts.Length != 2)
                return false;
                
            var tokenUsername = parts[0];
            var expiryString = parts[1];
            
            if (!DateTime.TryParse(expiryString, out DateTime expiry))
                return false;
                
            if (DateTime.UtcNow >= expiry)
                return false;

            if (tokenUsername == "admin") // This matches the hardcoded username in AuthController
            {
                username = tokenUsername;
                return true;
            }
            
            return false;
        }
        catch
        {
            return false;
        }
    }
    
    private async Task<(bool isValid, string username)> ValidateTokenAsync(string token)
    {
        return await Task.Run(() =>
        {
            string username = string.Empty;
            try
            {
                var tokenData = Convert.FromBase64String(token);
                var decodedString = Encoding.UTF8.GetString(tokenData);
                var parts = decodedString.Split(':');
                
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
                    username = tokenUsername;
                    return (true, username);
                }
                
                return (false, string.Empty);
            }
            catch
            {
                return (false, string.Empty);
            }
        });
    }
}

