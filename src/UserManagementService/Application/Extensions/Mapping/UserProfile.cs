using AutoMapper;
using UserManagementService.Application.DTOs;
using UserManagementService.Domain.Models;

namespace UserManagementService.Application.Extensions.Mapping;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<ShortUserInfo, ShortUserInfoDto>();
    }
}