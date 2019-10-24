using System.Collections.Generic;


namespace StevenGuptaStatLoader
{
    /*
     * Class file for all things related to the Play By Play API
     */ 

    public class AllPlaysCollection
    {
        public List<Play> allPlays { get; set; }
    }


    public class PlayEventsCollection
    {
        public Details details { get; set; }
        public PitchData pitchData { get; set; }
        public int pitchNumber { get; set; }
        public HitData hitData { get; set; }
        public bool isPitch { get; set; }

        public class Details
        {
            public string description { get; set; }
            public bool isinPlay { get; set; }
            public bool isStrike { get; set; }
            public bool isBall { get; set; }
            public Call call { get; set; }
            public Type type { get; set; }


            public class Call
            {
                public string code { get; set; }
                public string description { get; set; }
            }

            public class Type
            {
                public string code { get; set; }
                public string description { get; set; }
            }

        }

        public class PitchData
        {
            public decimal startSpeed { get; set; }
            public decimal endSpeed { get; set; }
            public Coordinates coordinates { get; set; }
            public Breaks breaks { get; set; }

            public class Coordinates
            {
                public decimal aY { get; set; }
                public decimal aZ { get; set; }
                public decimal pfxX { get; set; }
                public decimal pfxZ { get; set; }
                public decimal pX { get; set; }
                public decimal pZ { get; set; }
                public decimal x { get; set; }
                public decimal y { get; set; }

            }

            public class Breaks
            {
                public decimal breakAngle { get; set; }
                public decimal breakLength { get; set; }
                public decimal breakY { get; set; }
                public decimal spinRate { get; set; }
                public decimal spinDirection { get; set; }
            }
        }

        public class HitData
        {
            public Coordinates coordinates { get; set; }

            public decimal launchSpeed { get; set; }
            public decimal launchAngle { get; set; }
            public decimal totalDistance { get; set; }
            public int location { get; set; }

            public class Coordinates
            {
                public decimal coordX { get; set; }
                public decimal coordY { get; set; }

            }

        }
    }

    
    public class Play
    {
        public Result result { get; set; }
        public MatchUp matchup { get; set; }
        public List<PlayEventsCollection> playEvents { get; set; }
        public int atBatIndex { get; set; }

        public class Result
        {

            public string type { get; set; }
            public string eventType { get; set; }
            public string description { get; set; }
            public string Event { get; set; }

        }

        public class MatchUp
        {
            public Batter batter { get; set; }
            public Pitcher pitcher { get; set; }
            public BatSide batSide { get; set; }
            public PitchHand pitchHand { get; set; }

            public class Batter
            {
                public int id { get; set; }
                public string fullName { get; set; }

                //we could collect up more information about the batside etc for this exercise I am not.
            }

            public class Pitcher
            {
                public int id { get; set; }
                public string fullName { get; set; }
                //we could collect up more information about the pitch hand etc.

            }
            public class BatSide
            {
                public string code { get; set; }
            }
            public class PitchHand
            {
                public string code { get; set; }
            }
        }

    }

}
