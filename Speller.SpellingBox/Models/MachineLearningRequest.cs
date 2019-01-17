using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Speller.SpellingBox.Models
{
    public class Inputs
    {

        [JsonProperty("input1")]
        public List<string> Input1 { get; set; }
    }

    public class GlobalParameters
    {
    }

    public class MachineLearningRequest
    {

        [JsonProperty("Inputs")]
        public Inputs Inputs { get; set; } = new Inputs();

        [JsonProperty("GlobalParameters")]
        public GlobalParameters GlobalParameters { get; set; }
    }


}
