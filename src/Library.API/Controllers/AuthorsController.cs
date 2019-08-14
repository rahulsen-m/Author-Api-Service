using System;
using System.Collections.Generic;
using AutoMapper;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Library.API.Controllers
{
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        // Create a field to store the mapper object
        private readonly IMapper _mapper;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly ITypeHelperService _typeHelperSevice;
        private ILibraryRepository _libraryRepository;
        
        /// <summary>
        /// Author constructor
        /// </summary>
        /// <param name="libraryRepository">libraryRepository</param>
        /// <param name="mapper">automapper</param>
        /// <param name="propertyMappingService">property mapping service</param>
        /// <param name="typeHelperService">type helper service</param>
        public  AuthorsController(ILibraryRepository libraryRepository, IMapper mapper, IPropertyMappingService propertyMappingService, ITypeHelperService typeHelperService)
        {
            _libraryRepository = libraryRepository;
            _mapper = mapper;
            _propertyMappingService = propertyMappingService;
            _typeHelperSevice = typeHelperService;
        }
        
        /// <summary>
        /// Get the authors 
        /// </summary>
        /// <param name="authorResourceParameters">author resource parameter</param>
        /// <returns>All author details</returns>
        [HttpGet("api/authors", Name = "GetAuthors")]
        //[FromQuery] int pageNumber, [FromQuery] int pageSize = 10 can be used as parameter
        public IActionResult GetAuthors([FromQuery] AuthorResourceParameters authorResourceParameters){
            if (!_propertyMappingService.ValidMappingExistsFor<AuthorDto, Author>
               (authorResourceParameters.OrderBy))
            {
                return BadRequest();
            }
            if (!_typeHelperSevice.TypeHasPropertires<AuthorDto>(authorResourceParameters.Fields))
            {
                return BadRequest();
            }
            var authorFromRepository = _libraryRepository.GetAuthors(authorResourceParameters);
            var previousPageLink = authorFromRepository.HasPreviousPage ? CreateAuthorResourceUri(
                authorResourceParameters, ResourceUriType.PreviousPage
            ) : null;

            var nextPageLink = authorFromRepository.HasNextPage ? CreateAuthorResourceUri(
                authorResourceParameters, ResourceUriType.NextPage
            ) : null;

            // Metadata
            var paginationMetadata = new
            {
                totalCount = authorFromRepository.TotalCount,
                pageSize = authorFromRepository.PageSize,
                currentPage = authorFromRepository.CurrentPage,
                totalPages = authorFromRepository.TotalPages,
                previousLink = previousPageLink,
                nextLink = nextPageLink
            };
            // Adding custom header to the response
            Response.Headers.Add("X-Pagination",JsonConvert.SerializeObject(paginationMetadata));
            var authors = _mapper.Map<IEnumerable<AuthorDto>>(authorFromRepository);
            return Ok(authors.ShapData(authorResourceParameters.Fields));
        }

        [HttpGet("api/authors/{id}", Name = "GetAuthor")]
        public IActionResult GetAuthors(Guid Id, [FromQuery] string fields){
            if (!_typeHelperSevice.TypeHasPropertires<AuthorDto>(fields))
            {
                return BadRequest();
            }
            var authorFromRepo = _libraryRepository.GetAuthor(Id);
            if (authorFromRepo == null)
            {
                return NotFound();
            }
            var author = _mapper.Map<AuthorDto>(authorFromRepo);
            return Ok(author.ShapeData(fields));
        }

        [HttpPost("api/authors")]
        public IActionResult CreateAuthor([FromBody] AuthorForCreationDto author)
        {
            if (author == null)
            {
                return BadRequest();
            }
            var authorEntity = _mapper.Map<Author>(author);
            _libraryRepository.AddAuthor(authorEntity);
            if (!_libraryRepository.Save())
            {
                return StatusCode(500, "A problem occurred while createing the author.");
            }
            var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);
            return CreatedAtRoute("GetAuthor", new { id = authorToReturn.Id}, authorToReturn);
        }

        [HttpPost("api/authors/{id}")]
        public IActionResult BlockAuthorCreation(Guid id)
        {
            if (_libraryRepository.AuthorExists(id))
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }
            return NotFound();
        }

        [HttpDelete("api/authors/{id}")]
        public IActionResult DeleteAuthor(Guid id)
        {
            var authorFromRepo = _libraryRepository.GetAuthor(id);
            if (authorFromRepo == null)
            {
                return NotFound();
            }
            // cascade delete(Delete the associated books)
            _libraryRepository.DeleteAuthor(authorFromRepo);
            if (!_libraryRepository.Save())
            {
                throw new Exception($"Deleting author {id} failed on save.");
            }
            // This return 204 which shows that the the content is deleted.
            return NoContent();
        }

        #region Private Methods

        private string CreateAuthorResourceUri(AuthorResourceParameters authorResourceParameter, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return this.Url.Link("GetAuthors",
                    new {
                        fields = authorResourceParameter.Fields,
                        orderBy = authorResourceParameter.OrderBy,
                        searchQuery = authorResourceParameter.SearchQuery,
                        genre = authorResourceParameter.Genre,
                        PageNumber = authorResourceParameter.PageNumber - 1,
                        PageSize = authorResourceParameter.PageSize
                    });
                case ResourceUriType.NextPage:
                    return this.Url.Link("GetAuthors",
                    new {
                        fields = authorResourceParameter.Fields,
                        orderBy = authorResourceParameter.OrderBy,
                        searchQuery = authorResourceParameter.SearchQuery,
                        genre = authorResourceParameter.Genre,
                        PageNumber = authorResourceParameter.PageNumber + 1,
                        PageSize = authorResourceParameter.PageSize
                    });
                default:
                    return this.Url.Link("GetAuthors",
                    new {
                        fields = authorResourceParameter.Fields,
                        orderBy = authorResourceParameter.OrderBy,
                        searchQuery = authorResourceParameter.SearchQuery,
                        genre = authorResourceParameter.Genre,
                        PageNumber = authorResourceParameter.PageNumber,
                        PageSize = authorResourceParameter.PageSize
                    });
            }
        }

        #endregion
    }
}