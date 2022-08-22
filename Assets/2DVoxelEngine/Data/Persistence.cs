using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

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
        /// <typeparam name="T">Type that is serializable</typeparam>
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

        /// <summary>
        /// Loads data at specified filename
        /// </summary>
        /// <typeparam name="T">Type to deserialze</typeparam>
        /// <param name="filename">relative path + file name of data</param>
        /// <param name="data">data read from the file</param>
        /// <returns></returns>
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
            catch
            {
                data = default;
                return false;
            }

            return true;
        }
    }

}