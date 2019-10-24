using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.IO;
using Microsoft.VisualBasic;
using System.Web.Script.Serialization;
using static StevenGuptaStatLoader.Game;
using ExtensionMethods;

namespace StevenGuptaStatLoader
{
    class LoadStats
    {
        // load messages
        public Boolean loadSuccessful { get; set; }
        public string Message { get; set; }
        
        //json objects
        GameInfoCollection gameInfo;
        AllPlaysCollection allPlays;
        DateCollection dateCollection;
        
      

        // Database context
        glna9svpdbsdp0DataContext db;

        string gameInfoJSON;
        string daysGamesJSON;


        public LoadStats(int gameID)
        {
            // connect to the SQL Server DB
            if (!initializeDBConnection())
            {
                return;
            }

            if (gameID == 0)
            { 

                DateTime StartDate = DateTime.Parse("10/21/2019");
                DateTime EndDate = DateTime.Parse("10/23/2019");

                for (DateTime date = StartDate; date.Date <= EndDate.Date; date = date.AddDays(1))
                {

                    getGameIDsPerDay(date.ToShortDateString());
                    try
                    {


                        for (int i = 0; i < dateCollection.dates[0].games.Count; i++)
                        {
                            loadData(dateCollection.dates[0].games[i].gamePK);
                        }

                    }
                    catch (Exception e)
                    {

                    }
                }

            }

            else
            {
                loadData(gameID);
            }



            //int? pitchRecords = 0;
            //int? hitRecords = 0;
            //int? playRecords = 0;

            //db.statSP_SelectLoadResult(gameID, ref pitchRecords, ref hitRecords, ref playRecords);

            //Message = String.Format("Pitch Records Loaded {0}, Hit Records Loaded {1}, Play Records Loaded {2}", pitchRecords, hitRecords, playRecords);
            Message = "Load Complete";
            db.Connection.Close();
        }

        private bool loadData(int gameID)
        {

            // set up the JSON objects
            if (!getStats(gameID))
            {
                return false;
            }

            // extract initial game data
            if (!gameDataLoad(gameID))
            {
                return false;
            }

            // capture players that are on both rosters
            //if (!loadPlayersToDb("home"))
           // {
            //    return false;
          //  }

          //  if (!loadPlayersToDb("away"))
           // {
           //     return false;
          //  }

            // capture all the plays from the game.
            if (!allPlaysLoad(gameID))
            {
                return false;
            }

            return true;
        }


        private bool getGameIDsPerDay(string gameDt)
        {
            try
            {
                using (var webClient = new WebClient())
                {

                    daysGamesJSON = webClient.DownloadString(string.Format("https://statsapi.mlb.com/api/v1/schedule/games?sportId=1&date={0}", gameDt));
                    dateCollection = JsonConvert.DeserializeObject<DateCollection>(daysGamesJSON);

                }
            }
            catch (Exception e)
            {
                writeErrorLog(e.Message, 0);
                Message = e.Message;
                loadSuccessful = false;
                return false;
            }

            return true;
        }

        // create our Deserialized JSON data objects
        private bool getStats(int gameID)
        {
            try
            {
                using (var webClient = new WebClient())
                {

                    gameInfoJSON = webClient.DownloadString(string.Format("https://statsapi.mlb.com/api/v1/game/{0}/boxscore", gameID.ToString()));
                    gameInfo = JsonConvert.DeserializeObject<GameInfoCollection>(gameInfoJSON);

                    string allPlaysJSON = webClient.DownloadString(string.Format("https://statsapi.mlb.com/api/v1/game/{0}/playByPlay", gameID.ToString()));
                    allPlays = JsonConvert.DeserializeObject<AllPlaysCollection>(allPlaysJSON);

                }
            }
            catch (Exception e)
            {
                writeErrorLog(e.Message, gameID);
                Message = e.Message;
                loadSuccessful = false;
                return false;
            }

            return true;
                           
        }
        // Extract all player id's and names from the box scrore data.
        private bool loadPlayersToDb(string homeAway)
        {


            try
            {
                // the key is dynamic so we must take an extra step here to find the key to get to the person value.

                System.Web.Script.Serialization.JavaScriptSerializer serializer = new JavaScriptSerializer();
                dynamic item = serializer.Deserialize<object>(gameInfoJSON);
                foreach (var p in item["teams"][homeAway]["players"])
                {

                    int playerID = item["teams"][homeAway]["players"][p.Key]["person"]["id"];
                    string fullName = item["teams"][homeAway]["players"][p.Key]["person"]["fullName"];
                    db.statSP_PlayerInsert(playerID, fullName);
                }

                return true;
            }
            catch (Exception e)
            {
                writeErrorLog(e.Message, 0);
                Message = "Failed to loadPlayersToDb " + e.Message;
                loadSuccessful = false;
                return false;
            }

        }
    

