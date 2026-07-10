using Microsoft.AspNetCore.Mvc;
using Chatbot.Services;
using Chatbot.Models;
using System.Text.Json;

namespace Chatbot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiseasesController : ControllerBase
    {
        private readonly IDiseaseService _service;

        public DiseasesController(IDiseaseService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var list = _service.GetAll();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            var el = _service.GetById(id);
            if (el == null) return NotFound();
            return Ok(el);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Disease body)
        {
            _service.Add(body);
            return Created("/api/diseases/", body);
        }

        [HttpPost("batch")]
        public IActionResult CreateBatch([FromBody] List<Disease> body)
        {
            if (body == null || body.Count == 0) return BadRequest("Empty list");
            foreach (var d in body)
            {
                _service.Add(d);
            }
            return Created("/api/diseases/batch", body);
        }

        [HttpPut("{id}")]
        public IActionResult Update(string id, [FromBody] Disease body)
        {
            var ok = _service.Update(id, body);
            if (!ok) return NotFound();
            return Ok(body);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var ok = _service.Delete(id);
            if (!ok) return NotFound();
            return Ok();
        }

        [HttpGet("search-by-symptoms")]
        public IActionResult SearchBySymptoms([FromQuery] string q, [FromQuery] int k = 3)
        {
            if (string.IsNullOrWhiteSpace(q)) return BadRequest("Query required");
            var list = _service.FindTopKBySymptoms(q, k);
            var outList = list.Select(t => new { disease = t.Disease, score = t.Score }).ToList();
            return Ok(outList);
        }
    }
}
