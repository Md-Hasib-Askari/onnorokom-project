using AssignmentSystem.Application.DTOs.Auth;
using AssignmentSystem.Domain.Entities;
using AutoMapper;

namespace AssignmentSystem.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<AuthUser, UserListItemDto>();
    }
}
