using Auth.Core.DTOs;
using Auth.Core.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Apı.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : CustomBaseController
    {
        private readonly IAuthenricationService _authenricationService;
        public AuthController(IAuthenricationService authenricationService)
        {
            _authenricationService = authenricationService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateToken(LoginDto loginDto)
        {
            return CreateActionResult(await _authenricationService.CreateTokenAsync(loginDto));
            
        }
        [HttpPost]
        public async Task<IActionResult> CreateTokenByClient(ClientLoginDto clientLoginDto)
        {
            return CreateActionResult(await _authenricationService.CreateTokenByClient(clientLoginDto));

        }

        [HttpPost]
        public async Task<IActionResult> RevokeRefreshToken(string refreshToken)
        {
            return CreateActionResult(await _authenricationService.RevokeRefreshTokenAsync(refreshToken));

        }
        [HttpPost]
        public async Task<IActionResult> CreateTokenByRefreshToken(RefreshTokenDto refreshTokenDto)
        {
            return CreateActionResult(await _authenricationService.CreateTokenByRefreshToken(refreshTokenDto.Token));

        }
    }
}
