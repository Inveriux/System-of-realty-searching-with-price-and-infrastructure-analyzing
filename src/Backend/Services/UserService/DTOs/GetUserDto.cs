using UserService.Models;
using AutoMapper;

namespace UserService.DTOs;

public class GetUserDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; } 
    public required string Email { get; init; } 
    public string? AccountImageUrl { get; init; }
    public string? PhoneNumber { get; init; }
    public bool EmailConfirmed { get; init; }
    public bool HasPassword { get; init; }
    public bool IsGoogleAccount { get; init; }
    public required string Role { get; set; }


    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<User, GetUserDto>()
                .ForMember(x => x.HasPassword, y => y.MapFrom(s => !string.IsNullOrWhiteSpace(s.PasswordHash)))
                .ForMember(x => x.IsGoogleAccount, y => y.MapFrom(s => s.GoogleAuthentication != null));    
        }
    }
}
