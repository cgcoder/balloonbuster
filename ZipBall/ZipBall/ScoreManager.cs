using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.IsolatedStorage;
using System.Xml.Serialization;

namespace ZipBall
{
    public class Score
    {
        public string PlayerName { get; set; }
        public int PlayerScore { get; set; }
    }

    public class ScoreStore
    {
        public ScoreStore()
        {
            Scores = new List<Score>();
        }

        public List<Score> Scores
        {
            get;
            set;
        }
    }

    public class ScoreManager
    {
        private int MaxScores;
        private ScoreStore store;
        private bool loadOk;

        public ScoreManager()
        {
            store = new ScoreStore();
            MaxScores = 5;
        }

        public void CheckAndLoad()
        {
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFileStream stream = null; 
            loadOk = false;
            try
            {
                if (storage.FileExists("score.txt"))
                {
                    XmlSerializer serializer = new XmlSerializer(store.GetType());
                    stream = storage.OpenFile("score.txt", System.IO.FileMode.OpenOrCreate);

                    store = serializer.Deserialize(stream) as ScoreStore;

                    loadOk = true;
                    storage.Dispose();
                }
            }
            catch (Exception e)
            {
                loadOk = false;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }

            if (!loadOk)
            {
                try
                {
                    storage.CreateFile("score.txt");
                    var scores = new List<Score>();
                    for (int i = 0; i < MaxScores; i++)
                    {
                        scores.Add(new Score
                        {
                            PlayerName = "noname",
                            PlayerScore = 0,
                        });
                    }
                    store.Scores = scores;
                    loadOk = true;
                }
                catch (Exception e)
                {
                    loadOk = false;
                }
                storage.Dispose();
                SaveScores();
            }

        }

        public bool IsHighScore(int sc)
        {
            bool IsHigh = false;

            if (store.Scores.Count == 0)
            {
                IsHigh = true;
            }
            else
            {
                foreach (Score si in store.Scores)
                {
                    // found new high score
                    if (sc > si.PlayerScore)
                    {
                        IsHigh = true;
                        break;
                    }
                }
            }

            return IsHigh;
        }

        public bool IsLoadOk
        {
            get { return loadOk; }
        }

        public void SaveScores()
        {
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
            XmlSerializer serializer = null;
            try
            {
                if (storage.FileExists("score.txt"))
                {
                    IsolatedStorageFileStream stream = storage.OpenFile("score.txt", System.IO.FileMode.OpenOrCreate);
                    
                    serializer = new XmlSerializer(store.GetType());
                    serializer.Serialize(stream, store);

                    stream.Close();
                    stream.Dispose();
                }
            }
            catch (Exception e)
            {
                // ignore save error!
            }
            finally
            {
                
            }
        }

        public List<Score> Scores
        {
            get
            {
                if (store != null) return store.Scores;
                return null;
            }
        }

        public bool CheckInsertHighScore(Score s)
        {
            bool Inserted = false;

            List<Score> scores = store.Scores;

            if (IsHighScore(s.PlayerScore))
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

            SaveScores();

            return Inserted;
        }
    }
}
