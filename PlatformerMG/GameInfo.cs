using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace TexasJames
{
    public class EnemyInfo
    {
        public float Speed = 0.0f;
        public float ChaseSpeed = 0.0f;
    }
    
    public class PlayerInfo
    {
        public float MaxMoveSpeed = 0.0f;
    }

    public class GemInfo
    {
        public float BounceHeight = 0.0f;
        public float BounceRate = 0.0f;
        public Color Color;
    }

    public class GameInfo
    {
        private static GameInfo mInstance = null;
        public static GameInfo Instance
        {
            get
            {
                if (mInstance == null)
                    mInstance = new GameInfo();
                return mInstance;
            }

            set { mInstance = value; }
        }

        public List<int> readHighscores()
        {
            List<int> highscoresInt = new List<int>();
            string[]scores = Highscores.Split(',');

            for(int i = 0; i < scores.Length; i++)
            {
                int toAdd = Int32.Parse(scores[i]);
                highscoresInt.Add(toAdd);
            }
            return highscoresInt;
        }

        public void WriteHighscores(List<int> scores)
        {
            if (scores.Count() <= 0) return;
            string toWrite = "";
            for(int i = 0; i < scores.Count() - 1; i++)
            {
                toWrite += scores[i]+",";
            }
            toWrite += scores[scores.Count() - 1];

            GameInfo.Instance.Highscores = toWrite;
        }

        public PlayerInfo PlayerInfo;
        public EnemyInfo EnemyInfo;
        public GemInfo GemInfo;
        public bool WasKeyCollected = false;
        public int Highscore = 0;
        public int NumberOfBullets = 0;
        public int Score = 0;
        public string Highscores = "";
    }
}
