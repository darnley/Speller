using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Speller.Presentation.Web.Api.Models
{
    public class SpellingRequest : Request
    {
        public IEnumerable<string> Dictionary { get; set; }
        public IEnumerable<string> Words { get; set; }
    }
}
