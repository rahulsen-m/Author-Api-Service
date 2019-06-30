using System;
using System.Collections.Generic;
using AutoMapper;
using Library.API.Entities;
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

        [HttpGet("{Id}", Name = "GetBookForAuthor")]
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

        [HttpPost]
        public IActionResult CreateBookForAuthor(Guid authorId,[FromBody] BookForCreationDto book)
        {
            if (book == null)
            {
                return BadRequest();
            }
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var bookEntity = _mapper.Map<Book>(book);
            _libraryRepository.AddBookForAuthor(authorId, bookEntity);
            if (!_libraryRepository.Save())
            {
                return StatusCode(500, "Error Occurred while adding the book for the author.");
            }
            var bookToReturn = _mapper.Map<BookDto>(bookEntity);
            return CreatedAtRoute("GetBookForAuthor", new {authorId = bookToReturn.AuthorId, id = bookToReturn.Id}, bookToReturn);
        }
    }
}