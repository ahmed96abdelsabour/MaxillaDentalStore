using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Common.Helpers
{
    public class PasswordHasher : IPasswordHasher
    {
        /*
         * We are using BCrypt algorithm for hashing passwords.
         * BCrypt is a password-hashing function designed to be slow,
         * which makes it resistant to brute-force attacks.
         * 
         * It automatically handles salt generation and storage within the hash itself.
         */

        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.EnhancedHashPassword(password);
        }

        public bool Verify(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(password, hashedPassword);
        }
    }
}
