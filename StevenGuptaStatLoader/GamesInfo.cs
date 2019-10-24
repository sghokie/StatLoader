using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StevenGuptaStatLoader
{

    public class DateCollection
    {
        public List<Dates> dates { get; set; }
    }

    public class Dates
    {
        public string date { get; set; }
        public List<Games> games { get; set; }
    }


    public class Games
    {
        public int gamePK { get; set; }
    }

}
