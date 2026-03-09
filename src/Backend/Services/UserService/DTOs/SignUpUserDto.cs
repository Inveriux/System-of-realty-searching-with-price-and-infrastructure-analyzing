using AutoMapper;
using UserService.Models;

namespace UserService.DTOs;

public record SignUpUserDto(
    string Name,
    string Email,
    string Password,
    string? AccountImageUrl
)
{
    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<SignUpUserDto, User>()
                .ConstructUsing(dto => User.Create(dto.Name, dto.Email, dto.AccountImageUrl));
        }
    }
};
