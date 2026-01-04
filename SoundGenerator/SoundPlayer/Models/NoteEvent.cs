
namespace SoundGen
{
    public class NoteEvent
    {
        public double? Frequency { get; set; } // null = пауза
        public int DurationMs { get; set; }

        public NoteEvent(double? frequency, int durationMs)
        {
            Frequency = frequency;
            DurationMs = durationMs;
        }
    }
}
