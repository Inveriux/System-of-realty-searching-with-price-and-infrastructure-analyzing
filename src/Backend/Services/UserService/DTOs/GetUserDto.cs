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
    public required string Role { get; set; }


    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<User, GetUserDto>();    
        }
    }
}
