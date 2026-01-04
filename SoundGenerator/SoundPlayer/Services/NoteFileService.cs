using System.IO;
using System.Windows;

namespace SoundGen.Services
{
    public static class NoteFileService
    {
        public static void SaveNotesToFile(string content, string filePath)
        {
            try
            {
                File.WriteAllText(filePath, content);
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Ошибка записи файла:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static string LoadNotesFromFile(string filePath)
        {
            try
            {
                return File.ReadAllText(filePath);
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Ошибка чтения файла:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}