        private bool initializeDBConnection()
        {
            // normally database connection string should be initialized through an enrypted connection library rather than a hardcoded connection string.
            string dbConn = "Data Source=stevengserver.database.windows.net;Initial Catalog=mlbdata2;Persist Security Info=True;User ID=steveng;Password=Pinkfloyd0/";

            try
            {
                // initialize database context
                db = new glna9svpdbsdp0DataContext(dbConn);
                db.CommandTimeout = 0;

                return true;
            }
            catch (Exception e)
            {
                writeErrorLog(e.Message,0);
                Message = "Failed to initializeDBConnection " +  e.Message;
                loadSuccessful = false;
                return false;
            }

        }

        // This method will retrieve the basic amount of information about the game played. Score, hits, errors, date, time, etc.
        private bool gameDataLoad(int gameID)
        {
            
            string gameDate = null;
            string gateStartTime = null;
            string awayTeam;
            int awayTeamRuns;
            int awayTeamHits;
            int awayTeamErrors;
            string homeTeam;
            int homeTeamRuns;
            int homeTeamHits;
            int homeTeamErrors;
            string venue = null;
            string weatherTemp = null;
            string weatherDesc = null;
            string wind = null;

            try
            {
                         
                foreach (Info info in gameInfo.info)
                {
                    switch (info.label)
                    {
                        case "First pitch":
                            gateStartTime = info.value.Replace(".", "");
                            break;
                        case "Venue":
                            venue = info.value.Replace(".","");
                            break;

                        case "Weather":
                            weatherTemp = info.value;
                            if (weatherTemp.Contains(","))
                            {
                                weatherDesc = weatherTemp.RightToSpace(",");
                                weatherTemp = weatherTemp.LeftToSpace(",").Replace("Degrees","").Trim();
                            
                            }
                            break;
                        case "Wind":
                            wind = info.value;
                            break;
                    }
                }

                if (Information.IsDate(gameInfo.info.LastOrDefault().label))
                {
                    gameDate = gameInfo.info.LastOrDefault().label;
                }

                awayTeam = gameInfo.teams.away.team.name;
                awayTeamRuns = gameInfo.teams.away.teamStats.batting.runs;
                awayTeamHits = gameInfo.teams.away.teamStats.batting.hits;
                awayTeamErrors = gameInfo.teams.away.teamStats.fielding.errors;

                homeTeam = gameInfo.teams.home.team.name;
                homeTeamRuns = gameInfo.teams.home.teamStats.batting.runs;
                homeTeamHits = gameInfo.teams.home.teamStats.batting.hits;
                homeTeamErrors = gameInfo.teams.home.teamStats.fielding.errors;

                db.statSP_GameInsert(gameID, DateTime.Parse(gameDate + " " + gateStartTime), awayTeam, awayTeamRuns, awayTeamHits, awayTeamErrors, homeTeam, homeTeamRuns, homeTeamHits, homeTeamErrors, venue, weatherTemp, weatherDesc,wind);
                return true;

            }
            catch (Exception e)
            {
                writeErrorLog(e.Message, gameID);
                Message = "Failed gameDataLoad " + e.Message;
                loadSuccessful = false;
                return false;
            }


        }

