using System;

namespace SimpleIdentity.DTOs;

public class RegisterDTO
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public required string Email { get; set; }
    public required string Password { get; set; }

}

public class LoginDTO
{
    public required string Email { get; set; }
    public required string Password { get; set; }

}

