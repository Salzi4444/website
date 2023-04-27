using System;
using System.Text;
using System.Net;
using System.Linq;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

namespace ChessGameGetter
{
    internal class Program
    {
        static long gamecount = 0;

        static void Main(string[] args)
        {
            List<string> names = getGameData("Salzii", GameDataType.opponentname);
            Console.WriteLine("got names to instpect");

            for (int i = 0; i < names.Count; i++)
            {
                WritePGNS(names[i]);

                Console.WriteLine($"wrote {i} / {names.Count} {names[i]}");
            }

            while (true) { }
        }

        static void WritePGNS(string username)
        {
            List<string> pgn = getGameData(username, GameDataType.pgn);

            gamecount += pgn.Count;

            using (StreamWriter fs = new StreamWriter($@"games\{username}.txt"))
            {
                foreach (string game in pgn)
                {
                    fs.WriteLine(game);
                }
            }
        }

        static List<string> getGameData(string username, GameDataType gameDataType)
        {
            dynamic obj = GetJson($"https://api.chess.com/pub/player/{username}/games/archives");

            dynamic archives = obj["archives"];

            List<string> data = new List<string>();

            foreach (string archive in archives)
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
            //Console.WriteLine($"requesting {url}");

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
            pgn = Regex.Replace(pgn, "#", "");
            pgn = Regex.Replace(pgn, "1-0", "");
            pgn = Regex.Replace(pgn, "0-1", "");
            pgn = Regex.Replace(pgn, "1/2-1/2", "");

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


Salzii
68 moves / game

hikaru
86 moves / game

*/