using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Speller.SpellingBox.Services;
using Speller.Presentation.Web.Api.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

namespace Speller.Presentation.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpellerController : ControllerBase
    {
        public readonly ILogger<SpellerController> _logger;
        public readonly IMachineLearningService _machineLearningService;
        public readonly IConfiguration _configuration;

        public SpellerController(ILogger<SpellerController> logger,
            IMachineLearningService machineLearningService,
            IConfiguration configuration)
        {
            this._logger = logger;
            this._machineLearningService = machineLearningService;
            this._configuration = configuration;
        }

        // GET api/speller
        [HttpGet]
        public ActionResult<string> Get()
        {   
            return new OkObjectResult("It is working.");
        }

        // POST api/speller
        [HttpPost]
        public ActionResult Post([FromBody] SpellingRequest request, [FromQuery] string machineLearning, [FromServices] ISpellingService spellingService)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }

            if (machineLearning != null)
            {
                #region Machine Learning Service configuration
                var machineLearningConfigurationSection = _configuration.GetSection("MachineLearningEndpointSettings");

                try
                {
                    this._machineLearningService.AddEndpoint(
                        new Uri(machineLearningConfigurationSection.GetSection(machineLearning).GetValue<string>("Url")),
                        machineLearningConfigurationSection.GetSection(machineLearning).GetValue<string>("apiKey"));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Machine Learning configuration with name '{machineLearning}' not found");

                    return BadRequest(new ArgumentOutOfRangeException("machineLearning"));
                }
                #endregion
            }

            var result = spellingService.SuggestCorrectionWithMachineLearning(request.Dictionary, request.Words.ToList(), machineLearning);

            result.Wait();

            return new OkObjectResult(result.Result);
        }
    }
}
