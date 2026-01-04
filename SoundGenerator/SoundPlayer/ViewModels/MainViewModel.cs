using SoundGen.MarkovChain;
using SoundGen.Models;
using SoundGen.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace SoundGen.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private static readonly Dictionary<string, double> NoteByName = new(StringComparer.OrdinalIgnoreCase)
        {
            // Октава 3
            { "C3", 130.81 },   { "C#3", 138.59 }, { "Db3", 138.59 },
            { "D3", 146.83 },   { "D#3", 155.56 }, { "Eb3", 155.56 },
            { "E3", 164.81 },   { "F3", 174.61 },
            { "F#3", 185.00 },  { "Gb3", 185.00 },
            { "G3", 196.00 },   { "G#3", 207.65 }, { "Ab3", 207.65 },
            { "A3", 220.00 },   { "A#3", 233.08 }, { "Bb3", 233.08 },
            { "B3", 246.94 },

            // Октава 4
            { "C4", 261.63 },   { "C#4", 277.18 }, { "Db4", 277.18 },
            { "D4", 293.66 },   { "D#4", 311.13 }, { "Eb4", 311.13 },
            { "E4", 329.63 },   { "F4", 349.23 },
            { "F#4", 369.99 },  { "Gb4", 369.99 },
            { "G4", 392.00 },   { "G#4", 415.30 }, { "Ab4", 415.30 },
            { "A4", 440.00 },   { "A#4", 466.16 }, { "Bb4", 466.16 },
            { "B4", 493.88 },

            // Октава 5
            { "C5", 523.25 },   { "C#5", 554.37 }, { "Db5", 554.37 },
            { "D5", 587.33 },   { "D#5", 622.25 }, { "Eb5", 622.25 },
            { "E5", 659.25 },   { "F5", 698.46 },
            { "F#5", 739.99 },  { "Gb5", 739.99 },
            { "G5", 783.99 },   { "G#5", 830.61 }, { "Ab5", 830.61 },
            { "A5", 880.00 },   { "A#5", 932.33 }, { "Bb5", 932.33 },
            { "B5", 987.77 },

            // Октава 6
            { "C6", 1046.50 },   { "C#6", 1108.73 }, { "Db6", 1108.73 },
            { "D6", 1174.66 },   { "D#6", 1244.51 }, { "Eb6", 1244.51 },
            { "E6", 1318.51 },   { "F6", 1396.91 },
            { "F#6", 1479.98 },  { "Gb6", 1479.98 },
            { "G6", 1567.98 },   { "G#6", 1661.22 }, { "Ab6", 1661.22 },
            { "A6", 1760.00 },   { "A#6", 1864.66 }, { "Bb6", 1864.66 },
            { "B6", 1975.53 },

            // Без октавы = октава 4
            { "C", 261.63 },    { "C#", 277.18 },  { "Db", 277.18 },
            { "D", 293.66 },    { "D#", 311.13 },  { "Eb", 311.13 },
            { "E", 329.63 },    { "F", 349.23 },
            { "F#", 369.99 },   { "Gb", 369.99 },
            { "G", 392.00 },    { "G#", 415.30 },  { "Ab", 415.30 },
            { "A", 440.00 },    { "A#", 466.16 },  { "Bb", 466.16 },
            { "B", 493.88 }
        };

        private string _noteInputText = "C C# D D# E F F# G G# A A# B";
        private string _consoleText = string.Empty;
        private int _defaultDuration = 300;
        private int _volume = 60;
        private System.Media.SoundPlayer _currentPlayer;
        private SavedMelody _selectedMelody;
        private string _defaultSavePath;

        public string NoteInputText
        {
            get => _noteInputText;
            set => SetField(ref _noteInputText, value);
        }

        public string ConsoleText
        {
            get => _consoleText;
            set => SetField(ref _consoleText, value);
        }

        public int DefaultDuration
        {
            get => _defaultDuration;
            set => SetField(ref _defaultDuration, value);
        }

        public int Volume
        {
            get => _volume;
            set => SetField(ref _volume, value);
        }

        public SavedMelody SelectedMelody
        {
            get => _selectedMelody;
            set
            {
                if (SetField(ref _selectedMelody, value) && value != null)
                {
                    NoteInputText = value.Content;
                    OnPropertyChanged(nameof(SelectedMelody)); // Уведомляем об изменении
                }
            }
        }

        public ObservableCollection<SavedMelody> Melodies { get; } = new ObservableCollection<SavedMelody>();

        // Команды
        public ICommand PlayCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand GenerateCommand { get; }
        public ICommand SaveAsCommand { get; }
        public ICommand OpenCommand { get; }
        public ICommand PianoKeyCommand { get; }
        public ICommand ClearTextCommand { get; }
        public ICommand ClearConsoleCommand { get; }
        public ICommand RefreshMelodiesCommand { get; }

        NoteGenerator noteGenerator = new NoteGenerator();

        public MainViewModel()
        {
            PlayCommand = new RelayCommand(PlayMelody, CanPlayMelody);
            StopCommand = new RelayCommand(StopPlayback);
            GenerateCommand = new RelayCommand(GenerateSound);
            SaveAsCommand = new RelayCommand(SaveAs);
            OpenCommand = new RelayCommand(Open);
            PianoKeyCommand = new RelayCommand<PianoKeyInfo>(PlayPianoKey);
            ClearTextCommand = new RelayCommand(ClearText);
            ClearConsoleCommand = new RelayCommand(ClearConsole);
            RefreshMelodiesCommand = new RelayCommand(RefreshMelodies);

            _defaultSavePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "melody.txt");
            RefreshMelodies();

            noteGenerator.Notify += msg => { ConsoleText += msg; };
        }

        private void ClearText()
        {
            NoteInputText = string.Empty;
        }

        private void ClearConsole()
        {
            ConsoleText = string.Empty;
        }

        private void RefreshMelodies()
        {
            Melodies.Clear();
            var melodies = MelodyLoader.LoadMelodiesFromAppDirectory();
            ConsoleText += "Loaded " + melodies.Count + " melodies.\n";
            foreach (var melody in melodies)
            {
                Melodies.Add(melody);
            }
        }

        private bool CanPlayMelody()
        {
            return !string.IsNullOrWhiteSpace(NoteInputText);
        }

        private void PlayMelody()
        {
            StopPlayback();

            string input = NoteInputText.Trim();
            var notes = ParseNotes(input, DefaultDuration);
            if (notes == null || notes.Count == 0)
                return;

            double volume = Volume / 100.0;
            var stream = AudioGenerator.GenerateWavStream(notes, volume);
            _currentPlayer = new System.Media.SoundPlayer(stream);
            _currentPlayer.Play();
        }

        private List<NoteEvent> ParseNotes(string input, int defaultDuration)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show("Введите мелодию!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            var tokens = Regex.Split(input, @"[,\s;|]+");
            var notes = new List<NoteEvent>();

            foreach (string token in tokens)
            {
                string clean = token.Trim();
                if (string.IsNullOrEmpty(clean)) continue;

                string notePart = clean;
                int duration = defaultDuration;

                if (clean.Contains("/"))
                {
                    string[] parts = clean.Split('/');
                    if (parts.Length != 2)
                    {
                        ShowError($"Неверный формат: '{clean}'. Используйте 'Нота/Длительность', например C/300.");
                        return null;
                    }

                    notePart = parts[0].Trim();
                    if (!int.TryParse(parts[1].Trim(), out duration) || duration <= 0)
                    {
                        ShowError($"Неверная длительность в '{clean}'. Должно быть целое положительное число (мс).");
                        return null;
                    }
                }

                if (notePart == "_" || notePart == "0")
                {
                    notes.Add(new NoteEvent(null, duration));
                    continue;
                }

                if (NoteByName.TryGetValue(notePart, out double freq))
                {
                    notes.Add(new NoteEvent(freq, duration));
                }
                else
                {
                    ShowError($"Неизвестная нота: '{notePart}'.\nПоддерживаются: C, C#, Db, D, D#, Eb, E, F, F#, G, G#, A, A#, Bb, B (с октавами, например C4, F#5).\nПауза: _ или 0.");
                    return null;
                }
            }

            return notes;
        }

        private void StopPlayback()
        {
            if (_currentPlayer != null)
            {
                _currentPlayer.Stop();
                _currentPlayer.Dispose();
                _currentPlayer = null;
            }
        }

        private void GenerateSound()
        {
            if (Melodies.Count == 0) return;

            // Training
            if (!noteGenerator.isTrained)
            {
                var _musicData = new List<string>();
                foreach (var melody in Melodies)
                {
                    var notes = melody.Content.Replace("\r\n", " ");
                    _musicData.Add(notes);
                }
                noteGenerator.Train(_musicData);
            }

            // Generation
            NoteInputText = noteGenerator.Generate(maxNotes: 60);
        }

        private void SaveAs()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                DefaultExt = ".txt",
                FileName = "melody.txt",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "Music"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(dialog.FileName, NoteInputText);
                    RefreshMelodies(); // Обновляем список после сохранения
                    MessageBox.Show($"Мелодия сохранена в файл:\n{dialog.FileName}", "Сохранено",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    ConsoleText += "Melody was saved.\n";
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Ошибка записи файла:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Open()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "Music"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    NoteInputText = File.ReadAllText(dialog.FileName);
                    RefreshMelodies(); // Обновляем список
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Ошибка чтения файла:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void PlayPianoKey(PianoKeyInfo keyInfo)
        {
            var noteEvent = new NoteEvent(keyInfo.Frequency, 300);
            using (var stream = AudioGenerator.GenerateWavStream(new[] { noteEvent }, Volume / 100.0))
            {
                var player = new System.Media.SoundPlayer(stream);
                player.Play();
            }

            // Добавляем в текстовое поле
            string noteEntry = $"{keyInfo.Name}/300";
            if (string.IsNullOrEmpty(NoteInputText.Trim()))
                NoteInputText = noteEntry;
            else
                NoteInputText += " " + noteEntry;
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void OnWindowClosed()
        {
            StopPlayback();
        }
    }
}