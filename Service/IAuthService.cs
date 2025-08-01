using System;
using Training.Models;

namespace Training.Service;

public interface IAuthService
{
    Task<string?> AuthenticateUser(string email, string password); 
    Task<User?> RegisterUser(User user, string password); 
}
