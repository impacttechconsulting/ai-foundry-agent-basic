namespace AiFoundryAgent.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    // Hardcoded credentials for basic authentication
    private const string ValidUsername = "admin";
    private const string ValidPassword = "password123";

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (request.Username == ValidUsername && request.Password == ValidPassword)
        {
            var token = GenerateToken(request.Username);
            
            return Ok(new { 
                success = true, 
                token = token,
                username = request.Username
            });
        }
        
        return Unauthorized(new { success = false, message = "Invalid credentials" });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(new { success = true, message = "Logged out successfully" });
    }

    [HttpGet("validate")]
    public IActionResult ValidateToken()
    {
        var token = GetTokenFromHeader();
        
        if (string.IsNullOrEmpty(token))
        {
            return Unauthorized(new { success = false, message = "No token provided" });
        }

        if (ValidateToken(token))
        {
            var username = ExtractUsernameFromToken(token);
            return Ok(new { success = true, username = username });
        }

        return Unauthorized(new { success = false, message = "Invalid or expired token" });
    }

    private string GenerateToken(string username)
    {
        var tokenData = $"{username}|{DateTime.UtcNow.AddHours(24):O}"; // Valid for 24 hours, using | as separator to avoid conflicts with datetime format
        var tokenBytes = System.Text.Encoding.UTF8.GetBytes(tokenData);
        return Convert.ToBase64String(tokenBytes);
    }

    private bool ValidateToken(string token)
    {
        try
        {
            var tokenData = Convert.FromBase64String(token);
            var decodedString = System.Text.Encoding.UTF8.GetString(tokenData);
            var parts = decodedString.Split(':');
            
            if (parts.Length != 2)
                return false;
                
            var username = parts[0];
            var expiryString = parts[1];
            
            if (!DateTime.TryParse(expiryString, out DateTime expiry))
                return false;
                
            return DateTime.UtcNow < expiry;
        }
        catch
        {
            return false;
        }
    }

    private string ExtractUsernameFromToken(string token)
    {
        try
        {
            var tokenData = Convert.FromBase64String(token);
            var decodedString = System.Text.Encoding.UTF8.GetString(tokenData);
            var parts = decodedString.Split(':');
            
            if (parts.Length >= 1)
                return parts[0];
                
            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private string GetTokenFromHeader()
    {
        if (Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var header = authHeader.ToString();
            if (header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return header.Substring(7).Trim();
            }
        }
        
        return string.Empty;
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}