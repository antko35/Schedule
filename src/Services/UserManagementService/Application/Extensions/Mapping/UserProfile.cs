using AutoMapper;
using UserManagementService.Application.DTOs;
using UserManagementService.Domain.Models;

namespace UserManagementService.Application.Extensions.Mapping;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, ShortUserInfoDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName))
            .ForMember(d => d.LastName, o => o.MapFrom(s => s.LastName))
            .ForMember(d => d.Patronymic, o => o.MapFrom(s => s.Patronymic));
    }
}