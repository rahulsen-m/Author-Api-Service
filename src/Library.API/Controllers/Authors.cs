using Library.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Authors : ControllerBase
    {
        private ILibraryRepository _libraryRepository;

        public Authors(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpGet]
        public IActionResult GetAuthors(){
            var authorFromRepository = _libraryRepository.GetAuthors();
            return new JsonResult(authorFromRepository);
        }
    }
}