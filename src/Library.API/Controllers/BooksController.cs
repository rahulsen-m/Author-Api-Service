using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Library.API.Controllers
{
    [ApiController]
    [Route("api/authors/{authorId}/books")]
    public class BooksController : ControllerBase
    {
        private IMapper _mapper;
        private ILogger<BooksController> _logger;
        private ILibraryRepository _libraryRepository;

        public BooksController(IMapper mapper, ILibraryRepository libraryRepository, ILogger<BooksController> logger)
        {
            _libraryRepository = libraryRepository;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet(Name = "GetBooksForAuthor")]
        public IActionResult GetBooksForAuthor(Guid authorId)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var booksFromRepository = _libraryRepository.GetBooksForAuthor(authorId);
            var books = _mapper.Map<IEnumerable<BookDto>>(booksFromRepository);
            // adding link in each books
            var booksForAuthor = books.Select(book => {
                book = CreateLinksForBook(book);
                return book;
            });
            var wrapper = new LinkedCollectionResourceWrapperDto<BookDto>(booksForAuthor);
            return Ok(CreateLinksForBooks(wrapper));
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
            // Added hypertext in the returned object
            return Ok(CreateLinksForBook(bookDetails));
        }

        [HttpPost(Name = "CreateBookForAuthor")]
        public IActionResult CreateBookForAuthor(Guid authorId,[FromBody] BookForCreationDto book)
        {
            if (book == null)
            {
                return BadRequest();
            }
            // Add custom rule and add that to the model state
            if (book.Title == book.Description)
            {
                ModelState.AddModelError(nameof(BookForCreationDto), "The description should be different from the book title.");
            }
            if (!ModelState.IsValid)
            {
                // retrun 422[Un-Processable entity]
                return new UnProcessableEntityObjectResult(ModelState);
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
            return CreatedAtRoute("GetBookForAuthor", new {authorId = bookToReturn.AuthorId, id = bookToReturn.Id}, CreateLinksForBook(bookToReturn));
        }

        [HttpDelete("{id}", Name = "DeleteBookForAuthor")]
        public IActionResult DeleteBookForAuthors(Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthorFromRepo == null)
            {
                return NotFound();
            }
            _libraryRepository.DeleteBook(bookForAuthorFromRepo);
            if (!_libraryRepository.Save())
            {
                throw new Exception($"Deleting book {id} for author {authorId} failed on save.");
            }

            //_logger.LogInformation(100, $"Deleting book {id} for author {authorId} is successful.");
            // This signify that the resource is deleted(204)
            return NoContent();
        }

        [HttpPut("{id}", Name = "UpdateBookForAuthor")]
        public IActionResult UpdateBookForAuthor(Guid authorId, Guid id, [FromBody] BookForUpdateDto book)
        {
            if (book == null)
            {
                return BadRequest();
            }
            if (book.Description == book.Title)
            {
                ModelState.AddModelError(nameof(BookForUpdateDto), "The description should be different from the book title.");
            }
            if (!ModelState.IsValid)
            {
                return new UnProcessableEntityObjectResult(ModelState);
            }
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthorFromRepo == null)
            {
                // if book is not found then adding the book to the author
                // This process is called upserting where consumer did a put request and server creates a resource without calling the post method
                var bookToAdd = _mapper.Map<Book>(book);
                bookToAdd.Id = id;
                _libraryRepository.AddBookForAuthor(authorId, bookToAdd);
                if (!_libraryRepository.Save())
                {
                    throw new Exception($"Upserting book {id} for author {authorId} failed to save.");
                }
                var bookToReturn = _mapper.Map<BookDto>(bookToAdd);
                return CreatedAtRoute("GetBookForAuthor", new {authorId = authorId, id = bookToReturn.Id}, bookToReturn);
            }
            // map to the dto
            // Apply the update 
            // Map back to entity
            _mapper.Map(book, bookForAuthorFromRepo);
            _libraryRepository.UpdateBookForAuthor(bookForAuthorFromRepo);
            if (!_libraryRepository.Save())
            {
                throw new Exception($"Updating book {id} for author {authorId} failed on save.");
            }
            return Ok();
        }

        [HttpPatch("{id}", Name = "PartiallyUpdateBookForAuthor")]
        public IActionResult PartiallyUpdateBookForAuthor(Guid authorId, Guid id, 
                [FromBody] JsonPatchDocument<BookForUpdateDto> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var booksFromRepository = _libraryRepository.GetBookForAuthor(authorId, id);
            if (booksFromRepository == null)
            {
                // return NotFound();
                // Upserting using patch method(Its just for demonstration purpose)
                var bookDto = new BookForUpdateDto();
                patchDocument.ApplyTo(bookDto, ModelState);
                if (bookDto.Description == bookDto.Title)
                {
                    ModelState.AddModelError(nameof(BookForUpdateDto), "The description must be different form the book title.");
                }
                TryValidateModel(bookDto);
                if (!ModelState.IsValid)
                {
                    return new UnProcessableEntityObjectResult(ModelState);
                }
                var bookToAdd = _mapper.Map<Book>(bookDto);
                bookToAdd.Id = id;
                _libraryRepository.AddBookForAuthor(authorId, bookToAdd);
                if (!_libraryRepository.Save())
                {
                    throw new Exception($"book {id} for author {authorId} failed to save.");
                }
                var bookToReturn = _mapper.Map<BookDto>(bookToAdd);
                return CreatedAtRoute("GetBookForAuthor", new {authorId= authorId, id = bookToReturn.Id}, bookToReturn);
            }
            // mapped book to BookForUpdateDto
            var bookToPatch = _mapper.Map<BookForUpdateDto>(booksFromRepository);
            // apply the patch instruction to the BookForUpdateDto object 
            patchDocument.ApplyTo(bookToPatch, ModelState);
            if(bookToPatch.Description == bookToPatch.Title)
            {
                ModelState.AddModelError(nameof(BookForUpdateDto), "The description must be different form the book title.");
            }
            TryValidateModel(bookToPatch);
            if (!ModelState.IsValid)
            {
                return new UnProcessableEntityObjectResult(ModelState);
            }
            // apply the bookToPatch changes to booksFromRepository to save in the db
            _mapper.Map(bookToPatch, booksFromRepository);
            _libraryRepository.UpdateBookForAuthor(booksFromRepository);
            if (!_libraryRepository.Save())
            {
                throw new Exception($"Patching book {id} for author {authorId} is failed to save.");
            }
            return Ok();
        }

        // Add links to the specific book
        private BookDto CreateLinksForBook(BookDto book)
        {
            // link to self
            book.Links.Add(new LinkDto(this.Url.Link("GetBookForAuthor", 
                new {id = book.Id}),
                "self",
                "GET"));
            
            // link to delete
            book.Links.Add(
                new LinkDto(this.Url.Link("DeleteBookForAuthor", 
                new { id = book.Id }),
                "delete_book",
                "DELETE"));

            // link to full update
            book.Links.Add(
                new LinkDto(this.Url.Link("UpdateBookForAuthor", 
                new { id = book.Id }),
                "update_book",
                "PUT"));

            // link to partially update
            book.Links.Add(
                new LinkDto(this.Url.Link("PartiallyUpdateBookForAuthor", 
                new { id = book.Id }),
                "partially_update_book",
                "PATCH"));
            return book;
        }

        // Wrap the books response in a value property
        private LinkedCollectionResourceWrapperDto<BookDto> CreateLinksForBooks(
            LinkedCollectionResourceWrapperDto<BookDto> booksWrapper)
        {
            // link to self
            booksWrapper.Links.Add(
                new LinkDto(this.Url.Link("GetBooksForAuthor", new { }),
                "self",
                "GET"));

            return booksWrapper;
        }
    }
}