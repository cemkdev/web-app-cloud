using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebAppAPI.API.Contracts.Files;
using WebAppAPI.Application.Options.Storage;

namespace WebAppAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController(IOptions<BaseStorageOptions> baseStorageOptions) : ControllerBase
    {
        [HttpGet("get-base-storage-url")]
        public ActionResult<BaseStorageUrlResponse> GetBaseStorageUrl()
        {
            BaseStorageUrlResponse response = new()
            {
                Url = baseStorageOptions.Value.Url
            };

            return Ok(response);
        }
    }
}
