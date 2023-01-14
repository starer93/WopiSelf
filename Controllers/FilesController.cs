using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wopi.FileProvider;
using Wopi.Http;
using Wopi.Http.Results;
using Wopi.Models;

namespace Wopi.Controllers
{
    [ApiController]
    [Route("wopi/[controller]")]
    public class FilesController : Controller
    {
        private readonly IHostEnvironment _appEnvironment;
        private WopiFileSystemProvider fileProvider;
  
        public FilesController(IHostEnvironment appEnvironment)
        {
            _appEnvironment = appEnvironment;
            fileProvider =  new WopiFileSystemProvider(_appEnvironment);
        }

       
        private HostCapabilities HostCapabilities => new()
        {
            SupportsCobalt = false,
            SupportsGetLock = true,
            SupportsLocks = true,
            SupportsExtendedLockLength = true,
            SupportsFolders = true,//?
            SupportsCoauth = true,//?
            SupportsUpdate = true //TODO: PutRelativeFile - usercannotwriterelative
        };

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCheckFileInfo(string id)
        {
          
            return new JsonResult(fileProvider.GetWopiFile(id)?.GetCheckFileInfo(User, HostCapabilities), null);
        }

        /// <summary>
        /// Returns contents of a file specified by an identifier.
        /// Specification: https://learn.microsoft.com/en-us/microsoft-365/cloud-storage-partner-program/rest/files/getfile
        /// Example URL path: /wopi/files/(file_id)/contents
        /// </summary>
        /// <param name="id">File identifier.</param>
        /// <returns></returns>
        [HttpGet("{id}/contents")]
        public async Task<IActionResult> GetFile(string id)
        {
            // Get file
            var file = fileProvider.GetWopiFile(id);

            // Check expected size
            var maximumExpectedSize = HttpContext.Request.Headers[WopiHeaders.MAX_EXPECTED_SIZE].ToString().ToNullableInt();
            if (maximumExpectedSize is not null && file.GetCheckFileInfo(User, HostCapabilities).Size > maximumExpectedSize.Value)
            {
                return new PreconditionFailedResult();
            }

            // Try to read content from a stream
            return new FileStreamResult(file.GetReadStream(), "application/octet-stream");
        }
    }
}
