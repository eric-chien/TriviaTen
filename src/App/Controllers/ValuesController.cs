using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    /// <summary>
    ///     Test Controller
    /// </summary>
    [ApiVersion(Version)]
    [Route("v{version:apiVersion}/test")]
    [ApiController]
    [AllowAnonymous]
    public class ValuesController : ControllerBase
    {
        private const string Version = "1";

        /// <summary>
        ///     Test Get endpoint
        /// </summary>
        /// <response code="200">If the values have been found</response>
        /// <returns>
        ///     Test Values
        /// </returns>
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            return await Task.FromResult(new string[] { "value1", "value2" });
        }
        
    }
}
