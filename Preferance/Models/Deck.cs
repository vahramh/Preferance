using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Preferance.Models
{   
    public class Card
    {
        public string Id { get; set; }
        public string Colour { get; set; }
        public string Value { get; set; }
        public int Seniority { get; set; }
        public string HandId { get; set; }
        public int Sequence { get; set; }
    }
}
