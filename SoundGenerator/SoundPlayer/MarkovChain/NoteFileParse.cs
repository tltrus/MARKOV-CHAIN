using System.IO;

namespace SoundGen.MarkovChain
{
    /// <summary>
    /// Класс для парсинга музыкальных файлов
    /// </summary>
    public class NoteFileParse
    {
        /// <summary>
        /// Результат парсинга - список музыкальных блоков
        /// </summary>
        public List<List<string>> MusicData { get; private set; }

        /// <summary>
        /// Имя файла, который был обработан
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// Количество блоков в файле
        /// </summary>
        public int BlockCount => MusicData?.Count ?? 0;

        /// <summary>
        /// Общее количество нот/элементов во всех блоках
        /// </summary>
        public int TotalElements => MusicData?.Sum(block => block.Count) ?? 0;

        /// <summary>
        /// Статус выполнения парсинга
        /// </summary>
        public bool IsParsed { get; private set; }

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public NoteFileParse()
        {
            MusicData = new List<List<string>>();
            IsParsed = false;
        }

        /// <summary>
        /// Загружает и парсит музыкальный файл
        /// </summary>
        /// <param name="filePath">Путь к файлу</param>
        /// <returns>True если парсинг успешен, false в случае ошибки</returns>
        public bool LoadAndParse(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Файл {filePath} не найден!");
                    return false;
                }

                FileName = Path.GetFileName(filePath);
                string[] lines = File.ReadAllLines(filePath);

