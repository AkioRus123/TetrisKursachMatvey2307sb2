using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

namespace TetrisWithTxt
{
    public class HighScore
    {
        public string PlayerName { get; set; }
        public int Score { get; set; }
        public DateTime Date { get; set; }

        
        public HighScore(string playerName, int score, DateTime date)
        {
            PlayerName = playerName;
            Score = score;
            Date = date;
        }

        
        public override string ToString()
        {
            return $"{PlayerName},{Score},{Date:yyyy-MM-dd HH:mm:ss}";
        }

        
        public static HighScore FromString(string line)
        {
            var parts = line.Split(',');
            if (parts.Length == 3 &&
                int.TryParse(parts[1], out int score) &&
                DateTime.TryParse(parts[2], out DateTime date))
            {
                return new HighScore(parts[0], score, date);
            }
            return null;
        }
    }
}
