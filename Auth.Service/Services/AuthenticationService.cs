using Auth.Core.Configuration;
using Auth.Core.DTOs;
using Auth.Core.Entities;
using Auth.Core.Repository;
using Auth.Core.Service;
using Auth.Core.UnitofWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Service.Services
{
    public class AuthenticationService : IAuthenricationService
    {
        private readonly List<Client> _clients;
        private readonly ITokenService _tokenservice;
        private readonly UserManager<UserApp> _userManager;
        private readonly IUnitofWork _unitOfWork;
        private readonly IGenericRepository<UserRefreshToken> _userRefreshTokenRepository;

        public AuthenticationService(IOptions<List<Client>> optionClients, ITokenService tokenservice, UserManager<UserApp> userManager, IUnitofWork unitOfWork, IGenericRepository<UserRefreshToken> userRefreshTokenRepository)
        {
            _clients = optionClients.Value;
            _tokenservice = tokenservice;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _userRefreshTokenRepository = userRefreshTokenRepository;
        }

        //Üyelik gerektiren api için kullanılacak.
        public async Task<Response<TokenDto>> CreateTokenAsync(LoginDto loginDto)
        {
            if (loginDto == null)
            {
                throw new ArgumentNullException(nameof(loginDto));
            }

            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null)
            {
                //TokenDto döndük ama boş olarak.Response model tokenDto istediği için Fail ile TokenDto null olarak gönderiyoruz. Kötü niyetli kullanıcılar hangisinin yanlış olduğunu anlayamasın diye Email veya şifre dedik.
                return Response<TokenDto>.Fail("Email or Password is wrong", 400, true);
            }

            if (!await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return Response<TokenDto>.Fail("Email or Password is wrong", 400, true);
            }

            var token = _tokenservice.CreateToken(user);

            //Sistemde refresh token olup olmadığını kontrol ediyoruz.
            var userRefreshToken = await _userRefreshTokenRepository.Where(x => x.UserId == user.Id).SingleOrDefaultAsync();
            //Sistemde refresh token olmadığı için yeniden ürettik.
            if (userRefreshToken == null)
            {
                await _userRefreshTokenRepository.AddAsync(new UserRefreshToken { UserId = user.Id, Code = token.RefreshToken, Expiration = token.RefreshTokenExpiration }) ;
            }
            else
            {
                userRefreshToken.Code = token.RefreshToken;
                userRefreshToken.Expiration = token.RefreshTokenExpiration;
            }

            await _unitOfWork.CommitAsync();
            return Response<TokenDto>.Success(token, 200);
        }
        //Client yani üyelik gerektirmen api için kullanılacak.
        public async Task<Response<ClientTokenDto>> CreateTokenByClient(ClientLoginDto clientLoginDto)
        {
            //Appsettings üzerinden oluşturduğumuz client dizininde bize gelen id var mı yok mu buna bakıyoruz.
            var client = _clients.SingleOrDefault(x => x.ClientId == clientLoginDto.ClientId && x.ClientSecret == clientLoginDto.ClientSecret);

            if (client == null)
            {
                return Response<ClientTokenDto>.Fail("ClientId or ClientSecret not found", 404, true);
            }

            var token = _tokenservice.CreateTokenByClient(client);
            return Response<ClientTokenDto>.Success(token, 200);
        }

        public async Task<Response<TokenDto>> CreateTokenByRefreshToken(string refreshToken)
        {
            //Sistemde refresh token olup olmadığını kontrol ediyoruz.
            var existRefreshToken = await _userRefreshTokenRepository.Where(x => x.Code == refreshToken).SingleOrDefaultAsync();

            if (existRefreshToken == null)
            {
                return Response<TokenDto>.Fail("Refresh token not found", 404, true);
            }
            //Userı sistemden yakaladık.
            var user = await _userManager.FindByIdAsync(existRefreshToken.UserId);

            if (user == null)
            {
                return Response<TokenDto>.Fail("User Id not found", 404, true);
            }

            var token = _tokenservice.CreateToken(user);
            existRefreshToken.Code = token.RefreshToken;
            existRefreshToken.Expiration = token.RefreshTokenExpiration;
            await _unitOfWork.CommitAsync();
            return Response<TokenDto>.Success(token, 200);
        }

        

        public async Task<Response<NoDataDto>> RevokeRefreshTokenAsync(string refreshToken)
        {
            var existRefreshToken = await _userRefreshTokenRepository.Where(x => x.Code == refreshToken).SingleOrDefaultAsync();

            if (existRefreshToken == null)
            {
                return Response<NoDataDto>.Fail("Refresh token not found", 404, true);
            }

            _userRefreshTokenRepository.Remove(existRefreshToken);
            await _unitOfWork.CommitAsync();
            return Response<NoDataDto>.Success(200);
        }

       
    }
}
