using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Speller.SpellingBox.Models
{
    public class Results
    {

        [JsonProperty("output1")]
        public List<string> Output { get; set; }
    }

    public class MachineLearningResponse
    {

        [JsonProperty("Results")]
        public Results Results { get; set; } = new Results();
    }
}
