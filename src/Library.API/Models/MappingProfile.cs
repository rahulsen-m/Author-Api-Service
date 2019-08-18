using AutoMapper;
using Library.API.Entities;
using Library.API.Helpers;

namespace Library.API.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile() {
            // Add as many of these lines as you need to map your objects
            CreateMap<Author, AuthorDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => 
                $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => 
                src.DateOfBirth.GetCurrentAge(src.DateOfDeath)));
            CreateMap<Book, BookDto>();
            CreateMap<AuthorForCreationDto, Author>();
            CreateMap<AuthorForCreationWithDateOfDeathDto, Author>();
            CreateMap<BookForCreationDto, Book>();
            CreateMap<BookForUpdateDto, Book>();
            CreateMap<Book, BookForUpdateDto>();
        }
    }
}