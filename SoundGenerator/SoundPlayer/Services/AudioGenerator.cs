using System.IO;

namespace SoundGen.Services
{
    public static class AudioGenerator
    {
        /// <summary>
        /// Генерирует WAV-поток из списка нот.
        /// </summary>
        /// <param name="notes">Список нот с частотой и длительностью</param>
        /// <param name="volume">Громкость от 0.0 до 1.0</param>
        /// <param name="sampleRate">Частота дискретизации (по умолчанию 44100 Гц)</param>
        /// <returns>MemoryStream с готовым WAV-файлом</returns>
        public static MemoryStream GenerateWavStream(IList<NoteEvent> notes, double volume = 0.6, int sampleRate = 44100)
        {
            if (notes == null || notes.Count == 0)
                throw new ArgumentException("Список нот не может быть пустым.", nameof(notes));

            using var dataStream = new MemoryStream();

            for (int i = 0; i < notes.Count; i++)
            {
                var note = notes[i];
                if (note.Frequency.HasValue)
                {
                    AppendTone(dataStream, note.Frequency.Value, note.DurationMs, sampleRate, volume);

                    // Добавляем микро-паузу между нотами (5 мс) для предотвращения щелчков
                    if (i < notes.Count - 1)
                    {
                        AppendFadeOut(dataStream, 5, sampleRate);
                    }
                }
                else
                {
                    AppendSilence(dataStream, note.DurationMs, sampleRate);
                }
            }

            byte[] audioData = dataStream.ToArray();
            int dataSize = audioData.Length;

            var wavStream = new MemoryStream();
            WriteWavHeader(wavStream, dataSize, sampleRate);
            wavStream.Write(audioData, 0, dataSize);
            wavStream.Position = 0;

            return wavStream;
        }

        private static void AppendFadeOut(Stream stream, int durationMs, int sampleRate)
        {
            int samples = durationMs * sampleRate / 1000;
            if (samples <= 0) return;

            using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true);
            for (int i = 0; i < samples; i++)
            {
                double fade = 1.0 - (double)i / samples;
                short sample = (short)(0 * fade); // Плавно уменьшаем до тишины
                writer.Write(sample);
            }
        }
        // =============== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ===============

        private static void AppendTone(Stream stream, double frequency, int durationMs, int sampleRate, double volume)
        {
            int samples = durationMs * sampleRate / 1000;

            if (samples <= 0) return;

            using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true);

            // Длительности частей огибающей (можно настроить под ваши нужды)
            int attackMs = 5;      // Атака
            int decayMs = 5;       // Спад
            int releaseMs = 10;    // Затухание

            int attackSamples = attackMs * sampleRate / 1000;
            int decaySamples = decayMs * sampleRate / 1000;
            int releaseSamples = releaseMs * sampleRate / 1000;
            int sustainSamples = samples - attackSamples - decaySamples - releaseSamples;

            // Защита от отрицательных значений
            if (sustainSamples < 0)
            {
                // Для очень коротких нот делаем упрощенную огибающую
                attackSamples = samples / 3;
                releaseSamples = samples / 3;
                decaySamples = samples - attackSamples - releaseSamples;
                sustainSamples = 0;
            }

            double maxAmplitude = 32767 * Math.Min(volume, 1.0);

            for (int i = 0; i < samples; i++)
            {
                double t = (double)i / sampleRate;
                double wave = Math.Sin(2 * Math.PI * frequency * t);

                // ADSR-огибающая (Attack-Decay-Sustain-Release)
                double envelope = 0.0;

                if (i < attackSamples)
                {
                    // Атака: линейное нарастание от 0 до 1
                    envelope = (double)i / attackSamples;
                }
                else if (i < attackSamples + decaySamples)
                {
                    // Спад: от 1 до sustain level (0.8)
                    double decayProgress = (double)(i - attackSamples) / decaySamples;
                    envelope = 1.0 - 0.2 * decayProgress; // Плавно опускаемся до 0.8
                }
                else if (i < attackSamples + decaySamples + sustainSamples)
                {
                    // Сустейн: постоянный уровень
                    envelope = 0.8;
                }
                else
                {
                    // Затухание: линейное уменьшение до 0
                    double releaseProgress = (double)(i - (attackSamples + decaySamples + sustainSamples)) / releaseSamples;
                    envelope = 0.8 * (1.0 - releaseProgress);
                }

                double amplitude = wave * maxAmplitude * envelope;

                short sample = (short)Math.Max(-32768, Math.Min(32767, amplitude));
                writer.Write(sample);
            }
        }
        private static void AppendSilence(Stream stream, int durationMs, int sampleRate)
        {
            int samples = durationMs * sampleRate / 1000;
            using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true);
            for (int i = 0; i < samples; i++)
            {
                writer.Write((short)0);
            }
        }

        private static void WriteWavHeader(Stream stream, int dataSize, int sampleRate)
        {
            int channels = 1;                    // моно
            int bitsPerSample = 16;
            int byteRate = sampleRate * channels * (bitsPerSample / 8);
            int blockAlign = channels * (bitsPerSample / 8);
            int fileSize = 36 + dataSize;        // 44 - 8 = 36 (без "RIFF" и размера)

            using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true);
            // RIFF header
            writer.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"));
            writer.Write(fileSize);
            writer.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"));

            // fmt chunk
            writer.Write(System.Text.Encoding.UTF8.GetBytes("fmt "));
            writer.Write(16);                    // Subchunk1Size
            writer.Write((short)1);              // AudioFormat (1 = PCM)
            writer.Write((short)channels);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write((short)blockAlign);
            writer.Write((short)bitsPerSample);

            // data chunk
            writer.Write(System.Text.Encoding.UTF8.GetBytes("data"));
            writer.Write(dataSize);
        }
    }
}