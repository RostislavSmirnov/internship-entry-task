using AutoMapper;
using X0Game.DTOs;
using X0Game.Models;
namespace X0Game.MappingProfile
{
    public class MapperProfile : Profile
    {
        public MapperProfile() 
        {
            CreateMap<GameStartModelDTO, Game>();
            CreateMap<Game, GameStartModelDTO>();

            CreateMap<Game, GameShowDTO>()
            .ForMember(dest => dest.Field, opt => opt.MapFrom(src => src.Field))
            .ForMember(dest => dest.Version, opt => opt.MapFrom(src =>src.Version.ToString()));

            CreateMap<GameShowDTO, Game>()
                .ForMember(dest => dest.Field, opt => opt.MapFrom(src => src.Field))
                .ForMember(dest => dest.Version, opt => opt.MapFrom(src => Convert.FromBase64String(src.Version)));
        }
    }
}
