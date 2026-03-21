using AutoMapper;
using UserService.Models;

namespace UserService.DTOs;

public class SellerPublicProfileDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public decimal? Rating { get; init; }
    public int? ReviewsCount { get; init; }
    public bool? IsVerified { get; init; }
    public string? AgencyName { get; init; }

    private sealed class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<User, SellerPublicProfileDto>()
                .ForMember(x => x.FullName, y => y.MapFrom(s => s.Name))
                .ForMember(x => x.AvatarUrl, y => y.MapFrom(s => s.AccountImageUrl))
                .ForMember(x => x.Phone, y => y.MapFrom(s => s.PhoneNumber))
                .ForMember(x => x.Rating, y => y.MapFrom(_ => (decimal?)null))
                .ForMember(x => x.ReviewsCount, y => y.MapFrom(_ => (int?)null))
                .ForMember(x => x.IsVerified, y => y.MapFrom(_ => (bool?)null))
                .ForMember(x => x.AgencyName, y => y.MapFrom(_ => (string?)null));
        }
    }
}
