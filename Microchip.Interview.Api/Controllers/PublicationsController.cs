using Microsoft.AspNetCore.Mvc;
using Microchip.Interview.Api.Api;
using Microchip.Interview.Api.Domain;

namespace Microchip.Interview.Api.Controllers
{
    [ApiController]
//    [Route("[controller]")]
    [Route("publications")] 
    public sealed class PublicationsController : ControllerBase
    {
        private readonly PublicationService _service;

        public PublicationsController(PublicationService service)
        {
            _service = service;
        }

        // GET /publications?title=controller&isbn=978&pageNumber=1&pageSize=10&sortBy=title,isbn&sortDir=asc
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] PublicationQuery query)
        {
            var (items, total) = await _service.SearchAsync(query);

            var dtoItems = items.Select(PublicationMappers.ToListItem).ToList();

            var result = new
            {
                total,
                pageNumber = query.PageNumber,
                pageSize = query.PageSize,
                totalPages = (int)Math.Ceiling((double)total / query.PageSize),
                items = dtoItems
            };

            return Ok(result);
        }

        // GET /publications/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var p = await _service.GetDetailsAsync(id);
            if (p is null)
                return NotFound();

            return Ok(PublicationMappers.ToDetails(p));
        }

        // GET /publications/{id}/versions
        [HttpGet("{id}/versions")]
        public async Task<IActionResult> GetVersions(Guid id)
        {
            var p = await _service.GetDetailsAsync(id.ToString());
            if (p is null)
                return NotFound();

            var versions = p.Versions
                .Select(PublicationMappers.ToVersion)
                .ToList();
            
            return Ok(p.Versions);
        }
    }
}
