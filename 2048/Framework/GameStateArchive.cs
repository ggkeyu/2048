using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace _2048.Framework
{
    /// <summary>
    /// 游戏存档
    /// </summary>
    [Serializable]
    public class GameStateArchive
    {
        public string Name { get; set; }

        public int Score { get; set; }

        private int[] blocks = Array.Empty<int>();

        public int[] GetBlocks()
        {
            return blocks;
        }

        public void SetBlocks(int[] value)
        {
            blocks = value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public void Save(string path)
        {
            using(var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                BinaryFormatter formatter = new BinaryFormatter();

                formatter.Serialize(fs, this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static GameStateArchive Load(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read,FileShare.Read))
            {
                BinaryFormatter formatter = new BinaryFormatter();

                GameStateArchive archive = formatter.Deserialize(fs) as GameStateArchive;

                return archive;
            }
        }
    }
}
