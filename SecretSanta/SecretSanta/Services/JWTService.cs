using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
//using System.Text.Json;

namespace SecretSanta.Services;

public class JWTService  {

    static SymmetricSecurityKey getKey(){
        ReadOnlySpan<byte> key = "4df48011-3c8c-4732-b21c-a5aedb29cad5"u8;
        return new SymmetricSecurityKey(key.ToArray());
    }

    static SigningCredentials getCreds(){
        return new SigningCredentials(getKey(), SecurityAlgorithms.HmacSha256);
    }

    public string CreateJWTAddNewEMail(string newemail, string userid){
        Claim[] claims = {
            //new Claim(JwtRegisteredClaimNames.Sub, "user_ID"),
            new Claim("ApplicationUserId", userid),
            new Claim("EmailAddress", newemail)
        };

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: "issuer_here",
            audience: "audience_here",
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: getCreds()
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string? ValidateJWTAddNewEMail(string jwtoken, out string? email, out string? userid){
        TokenValidationParameters validationParameters = new TokenValidationParameters {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = getKey(),
            ValidateIssuer = true,
            ValidIssuer = "issuer_here",
            ValidateAudience = true,
            ValidAudience = "audience_here",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        try{
            SecurityToken validatedToken;
            ClaimsPrincipal principal = new JwtSecurityTokenHandler().ValidateToken(jwtoken, validationParameters, out validatedToken);
            //var userIdClaim  = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            //Console.WriteLine(JsonSerializer.Serialize(principal, new JsonSerializerOptions{ ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles, WriteIndented = true }));
            userid = principal.Claims.FirstOrDefault(c => c.Type == "ApplicationUserId")?.Value;
            email  = principal.Claims.FirstOrDefault(c => c.Type == "EmailAddress")?.Value;
            if(email==null || userid==null){
                throw new Exception("data not present");
            }
        } catch(Exception e) {
            email = null;
            userid = null;
            return "Invalid JWT "+e.Message;
        }
        return null;
    }

}

