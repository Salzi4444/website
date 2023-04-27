using System;
using System.Text;
using System.Net;
using System.Linq;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ChessGameGetter
{
    internal class Program
    {
        static long gamecount = 0;

        static void Main(string[] args)
        {
            List<string> original = getGameData("Salzii", GameDataType.opponentname);
            List<string> names = new List<string>();

            for (int i = 0; i < original.Count; i ++)
            {
                Console.WriteLine();
                Console.WriteLine($"{i} / {original.Count}: {original[i]}");
                names.AddRange(getGameData(original[i], GameDataType.opponentname));
                Console.WriteLine($"games: {gamecount}");
                Console.WriteLine($"names: {names.Count}");
            }

            names.Add("Salzii");
            names.AddRange(original);
            names = names.Distinct().ToList();

            Console.WriteLine($"original length: {original.Count}");
            Console.WriteLine($"names length: {names.Count}");
            Console.WriteLine($"games: {gamecount}");


            while (true) { }
        }

        static List<string> getGameData(string username, GameDataType gameDataType)
        {
            dynamic obj = GetJson($"https://api.chess.com/pub/player/{username}/games/archives");

            dynamic archives = obj["archives"];

            List<string> data = new List<string>();

            foreach(string archive in archives)
            {
                try
                {
                    dynamic games = GetJson(archive);
                    games = games["games"];

                    foreach (dynamic item in games)
                    {
                        if (item["rules"] == "chess" && item["initial_setup"] == "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
                        {
                            switch (gameDataType)
                            {
                                case GameDataType.pgn: data.Add(CutoffPgn(item["pgn"])); break;
                                case GameDataType.opponentname: data.Add(GetOpponentName(username, item["white"]["username"], item["black"]["username"])); break;
                            }

                            gamecount++;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("error while accessing archive: ");
                    Console.WriteLine(e.ToString());
                }
            }

            return data.Distinct().ToList();
        }

        static dynamic GetJson(string url)
        {
            Console.WriteLine($"requesting {url}");

            WebClient client = new WebClient();
            string request = client.DownloadString(url);

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            
            return serializer.Deserialize<object>(request);
        }

        static string CutoffPgn(string pgn)
        {
            string[] splits = pgn.Split(new string[] { "\n" }, StringSplitOptions.None);

            pgn = splits[splits.Length - 2];

            string pattern = @"\{[^}]+\}";

            pgn = Regex.Replace(pgn, pattern, "");
            pgn = Regex.Replace(pgn, "  ", " ");
            pgn = Regex.Replace(pgn, @"\.", "");
            pgn = Regex.Replace(pgn, "\n", "");

            splits = pgn.Split(' ');

            pgn = "";
            int trash;

            for (int i = 0; i < splits.Length; i ++)
            {
                if (i > 1)
                {
                    pgn += " ";
                }

                if (int.TryParse(splits[i], out trash) == false)
                {
                    pgn += splits[i];
                }
            }

            pgn = Regex.Replace(pgn, "  ", " ");

            return pgn;
        }

        static string GetOpponentName(string username, string white, string black)
        {
            if (username == white)
            {
                return black;
            }
            else
            {
                return white;
            }
        }
    }

    enum GameDataType
    {
        pgn,
        opponentname,
    }
}

/*

statistic:
dominiknatter
original length: 158
names length: 77616
games: 87445

*/