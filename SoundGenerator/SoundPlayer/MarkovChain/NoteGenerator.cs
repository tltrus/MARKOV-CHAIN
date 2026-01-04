using System.Text;


namespace SoundGen.MarkovChain
{
    class NoteGenerator
    {
        // Ключ: пара слов (prev, current), Значение: список следующих слов
        private Dictionary<(string, string), List<string>> Chain = new Dictionary<(string, string), List<string>>();
        // Пары слов, с которых начинаются предложения
        private List<(string, string)> StartPairs = new List<(string, string)>();

        public delegate void MessageHandler(string msg);
        public event MessageHandler? Notify;

        public bool isTrained;

        public void Train(List<string> sentences)
        {
            foreach (var sentence in sentences)
            {
                var words = sentence.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length < 2) continue; // Нужно хотя бы 2 слова, чтобы сформировать биграмму

                // Сохраняем пару первых двух слов как начальную
                StartPairs.Add((words[0], words[1]));

                // Строим цепь: (prev, current) -> next
                for (int i = 0; i < words.Length - 2; i++)
                {
                    var prev = words[i];
                    var current = words[i + 1];
                    var next = words[i + 2];

                    var key = (prev, current);

                    if (!Chain.ContainsKey(key))
                    {
                        Chain[key] = new List<string>();
                    }
                    Chain[key].Add(next);
                }
            }
            isTrained = true;
            Notify?.Invoke(Chain.Count + " chains have been created.\n");
        }

        public string Generate(int maxNotes = 30)
        {
            if (StartPairs.Count == 0) return "";

            var rand = new Random();
            var (prevNote, currentNote) = StartPairs[rand.Next(StartPairs.Count)];
            var result = new StringBuilder();
            result.Append($"{prevNote} {currentNote}");

            for (int i = 2; i < maxNotes; i++)
            {
                var key = (prevNote, currentNote);

                if (!Chain.ContainsKey(key)) break;

                var nextNotes = Chain[key];
                var nextNote = nextNotes[rand.Next(nextNotes.Count)];

                result.Append(" " + nextNote);

                prevNote = currentNote;
                currentNote = nextNote;
            }
            Notify?.Invoke("Notes generation is complete.\n");

            return result.ToString();
        }

        // Метод для вывода Chain
        public void PrintChain()
        {
            Console.WriteLine("\n--- Chain (цепь биграмм) ---");
            foreach (var kvp in Chain)
            {
                var (prev, current) = kvp.Key;
                Console.WriteLine($"('{prev}', '{current}') -> [{string.Join(", ", kvp.Value.Select(s => $"'{s}'"))}]");
            }
        }

        // Метод для вывода StartPairs
        public void PrintStartPairs()
        {
            Console.WriteLine("\n--- StartPairs (первые пары слов предложений) ---");
            Console.WriteLine($"[{string.Join(", ", StartPairs.Select(p => $"('{p.Item1}', '{p.Item2}')"))}]");
        }
    }
}
