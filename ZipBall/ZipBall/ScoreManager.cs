using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZipBall
{
    public class Score
    {
        public string PlayerName { get; set; }
        public int PlayerScore { get; set; }
    }
    public class ScoreManager
    {
        // sorted list of scores
        private List<Score> scores;
        private int MaxScores;

        public ScoreManager()
        {
            scores = new List<Score>();
            MaxScores = 10;
        }

        public bool IsHighScore(Score s)
        {
            bool IsHigh = false;

            if (scores.Count == 0)
            {
                IsHigh = true;
            }
            else
            {
                foreach (Score si in scores)
                {
                    // found new high score
                    if (s.PlayerScore >= si.PlayerScore)
                    {
                        IsHigh = true;
                        break;
                    }
                }
            }

            return IsHigh;
        }

        public bool CheckInsertHighScore(Score s)
        {
            bool Inserted = false;

            if (IsHighScore(s))
            {
                for (int i = 0; i < scores.Count; i++)
                {
                    if (s.PlayerScore >= scores[i].PlayerScore)
                    {
                        Inserted = true;
                        scores.Insert(i, s);
                        break;
                    }
                }

                // we need only top 'n' most high scores
                if (scores.Count > MaxScores)
                {
                    // remove last entry
                    scores.RemoveAt(scores.Count - 1);
                }
            }

            return Inserted;
        }
    }
}
