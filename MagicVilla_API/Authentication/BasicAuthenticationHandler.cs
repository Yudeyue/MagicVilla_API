using Azure.Identity;
using MagicVilla_API.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace MagicVilla_API.Authentication
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly ApplicationDbContext _db;
        public BasicAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, ApplicationDbContext db) : base(options, logger, encoder, clock)
        {
            _db = db;
        }

        protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // check if we have authentication header, the header name is Authorization
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.Fail("No header found");
            }

            var headerValue = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            if (headerValue != null)
            {
                var bytes = Convert.FromBase64String(headerValue.Parameter);
                string credentical = Encoding.UTF8.GetString(bytes);
                string[] array = credentical.Split(":");
                string username = array[0];
                string password = array[1];

                var user = await _db.Users.FirstOrDefaultAsync(u => u.Name == username && u.Password == password);

                if (user != null)
                {
                    // initial a new Claim object with input(name, value), claim is a statement about a subject and issuer
                    // Initializes a new instance of the Claim class with the specified claim type, and value.
                    var claim = new[] { new Claim(ClaimTypes.Name, user.Name) };
                    // Represents a claims-based identity.an identity described by a collection of claims
                    // Initializes a new instance of the ClaimsIdentity class with the specified claims and authentication type.
                    var identity = new ClaimsIdentity(claim, Scheme.Name);
                    // An IPrincipal implementation that supports multiple claims-based identities.
                    // ClaimsPrincipal exposes a collection of identities, each of which is a ClaimsIdentity. In the common case, this collection, which is accessed through the Identities property, will only have a single element.
                    var principal = new ClaimsPrincipal(identity);
                    // Contains user identity information as well as additional authentication state.
                    var ticket = new AuthenticationTicket(principal, Scheme.Name);

                    return AuthenticateResult.Success(ticket);  
                }else
                {
                    return AuthenticateResult.Fail("UnAutorized");
                }
            }else
            {
                return AuthenticateResult.Fail("Empty header");
            }
        }
    }
}