        // method for loading all the plays from the game to the database
        private bool allPlaysLoad(int gameID)
        {
            // matchup variables
            int batterID;
            int pitcherID;
            string playEvent;
            string playDescription;
            int atBatIndex;
            int? playID = 0;
            string pitchHand;
            string batHand;


            // pitch result variables
            int pitchDataPitchNumber;
            decimal pitchDataStartSpeed;
            decimal pitchDataPX;
            decimal pitchDataPZ;
            string pitchDataCallDescription;
            int? pitchDataPitchID = 0;
            decimal pitchDataX;
            decimal pitchDataY;

            decimal breakAngle;
            decimal breakLength;
            decimal breakY;
            decimal spinRate;
            decimal spinDirection;

            string pitchTypeCode;
            string pitchTypeDescription;

            // hit variables
            decimal hitDataLaunchSpeed;
            decimal hitDataLaunchAngle;
            decimal hitDataTotalDistance;

            int hitLocation;
            decimal hitCoordX;
            decimal hitCoordY;

            try
            {

                foreach (Play play in allPlays.allPlays)
                {
                    batterID = play.matchup.batter.id;
                    pitcherID = play.matchup.pitcher.id;
                    playEvent = play.result.Event;
                    playDescription = play.result.description;
                    atBatIndex = play.atBatIndex;
                    pitchHand = play.matchup.pitchHand.code;
                    batHand = play.matchup.batSide.code;

                    db.statSP_PlayInsert(gameID, atBatIndex, batterID, pitcherID, playEvent, playDescription,pitchHand,batHand, ref playID);

                    foreach (PlayEventsCollection playEvents in play.playEvents)
                    {
                        if (playEvents.isPitch)
                        { 
                            // each pitch and hit if available will be iterated from the PlayEvents
                            pitchDataPitchNumber = playEvents.pitchNumber;
                            pitchDataStartSpeed = playEvents.pitchData.startSpeed;
                            pitchDataPX = playEvents.pitchData.coordinates.pX;
                            pitchDataPZ = playEvents.pitchData.coordinates.pZ;
                            pitchDataCallDescription =  playEvents.details.description ?? playEvents.details.call.description;
                            if (pitchDataCallDescription == "Automatic Ball")
                            {
                                pitchTypeCode = null;
                                pitchTypeDescription = null;
                            }
                            else
                            {
                                pitchTypeCode = playEvents.details.type.code;
                                pitchTypeDescription = playEvents.details.type.description;

                            }

                            pitchDataX = playEvents.pitchData.coordinates.x;
                            pitchDataY = playEvents.pitchData.coordinates.y;

                            breakAngle = playEvents.pitchData.breaks.breakAngle;
                            breakLength = playEvents.pitchData.breaks.breakLength;
                            breakY = playEvents.pitchData.breaks.breakY;
                            spinRate = playEvents.pitchData.breaks.spinRate;
                            spinDirection = playEvents.pitchData.breaks.spinDirection;

                            db.statSP_PitchInsert(playID, pitchDataPitchNumber, pitchDataStartSpeed, pitchDataPX, pitchDataPZ,
                                pitchDataX, pitchDataY, breakAngle, breakLength, breakY, spinRate, spinDirection, 
                                pitchDataCallDescription, pitchTypeCode, pitchTypeDescription, ref pitchDataPitchID);
                            
                            if (playEvents.hitData != null)
                            { 
                                hitDataLaunchSpeed = playEvents.hitData.launchSpeed;
                                hitDataLaunchAngle = playEvents.hitData.launchAngle;
                                hitDataTotalDistance = playEvents.hitData.totalDistance;

                                hitLocation = playEvents.hitData.location;
                                hitCoordX = playEvents.hitData.coordinates.coordX;
                                hitCoordY = playEvents.hitData.coordinates.coordY;


                                db.statSP_HitInsert(playID, pitchDataPitchID, hitDataLaunchSpeed, 
                                    hitDataLaunchAngle, hitDataTotalDistance, hitLocation, hitCoordX, hitCoordY);
                            }
                        }
                    }

                }
                return true;
            }
            catch (Exception e)
            {
                writeErrorLog(e.Message, gameID);
                Message = "Failed allPlaysLoad " + e.Message;
                loadSuccessful = false;
                return false;
            }
        }

       
        private void writeErrorLog (string errMessage, int gameID)
        {
            StreamWriter sw = new StreamWriter("error.log", true);
            sw.WriteLine(gameID.ToString() + " " + errMessage);
            sw.Close();
        }
    }
}
