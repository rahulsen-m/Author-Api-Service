using System.Collections.Generic;
using AutoMapper;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        // Create a field to store the mapper object
        private readonly IMapper _mapper;
        private ILibraryRepository _libraryRepository;

        public AuthorsController(ILibraryRepository libraryRepository, IMapper mapper)
        {
            _libraryRepository = libraryRepository;
            _mapper = mapper;
        }

        [HttpGet("api/authors")]
        public IActionResult GetAuthors(){
            var authorFromRepository = _libraryRepository.GetAuthors();
            //var authors = new List<AuthorDto>();

            //without using the automapper
            // foreach (var author in authorFromRepository)
            // {
            //     authors.Add(new AuthorDto(){
            //         Id = author.Id,
            //         Name = $"{author.FirstName} {author.LastName}",
            //         Genre = author.Genre,
            //         Age = author.DateOfBirth.GetCurrentAge()
            //     });                
            // }
            
            var authors = _mapper.Map<IEnumerable<AuthorDto>>(authorFromRepository);

            return new JsonResult(authors);
        }
    }
}