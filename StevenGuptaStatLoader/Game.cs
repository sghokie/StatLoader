using System.Collections.Generic;

namespace StevenGuptaStatLoader
{
    class Game
    {
     /*
      * Class file related to all things needed from the Box Score API
      */  
       
        public class Teams
        {
            public Away away { get; set; }
            public Home home { get; set; }

        }

        public class Away
        {
            public Team team { get; set; }
            public TeamStats teamStats { get; set; }
        }
        public class Home
        {
            public Team team { get; set; }
            public TeamStats teamStats { get; set; }

        }
        public class Team
        {
            public string name { get; set; }
        }

        public class Info
        {
            public string label { get; set; }
            public string value { get; set; }
         

        }

        public class TeamStats
        {
            
            public Batting batting { get;set;}
            public Fielding fielding { get; set; }
        }

        public class Batting
        {
            public int runs { get; set; }
            public int hits { get; set; }
        }

        public class Fielding
        {
            public int errors { get; set; }
        }
      
        public class GameInfoCollection
        {
            public List<Info> info { get; set; }
            public Teams teams { get; set; }
        }

 
    }
}
