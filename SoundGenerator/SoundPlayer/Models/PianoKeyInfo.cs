
namespace SoundGen.Models
{
    public class PianoKeyInfo
    {
        public string Name { get; set; }
        public double Frequency { get; set; }

        public PianoKeyInfo() { }

        public PianoKeyInfo(string name, double frequency)
        {
            Name = name;
            Frequency = frequency;
        }
    }
}
