using MagicVilla_API.Authentication;
using MagicVilla_API.Data;
using MagicVilla_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MagicVilla_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizeController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly JWTSettingscs _jwt;
        private readonly IRefreshhandler _refreshhandler;

        // Bind hierarchical configuration
        //The options pattern uses classes to provide strongly-typed access to groups of related settings
        //For example, to read the highlighted configuration values from an appsettings.json file:
        public AuthorizeController(ApplicationDbContext db, IOptions<JWTSettingscs> options, IRefreshhandler refreshHandler)
        {
            this._db = db;
            this._jwt = options.Value;
            _refreshhandler = refreshHandler;
        }

        [HttpPost("GenerateToken")]
        public async Task<ActionResult> GenerateToken([FromBody] UserCredencial userCredencial)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u=>u.Name == userCredencial.UserName && u.Password == userCredencial.Password); 
            
            if (user != null)
            {
                // genetate token
                // A SecurityTokenHandler designed for creating and validating Json Web Tokens.
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenKey = Encoding.UTF8.GetBytes(_jwt.SecurityKey);
                // This is a place holder for all the attributes related to the issued token.
                var tokenDesc = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, user.Name),
                        new Claim(ClaimTypes.Role, user.Role)
                    }),
                    Expires = DateTime.UtcNow.AddSeconds(30),
                    // Represents the cryptographic key and security algorithms that are used to generate a digital signature
                    // Represents the abstract base class for all keys that are generated using symmetric algorithms.
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256)
                };

                var token = tokenHandler.CreateToken(tokenDesc);
                var finaltoken = tokenHandler.WriteToken(token);

                return Ok(new TokenResponse
                {
                    Token = finaltoken,
                    RefreshToken = await _refreshhandler.GenerateToken(userCredencial.UserName)
                }) ;

            }else
            {
                return Unauthorized();
            }
        }


        [HttpPost("GenerateRefreshToken")]
        public async Task<ActionResult> GenerateToken([FromBody]TokenResponse tokenResponse)
        {
            var refTokenLogic = _db.RefreshTokenLogics.FirstOrDefaultAsync(u=>u.RefreshToke == tokenResponse.RefreshToken);

            if (refTokenLogic != null)
                {
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenKey = Encoding.UTF8.GetBytes(_jwt.SecurityKey);
                SecurityToken securityToken;
                //Reads and validates a 'JSON Web Token' (JWT) encoded as a JWS or JWE in Compact Serialized Format
                //A ClaimsPrincipal from the JWT. Does not include claims found in the JWT header.
                var principal = tokenHandler.ValidateToken(tokenResponse.Token, new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(tokenKey),
                    ValidateIssuer = false,
                    ValidateAudience = false
                    
                }, out securityToken);

                var _token = securityToken as JwtSecurityToken;

                if (_token != null && _token.Header.Alg.Equals(SecurityAlgorithms.HmacSha256)) {
                    string? userName = principal.Identity?.Name;
                    var exitData = _db.RefreshTokenLogics.First(u => u.UserName == userName && u.RefreshToke == tokenResponse.RefreshToken);

                    if (exitData != null)
                    {
                        //A SecurityToken designed for representing a JSON Web Token (JWT).
                        var _newtoken = new JwtSecurityToken(
                                claims: principal.Claims.ToArray(),
                                expires: DateTime.UtcNow.AddSeconds(30),
                                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecurityKey)), SecurityAlgorithms.HmacSha256)
                            );
                        var finaltoken = tokenHandler.WriteToken(_newtoken);

                        return Ok(new TokenResponse
                        {
                            Token = finaltoken,
                            RefreshToken = await _refreshhandler.GenerateToken(userName)
                        });
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }else
                {
                    return Unauthorized();
                }
            }else
            {
                return Unauthorized();
            }
        }
    }
}
