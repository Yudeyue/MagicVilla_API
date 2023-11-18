

using MagicVilla_API.Data;
using MagicVilla_API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Identity.Client;
using System.Security.Cryptography;

namespace MagicVilla_API.Authentication
{
    public class RefreshHandler : IRefreshhandler
    {
        private readonly ApplicationDbContext _db;

        public RefreshHandler(ApplicationDbContext db)
        {
            this._db = db;
        }

        public async Task<string> GenerateToken(string username)
        {
            var randomNumber = new byte[32];

            using (var randomnumbergenerator = RandomNumberGenerator.Create())
            {
                randomnumbergenerator.GetBytes(randomNumber);
                string refreshtoken = Convert.ToBase64String(randomNumber);
                var ExistToken = _db.RefreshTokenLogics.FirstOrDefaultAsync(u => u.UserName == username).Result;
                if (ExistToken != null)
                {
                    ExistToken.RefreshToke = refreshtoken;
                }
                else
                {
                    await _db.RefreshTokenLogics.AddAsync(new RefreshTokenLogic
                    {
                        UserName = username,
                        TokenId = new Random().Next(),
                        RefreshToke = refreshtoken
                    }); 
                }

                await _db.SaveChangesAsync();
                return refreshtoken;
            }
        }
    }
}
