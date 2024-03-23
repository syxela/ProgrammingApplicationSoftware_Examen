using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ex
{
    public class House
    {
        public string slug { get; set; }
        public string name { get; set; }
    }

    public class Character
    {
        public string name { get; set; }
        public string slug { get; set; }
        public House house { get; set; }
        public List<string> quotes { get; set; }
    }

    public class CharacterImage
    {
        public string fullName { get; set; }
        public string imageUrl { get; set; }
    }
}
