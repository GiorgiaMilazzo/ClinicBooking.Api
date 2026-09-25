using Microsoft.AspNetCore.Identity;
using ClinicBooking.Api.Models;

// algoritmo di hashing 
// libreria testata e verificata di .NET, Microsoft.AspNetCore.Identity, con la classe PasswordHasher<T>
// la libreria ti dà degli strumenti con nomi propri (VerifyHashedPassword, PasswordVerificationResult.Success) che devi usare esattamente come sono documentati, non puoi inventarteli o abbreviarli a piacere.

namespace ClinicBooking.Api.Services
{
    public class PasswordService
    {
        private readonly PasswordHasher<User> _hasher = new();

        public string HashPassword(User user, string plainPassword)
        {
            return _hasher.HashPassword(user, plainPassword); // _hasher è un oggetto della classe PasswordHasher<User>  
        }

        public bool VerifyPassword(User user, string hashedPassword, string plainPassword)
        {
            var result = _hasher.VerifyHashedPassword(user, hashedPassword, plainPassword);
            return result == PasswordVerificationResult.Success; // enum (Success o Failed)
        }
    }
}