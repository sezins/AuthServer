using Auth.Core.Configuration;
using Auth.Core.DTOs;
using Auth.Core.Entities;
using Auth.Core.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SharedLibrary.Configurations;
using SharedLibrary.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;


namespace Auth.Service.Services
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<UserApp> _userManager;
        private readonly CustomTokenOption _tokenOption;

        public TokenService(UserManager<UserApp> userManager,IOptions<CustomTokenOption> options)
        {
            _userManager=userManager;
            _tokenOption=options.Value;
        }
        private string CreateRefreshToken()
        {
            var numberByte = new Byte[32];
            using var rnd = RandomNumberGenerator.Create();
            rnd.GetBytes(numberByte);
            return Convert.ToBase64String(numberByte);
        }

        private IEnumerable<Claim> GetClaim(UserApp userApp,List<String> audiences)
        {//üyelik sistemi gerektıren bır token olusturmak ıcın bunu kullanırız.
            var userList = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,userApp.Id),
                new Claim(JwtRegisteredClaimNames.Email,userApp.Email),
                new Claim(ClaimTypes.Name,userApp.UserName),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
            };
            userList.AddRange(audiences.Select(x => new Claim(JwtRegisteredClaimNames.Aud, x)));
            return userList;
        } 

        private IEnumerable<Claim> GetClaimsByClient(Client client)
        {//üyelik sistemi gerektırmeyen bır token olusturmak ıcın bunu kullanırız.
            var claims = new List<Claim>();
            claims.AddRange(client.Audiences.Select(x=>new Claim(JwtRegisteredClaimNames.Aud,x)));

            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString());
            new Claim(JwtRegisteredClaimNames.Sub,client.ClientId.ToString());
            return claims;
        }
        public TokenDto CreateToken(UserApp userApp)
        {
            var accessTokenExpiration = DateTime.Now.AddMinutes(_tokenOption.AccessTokenExpiration);
            var refreshTokenExpiration = DateTime.Now.AddMinutes(_tokenOption.RefreshTokenExpiration);
            var securityKey = SignService.GetSymmetricSecurityKey(_tokenOption.SecurityKey);

            //Token imza kısmı
            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            //appsettings de girdiğimiz kısımları burada tanımladık.
            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
                issuer: _tokenOption.Issuer,
                expires: accessTokenExpiration,
                notBefore: DateTime.Now,
                //Test amaçlı yaptığımız için metotu asenkron yapmamak için sonuna Result ekledik. Bunu eklemeseydik gelen claimler async olduğu için Task ile dönecekti o sebeple result dedik. Asenkron bir metottan senkron şekilde sonucunu almak için ".Result" (bloklayıcı)
                claims:GetClaim(userApp, _tokenOption.Audience),
                signingCredentials: signingCredentials);

            //Tokenı oluşturacak olan class
            var handler = new JwtSecurityTokenHandler();

            //Tokenı oluşturan metot
            var token = handler.WriteToken(jwtSecurityToken);

            //TokenDto tipimize çeviriyoruz.
            var tokenDto = new TokenDto
            {
                AccessToken = token,
                RefreshToken = CreateRefreshToken(),
                AccessTokenExpiration = accessTokenExpiration,
                RefreshTokenExpiration = refreshTokenExpiration
            };

            return tokenDto;

        }

        public ClientTokenDto CreateTokenByClient(Client client)
        {
            //clientlar için token olusturma
            //refresh token clientlarda olmayacak
            //
            var accessTokenExpiration = DateTime.Now.AddMinutes(_tokenOption.AccessTokenExpiration);
            
            var securityKey = SignService.GetSymmetricSecurityKey(_tokenOption.SecurityKey);

            //Token imza kısmı
            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            //appsettings de girdiğimiz kısımları burada tanımladık.
            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
                issuer: _tokenOption.Issuer,
                expires: accessTokenExpiration,
                notBefore: DateTime.Now,
                //Test amaçlı yaptığımız için metotu asenkron yapmamak için sonuna Result ekledik. Bunu eklemeseydik gelen claimler async olduğu için Task ile dönecekti o sebeple result dedik. Asenkron bir metottan senkron şekilde sonucunu almak için ".Result" (bloklayıcı)
                claims: GetClaimsByClient(client),
                signingCredentials: signingCredentials); 

            //Tokenı oluşturacak olan class
            var handler = new JwtSecurityTokenHandler();

            //Tokenı oluşturan metot
            var token = handler.WriteToken(jwtSecurityToken);

            //TokenDto tipimize çeviriyoruz.
            var tokenDto = new ClientTokenDto
            {
                AccessToken = token,
               
                AccessTokenExpiration = accessTokenExpiration,
                
            };

            return tokenDto;
        }
    }
}
