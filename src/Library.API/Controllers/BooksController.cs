using System;
using System.Collections.Generic;
using AutoMapper;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [ApiController]
    [Route("api/authors/{authorId}/books")]
    public class BooksController : ControllerBase
    {
        private IMapper _mapper;
        private ILibraryRepository _libraryRepository;

        public BooksController(IMapper mapper, ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
            _mapper = mapper;
        }

        [HttpGet()]
        public IActionResult GetBooksForAuthor(Guid authorId)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var booksFromRepository = _libraryRepository.GetBooksForAuthor(authorId);
            var books = _mapper.Map<IEnumerable<BookDto>>(booksFromRepository);
            return Ok(books);
        }

        [HttpGet("{Id}")]
        public IActionResult GetBookForAuthor(Guid authorId,Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var booksFromRepository = _libraryRepository.GetBookForAuthor(authorId, id);
            if (booksFromRepository == null)
            {
                return NotFound();
            }
            var bookDetails = _mapper.Map<BookDto>(booksFromRepository);
            return Ok(bookDetails);
        }
    }
}