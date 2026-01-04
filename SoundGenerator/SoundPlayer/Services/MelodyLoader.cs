using SoundGen.Models;
using System.IO;

namespace SoundGen.Services
{
    public static class MelodyLoader
    {
        public static List<SavedMelody> LoadMelodiesFromAppDirectory()
        {
            try
            {
                var melodies = new List<SavedMelody>();
                var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var txtFiles = Directory.GetFiles(appDirectory + "Music", "*.txt");

                foreach (var filePath in txtFiles)
                {
                    try
                    {
                        var fileName = Path.GetFileNameWithoutExtension(filePath);
                        var content = File.ReadAllText(filePath);
                        var fileInfo = new FileInfo(filePath);

                        melodies.Add(new SavedMelody
                        {
                            Name = fileName,
                            FilePath = filePath,
                            ModifiedDate = fileInfo.LastWriteTime,
                            Content = content
                        });
                    }
                    catch (IOException)
                    {
                        // Пропускаем файлы, которые не удается прочитать
                        continue;
                    }
                }

                return melodies.OrderByDescending(m => m.ModifiedDate).ToList();
            }
            catch (Exception)
            {
                return new List<SavedMelody>();
            }
        }
    }
}