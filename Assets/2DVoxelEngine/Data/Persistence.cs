using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace Hazel.VoxelEngine2D.Data
{
    public class Persistence
    {
        public string SavePath { get; private set; }

        public Persistence(string rootId)
        {
            this.SavePath = Path.Combine(Path.GetFullPath(Application.persistentDataPath), rootId);
        }

        /// <summary>
        /// Saves a serializable 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename">file name save location</param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool Save<T>(string filename, T data)
        {
            try
            {
                string path = Path.Combine(this.SavePath, filename);
                string directory = Path.GetDirectoryName(path);

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var stream = File.OpenWrite(path);
                var formatter = new BinaryFormatter();

                formatter.Serialize(stream, data);
                stream.Flush();
                stream.Close();
                stream.Dispose();
            } 
            catch(System.Exception exception)
            {
                Debug.LogException(exception);
                return false;
            }

            return true;
        }

        public bool TryLoad<T>(string filename, out T data)
        {
            try
            {
                string path = Path.Combine(this.SavePath, filename);
                var formatter = new BinaryFormatter();
                var stream = File.OpenRead(path);

                data = (T)formatter.Deserialize(stream);
                stream.Flush();
                stream.Close();
                stream.Dispose();

            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
                data = default;
                return false;
            }

            return true;
        }
    }

}