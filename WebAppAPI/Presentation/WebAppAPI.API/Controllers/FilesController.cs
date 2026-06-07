using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebAppAPI.Application.Options.Storage;

namespace WebAppAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        readonly BaseStorageOptions _baseStorageOptions;

        public FilesController(IOptions<BaseStorageOptions> baseStorageOptions)
        {
            _baseStorageOptions = baseStorageOptions.Value;
        }

        [HttpGet("get-base-storage-url")]
        public IActionResult GetBaseStorageUrl()
        {
            return Ok(new
            {
                Url = _baseStorageOptions.Url
            });
        }
    }
}
