using AutoMapper;
using TheProject.Dtos;

namespace TheProject.AutoMapper
{
    public class Mapper : Profile
    {
        public Mapper()
        {
            CreateMap<Users, GetUserDto>();
            CreateMap<GetUserDto, Users>();
            CreateMap<AddUserDto, Users>();
            CreateMap<Users, AddUserDto>();
            CreateMap<Users, GetUserDto>()
    .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
    .ForMember(dest => dest.Dob, opt => opt.MapFrom(src => src.Dob));

            CreateMap<GetUserDto, AddUserDto>();
            CreateMap<AddUserDto, GetUserDto>();
        }
    }
}
