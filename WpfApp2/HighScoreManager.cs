using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TetrisWithTxt
{
    public static class HighScoreManager
    {
        private static readonly string highScoresFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "highscores.txt");

        // Метод для загрузки рекордов из файла
        public static List<HighScore> LoadHighScores()
        {
            List<HighScore> highScores = new List<HighScore>();

            if (File.Exists(highScoresFilePath))
            {
                var lines = File.ReadAllLines(highScoresFilePath);
                foreach (var line in lines)
                {
                    var score = HighScore.FromString(line);
                    if (score != null)
                    {
                        highScores.Add(score);
                    }
                }
            }

            return highScores.OrderByDescending(h => h.Score).ThenBy(h => h.Date).Take(10).ToList();
        }

        // Метод для сохранения рекорда в файл
        public static void SaveHighScore(HighScore newScore)
        {
            List<HighScore> highScores = LoadHighScores();
            highScores.Add(newScore);
            highScores = highScores.OrderByDescending(h => h.Score).ThenBy(h => h.Date).Take(10).ToList();

            using (StreamWriter writer = new StreamWriter(highScoresFilePath, false))
            {
                foreach (var score in highScores)
                {
                    writer.WriteLine(score.ToString());
                }
            }
        }
    }
}
