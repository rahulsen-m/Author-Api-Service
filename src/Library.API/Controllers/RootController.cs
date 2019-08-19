using System.Collections.Generic;
using Library.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class RootController : ControllerBase
    {

        public RootController()
        {
        }

        [HttpGet(Name = "GetRoot")]
        public IActionResult GetRoot([FromHeader(Name = "Accept")] string mediaType)
        {
            if (mediaType == "application/vnd.rahul.hateoas+json")
            {
                var links = new List<LinkDto>();

                links.Add(
                  new LinkDto(this.Url.Link("GetRoot", new { }),
                  "self",
                  "GET"));
    
                links.Add(
                 new LinkDto(this.Url.Link("GetAuthors", new { }),
                 "authors",
                 "GET"));

                links.Add(
                  new LinkDto(this.Url.Link("CreateAuthor", new { }),
                  "create_author",
                  "POST"));

                return Ok(links);
            }

            return NoContent();
        }
    }
}