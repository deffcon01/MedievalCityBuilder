using System;
using System.IO;
using System.Text;

namespace MedievalCityBuilder.Persistence
{
    /// <summary>
    /// Handles saving and loading of the world state.
    /// The temp-file-then-replace pattern prevents a crash during save from corrupting an existing world.
    /// </summary>
    public static class WorldSaveFile
    {
        public static void Save(string path, WorldSaveData data)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path cannot be null, empty, or whitespace.", nameof(path));
            }

            string? directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string tempPath = path + ".tmp";
            bool success = false;
            try
            {
                string json = WorldSaveSerializer.Serialize(data);
                
                // Write UTF-8 WITHOUT BOM
                var encoding = new UTF8Encoding(false);
                using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(stream, encoding))
                {
                    writer.Write(json);
                    writer.Flush();
                    stream.Flush(true);
                }
                
                File.Move(tempPath, path, true);
                success = true;
            }
            finally
            {
                if (!success && File.Exists(tempPath))
                {
                    try
                    {
                        File.Delete(tempPath);
                    }
                    catch (IOException)
                    {
                        // Ignore: cleanup failure should not mask the original exception
                        // (e.g., UnauthorizedAccessException on Windows if tempPath is a directory)
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Ignore: cleanup failure should not mask the original exception
                        // (e.g., Windows throws this when File.Delete is called on a directory)
                    }
                }
            }
        }

        public static WorldSaveData Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path cannot be null, empty, or whitespace.", nameof(path));
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Save file not found.", path);
            }

            string json = File.ReadAllText(path, new UTF8Encoding(false));
            return WorldSaveSerializer.Deserialize(json);
        }
    }
}
