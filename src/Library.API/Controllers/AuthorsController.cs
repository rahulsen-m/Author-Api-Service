using System;
using System.Collections.Generic;
using System.Linq;
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
        
        public  AuthorsController(ILibraryRepository libraryRepository, IMapper mapper, IPropertyMappingService propertyMappingService, ITypeHelperService typeHelperService)
        {
            _libraryRepository = libraryRepository;
            _mapper = mapper;
            _propertyMappingService = propertyMappingService;
            _typeHelperSevice = typeHelperService;
        }

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
            // var previousPageLink = authorFromRepository.HasPreviousPage ? CreateAuthorResourceUri(
            //     authorResourceParameters, ResourceUriType.PreviousPage
            // ) : null;

            // var nextPageLink = authorFromRepository.HasNextPage ? CreateAuthorResourceUri(
            //     authorResourceParameters, ResourceUriType.NextPage
            // ) : null;

            // Metadata
            var paginationMetadata = new
            {
                totalCount = authorFromRepository.TotalCount,
                pageSize = authorFromRepository.PageSize,
                currentPage = authorFromRepository.CurrentPage,
                totalPages = authorFromRepository.TotalPages
                // previousLink = previousPageLink,
                // nextLink = nextPageLink
            };
            // Adding custom header to the response
            Response.Headers.Add("X-Pagination",JsonConvert.SerializeObject(paginationMetadata));
            var authors = _mapper.Map<IEnumerable<AuthorDto>>(authorFromRepository);

            var links = CreateLinksForAuthors(authorResourceParameters,
                authorFromRepository.HasNextPage, authorFromRepository.HasPreviousPage);
            var shapedAuthors = authors.ShapeData(authorResourceParameters.Fields);
            var shapedAuthorsWithLinks = shapedAuthors.Select(author =>
            {
                var authorAsDictionary = author as IDictionary<string, object>;
                var authorLinks = CreateLinksForAuthor(
                    (Guid)authorAsDictionary["Id"], authorResourceParameters.Fields);

                authorAsDictionary.Add("links", authorLinks);

                return authorAsDictionary;
            });

            var linkedCollectionResource = new
            {
                value = shapedAuthorsWithLinks,
                links = links
            };

            return Ok(linkedCollectionResource);
        }

        [HttpGet("api/authors/{id}", Name = "GetAuthor")]
        public IActionResult GetAuthors(Guid id, [FromQuery] string fields){
            if (!_typeHelperSevice.TypeHasPropertires<AuthorDto>(fields))
            {
                return BadRequest();
            }
            var authorFromRepo = _libraryRepository.GetAuthor(id);
            if (authorFromRepo == null)
            {
                return NotFound();
            }
            var author = _mapper.Map<AuthorDto>(authorFromRepo);
            var link = CreateLinksForAuthor(id, fields);
            var linkedResourceToreturn = author.ShapeData(fields) as IDictionary<string, object>;
            linkedResourceToreturn.Add("links", link);
            return Ok(linkedResourceToreturn);
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
            // pass null as there we not need any data shapeing
            var links = CreateLinksForAuthor(authorEntity.Id, null);
            var linkedResourceToReturn = authorToReturn.ShapeData(null) as IDictionary<string, object>;
            linkedResourceToReturn.Add("links", links);
            return CreatedAtRoute("GetAuthor", new { id = authorToReturn.Id}, linkedResourceToReturn);
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
                case ResourceUriType.Current:
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

        private IEnumerable<LinkDto> CreateLinksForAuthor(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(this.Url.Link("GetAuthor", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(this.Url.Link("GetAuthor", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(this.Url.Link("DeleteAuthor", new { id = id }),
              "delete_author",
              "DELETE"));

            links.Add(
              new LinkDto(this.Url.Link("CreateBookForAuthor", new { authorId = id }),
              "create_book_for_author",
              "POST"));

            links.Add(
               new LinkDto(this.Url.Link("GetBooksForAuthor", new { authorId = id }),
               "books",
               "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForAuthors(
            AuthorResourceParameters authorsResourceParameters,
            bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateAuthorResourceUri(authorsResourceParameters,
               ResourceUriType.Current)
               , "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateAuthorResourceUri(authorsResourceParameters,
                  ResourceUriType.NextPage),
                  "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(
                    new LinkDto(CreateAuthorResourceUri(authorsResourceParameters,
                    ResourceUriType.PreviousPage),
                    "previousPage", "GET"));
            }

            return links;
        }

        #endregion
    }
}