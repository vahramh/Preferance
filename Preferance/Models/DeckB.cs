using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Preferance.Models
{   
    public class CardB
    {
        public string Id { get; set; }
        public string Colour { get; set; }
        public string Value { get; set; }
        public int Seniority { get; set; }
        public int SeniorityTrump { get; set; }
        public string HandBId { get; set; }
        public int Sequence { get; set; }
    }
}
