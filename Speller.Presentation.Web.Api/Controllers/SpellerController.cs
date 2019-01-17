using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Speller.SpellingBox.Services;
using Speller.Presentation.Web.Api.Models;
using Microsoft.Extensions.Logging;

namespace Speller.Presentation.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpellerController : ControllerBase
    {
        public readonly ILogger<SpellerController> _logger;
        public readonly IMachineLearningService _machineLearningService;

        public SpellerController(ILogger<SpellerController> logger,
            IMachineLearningService machineLearningService)
        {
            this._logger = logger;
            this._machineLearningService = machineLearningService;
        }

        // GET api/speller
        [HttpGet]
        public ActionResult<string> Get()
        {   
            return new OkObjectResult("It is working.");
        }

        // POST api/speller
        [HttpPost]
        public ActionResult Post([FromBody] SpellingRequest request, [FromServices] ISpellingService spellingService)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }

            this._logger.LogInformation($"Received request with {request.Dictionary.Count()} words in dictionary and {request.Words.LongCount()} words to be verified");

            spellingService.AddDictionary(request.Dictionary);

            var result = spellingService.SuggestCorrection(request.Words.ToList());

            result.Wait();

            if (this._machineLearningService.HasIndexes())
            {
                this._machineLearningService.CorrectWordsByIndexAsync(result.Result).Wait();
            }

            return new OkObjectResult(result.Result);
        }
    }
}
