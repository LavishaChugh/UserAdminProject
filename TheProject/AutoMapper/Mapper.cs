using AutoMapper;
using TheProject.Dtos;
using TheProject.Services;

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
            CreateMap<User, GetUserDto>();
            CreateMap<UpdateUserDto, User>();

            //CreateMap<Users, GetUserDto>()
            //.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            //.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            //.ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            //.ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.Image))
            //.ReverseMap();
        }
    }
}
