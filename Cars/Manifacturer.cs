using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cars
{
    public class Manifacturer
    {
        public string Name { get; set; }
        public string Headquarters { get; set; }
        public int Year { get; set; }

        internal static Manifacturer ParseFromCsv(string line)
        {
            var columns = line.Split(',');
            return new Manifacturer
            {
                Name = columns[0],
                Headquarters = columns[1],
                Year = Int32.Parse(columns[2])
            };
        }
    }
}
