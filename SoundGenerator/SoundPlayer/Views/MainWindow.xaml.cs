using SoundGen.Models;
using SoundGen.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;


namespace SoundGen
{
    public partial class MainWindow : Window
    {
        public MainViewModel ViewModel => DataContext as MainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            InitializePianoKeyboard();
        }

        private void InitializePianoKeyboard()
        {
            // Определим ноты от C3 до B5
            var notesInRange = new List<(string name, double frequency)>
            {
                // Octave 3
                ("C3", 130.81), ("C#3", 138.59), ("D3", 146.83), ("D#3", 155.56), ("E3", 164.81),
                ("F3", 174.61), ("F#3", 185.00), ("G3", 196.00), ("G#3", 207.65), ("A3", 220.00), ("A#3", 233.08), ("B3", 246.94),

                // Octave 4
                ("C4", 261.63), ("C#4", 277.18), ("D4", 293.66), ("D#4", 311.13), ("E4", 329.63),
                ("F4", 349.23), ("F#4", 369.99), ("G4", 392.00), ("G#4", 415.30), ("A4", 440.00), ("A#4", 466.16), ("B4", 493.88),

                // Octave 5
                ("C5", 523.25), ("C#5", 554.37), ("D5", 587.33), ("D#5", 622.25), ("E5", 659.25),
                ("F5", 698.46), ("F#5", 739.99), ("G5", 783.99), ("G#5", 830.61), ("A5", 880.00), ("A#5", 932.33), ("B5", 987.77),

                // Octave 6
                ("C6", 1046.50), ("C#6", 1108.73), ("D6", 1174.66), ("D#6", 1244.51), ("E6", 1318.51),
                ("F6", 1396.91), ("F#6", 1479.98), ("G6", 1567.98), ("G#6", 1661.22), ("A6", 1760.00), ("A#6", 1864.66), ("B6", 1975.53)
            };

            // Размеры
            double whiteKeyWidth = 30;
            double whiteKeyHeight = 120;
            double blackKeyWidth = 20;
            double blackKeyHeight = 70;
            double blackKeyOffsetX = whiteKeyWidth - blackKeyWidth / 2 - 2;

            // Позиция по X
            double x = 10; // Небольшой отступ слева

            // Создаем все белые клавиши
            foreach (var (name, freq) in notesInRange.Where(n => !n.name.Contains('#') && !n.name.Contains('b')))
            {
                var rect = new Rectangle
                {
                    Width = whiteKeyWidth,
                    Height = whiteKeyHeight,
                    Fill = Brushes.White,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Tag = new PianoKeyInfo(name, freq)
                };

                // Подпись для белой клавиши (снизу)
                var label = new TextBlock
                {
                    Text = name,
                    FontSize = 9,
                    Foreground = Brushes.Black,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = System.Windows.VerticalAlignment.Bottom,
                    Margin = new Thickness(0, 0, 0, 5)
                };

                var container = new Grid();
                container.Children.Add(rect);
                container.Children.Add(label);

                container.MouseLeftButtonDown += (sender, e) =>
                {
                    if (rect.Tag is PianoKeyInfo keyInfo)
                    {
                        ViewModel.PianoKeyCommand.Execute(keyInfo);
                    }
                };

                Canvas.SetLeft(container, x);
                Canvas.SetTop(container, 10);
                PianoCanvas.Children.Add(container);
                x += whiteKeyWidth;
            }

            // Чёрные клавиши
            x = 10;

            foreach (var (name, freq) in notesInRange)
            {
                if (!name.Contains('#') && !name.Contains('b'))
                {
                    x += whiteKeyWidth;
                }
                else
                {
                    var baseNote = name[0].ToString();
                    if (baseNote == "E" || baseNote == "B") continue;

                    double blackX = x - whiteKeyWidth + blackKeyOffsetX;

                    var rect = new Rectangle
                    {
                        Width = blackKeyWidth,
                        Height = blackKeyHeight,
                        Fill = Brushes.Black,
                        Tag = new PianoKeyInfo(name, freq)
                    };

                    // Подпись для черной клавиши (сверху, белым цветом)
                    var label = new TextBlock
                    {
                        Text = name,
                        FontSize = 8,
                        Foreground = Brushes.White,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                        VerticalAlignment = System.Windows.VerticalAlignment.Top,
                        Margin = new Thickness(0, 3, 0, 0)
                    };

                    var container = new Grid();
                    container.Children.Add(rect);
                    container.Children.Add(label);

                    container.MouseLeftButtonDown += (sender, e) =>
                    {
                        if (rect.Tag is PianoKeyInfo keyInfo)
                        {
                            ViewModel.PianoKeyCommand.Execute(keyInfo);
                        }
                    };

                    Canvas.SetLeft(container, blackX);
                    Canvas.SetTop(container, 10);
                    PianoCanvas.Children.Add(container);
                }
            }

            PianoCanvas.Width = x + 10; // Добавляем отступ справа
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ViewModel?.OnWindowClosed();
        }
    }
}