                ParseLines(lines);
                IsParsed = true;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке файла: {ex.Message}");
                IsParsed = false;
                return false;
            }
        }

        /// <summary>
        /// Парсит массив строк в музыкальные блоки
        /// </summary>
        /// <param name="lines">Массив строк для парсинга</param>
        public void ParseLines(string[] lines)
        {
            MusicData.Clear();
            List<string> currentBlock = new List<string>();

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();

                // Пропускаем комментарии и пустые строки
                if (IsCommentLine(trimmedLine) || string.IsNullOrWhiteSpace(trimmedLine))
                {
                    // Сохраняем текущий блок, если он не пуст
                    if (currentBlock.Count > 0)
                    {
                        SaveCurrentBlock(ref currentBlock);
                    }
                    continue;
                }

                // Добавляем элементы строки в текущий блок
                AddLineToBlock(trimmedLine, ref currentBlock);
            }

            // Сохраняем последний блок, если он не пуст
            if (currentBlock.Count > 0)
            {
                SaveCurrentBlock(ref currentBlock);
            }
        }

        /// <summary>
        /// Проверяет, является ли строка комментарием
        /// </summary>
        /// <param name="line">Строка для проверки</param>
        /// <returns>True если строка начинается с комментария</returns>
        private bool IsCommentLine(string line)
        {
            return line.StartsWith("//");
        }

        /// <summary>
        /// Добавляет элементы строки в текущий блок
        /// </summary>
        /// <param name="line">Строка для обработки</param>
        /// <param name="currentBlock">Текущий блок</param>
        private void AddLineToBlock(string line, ref List<string> currentBlock)
        {
            string[] parts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            currentBlock.AddRange(parts);
        }

        /// <summary>
        /// Сохраняет текущий блок в общий список и очищает его
        /// </summary>
        /// <param name="currentBlock">Текущий блок для сохранения</param>
        private void SaveCurrentBlock(ref List<string> currentBlock)
        {
            if (currentBlock.Count > 0)
            {
                MusicData.Add(new List<string>(currentBlock));
                currentBlock.Clear();
            }
        }

        /// <summary>
        /// Получает музыкальный блок по индексу
        /// </summary>
        /// <param name="index">Индекс блока (начиная с 0)</param>
        /// <returns>Музыкальный блок или null если индекс неверный</returns>
        public List<string> GetBlock(int index)
        {
            if (index >= 0 && index < MusicData.Count)
            {
                return new List<string>(MusicData[index]); // Возвращаем копию
            }
            return null;
        }

        /// <summary>
        /// Получает все музыкальные блоки
        /// </summary>
        /// <returns>Копия списка всех блоков</returns>
        public List<List<string>> GetAllBlocks()
        {
            return MusicData.Select(block => new List<string>(block)).ToList();
        }

        /// <summary>
        /// Получает блок в виде строки с разделителями
        /// </summary>
        /// <param name="index">Индекс блока</param>
        /// <param name="separator">Разделитель элементов (по умолчанию пробел)</param>
        /// <returns>Строка с элементами блока или пустая строка</returns>
        public string GetBlockAsString(int index, string separator = " ")
        {
            var block = GetBlock(index);
            return block != null ? string.Join(separator, block) : string.Empty;
        }

        /// <summary>
        /// Выводит информацию о всех блоках в консоль
        /// </summary>
        public void PrintSummary()
        {
            if (!IsParsed)
            {
                Console.WriteLine("Файл не был распарсен!");
                return;
            }

            Console.WriteLine($"Файл: {FileName}");
            Console.WriteLine($"Всего блоков: {BlockCount}");
            Console.WriteLine($"Всего элементов: {TotalElements}");
            Console.WriteLine("======================================");
            Console.WriteLine();

            for (int i = 0; i < MusicData.Count; i++)
            {
                Console.WriteLine($"Блок {i + 1}:");
                Console.WriteLine($"Количество элементов: {MusicData[i].Count}");
                Console.WriteLine($"Элементы: {GetBlockAsString(i)}");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Сохраняет результат парсинга в файл
        /// </summary>
        /// <param name="outputPath">Путь для сохранения</param>
        /// <returns>True если сохранение успешно</returns>
        public bool SaveToFile(string outputPath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(outputPath))
                {
                    writer.WriteLine($"Результат парсинга файла: {FileName}");
                    writer.WriteLine($"Дата парсинга: {DateTime.Now}");
                    writer.WriteLine($"Всего блоков: {BlockCount}");
                    writer.WriteLine($"Всего элементов: {TotalElements}");
                    writer.WriteLine("======================================");
                    writer.WriteLine();

                    for (int i = 0; i < MusicData.Count; i++)
                    {
                        writer.WriteLine($"Блок {i + 1}:");
                        writer.WriteLine($"Количество элементов: {MusicData[i].Count}");
                        writer.WriteLine("Элементы:");

                        foreach (string item in MusicData[i])
                        {
                            writer.WriteLine($"  {item}");
                        }

                        writer.WriteLine();
                    }
                }

                Console.WriteLine($"Результаты сохранены в файл: {outputPath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось сохранить результаты: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Экспортирует данные в формате JSON (упрощенный)
        /// </summary>
        /// <param name="outputPath">Путь для сохранения JSON файла</param>
        public bool ExportToJson(string outputPath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(outputPath))
                {
                    writer.WriteLine("{");
                    writer.WriteLine($"  \"fileName\": \"{FileName}\",");
                    writer.WriteLine($"  \"blockCount\": {BlockCount},");
                    writer.WriteLine($"  \"totalElements\": {TotalElements},");
                    writer.WriteLine("  \"blocks\": [");

                    for (int i = 0; i < MusicData.Count; i++)
                    {
                        writer.WriteLine("    {");
                        writer.WriteLine($"      \"blockNumber\": {i + 1},");
                        writer.WriteLine($"      \"elementCount\": {MusicData[i].Count},");
                        writer.WriteLine($"      \"elements\": [");

                        for (int j = 0; j < MusicData[i].Count; j++)
                        {
                            string element = $"\"{MusicData[i][j]}\"";
                            if (j < MusicData[i].Count - 1)
                                element += ",";
                            writer.WriteLine($"        {element}");
                        }

                        writer.WriteLine("      ]");
                        writer.Write(i < MusicData.Count - 1 ? "    }," : "    }");
                        writer.WriteLine();
                    }

                    writer.WriteLine("  ]");
                    writer.WriteLine("}");
                }

                Console.WriteLine($"Данные экспортированы в JSON: {outputPath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при экспорте в JSON: {ex.Message}");
                return false;
            }
        }
    }
}
