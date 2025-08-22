using System;
using Training.Models;

namespace Training.Service;

public interface IAuthService
{
    Task<(string? JwtToken, string? RefreshToken)> AuthenticateUser(string email, string password);
    Task<User?> RegisterUser(User user, string password);
    Task<(string? JwtToken, string? RefreshToken)> Refresh(string refreshToken);
    
}
