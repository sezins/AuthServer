using Auth.Core.DTOs;
using Auth.Core.Entities;
using AutoMapper;

namespace Auth.Service
{
    public class DtoMapper:Profile
    {
        public DtoMapper()
        {
            CreateMap<ProductDto,Product>().ReverseMap();
            CreateMap<UserAppDto,UserApp>().ReverseMap();
        }
    }
}