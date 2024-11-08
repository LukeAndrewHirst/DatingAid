using System.Globalization;
using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            

            CreateMap<AppUser, MemberDto>()
                .ForMember(d => d.PhotoUrl, o => o.MapFrom(src => src.Photos.FirstOrDefault(x => x.IsMain).Url))
                .ForMember(d => d.Age, o => o.MapFrom(s => s.DateOfBirth.CalculateAge()));
            CreateMap<Photo, PhotoDto>();
            CreateMap<MemberUpdateDto, AppUser>();
            CreateMap<RegisterDto, AppUser>();
            CreateMap<string, DateOnly>().ConvertUsing(s => DateOnly.Parse(s.Replace("Z", "").Replace("T", " ").Substring(0, 10)));
        }
    }
}