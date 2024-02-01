using Microsoft.AspNetCore.Mvc;
using PartUpload.Dto;

namespace PartUpload.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MultiPartUploadFileTestController : ControllerBase
    {
        private readonly ILogger<MultiPartUploadFileTestController> _logger;

        public MultiPartUploadFileTestController(ILogger<MultiPartUploadFileTestController> logger)
        {
            _logger = logger;
        }
        [HttpPost()]
        public async Task<string> MultiPartUploadFileTest(
            [FromForm] MultiPartUploadFileTestInput input)
        {
            return "Ö´ÐÐÍê³É";
        }
    }
}
