# MARKOV-CHAIN

## SoundGenerator

🎵 Musical Note Generator & Player is a desktop application for generating, playing, and editing musical melodies using text-based note notation. Built with C# WPF, it combines a simple piano keyboard interface with Markov chain-based melody generation.

🎬 Video Demos
- YouTube:
- RuTube:

✨ Key Features
- 🎹 Visual Piano Keyboard - Interactive keyboard with 4 octaves (C3 to B6) for note input
- 📝 Text-Based Melody Input - Supports note notation like C/300 C#/200 D/400 (note/duration in ms)
- 🔊 Real-Time Audio Generation - Generates WAV audio on-the-fly using sine waves with ADSR envelopes
- 🤖 AI Melody Generation - Markov chain algorithm learns from existing melodies and creates new ones
- 💾 Melody Management - Save, load, and organize melodies in the application directory
- 🎚️ Audio Controls - Volume slider and tempo/duration adjustment
- 📊 Console Output - Real-time feedback and generation logs

🎼 Supported Notation
- Notes: C, C#, Db, D, D#, Eb, E, F, F#, Gb, G, G#, Ab, A, A#, Bb, B
- Octaves: 3, 4, 5, 6 (default is 4)
- Durations: /300 for 300ms duration (optional, uses default if omitted)
- Rests: _ or 0 for silence
- Example: C4/200 D4/300 E4/400 F4/500

🛠️ Technical Stack
- Language: C# (.NET)
- UI Framework: WPF (XAML)
- Audio: System.Media.SoundPlayer with custom WAV generation
- Algorithm: Markov chain (bigram model) for melody generation
- Architecture: MVVM pattern with ViewModel binding



<img width="874" height="558" alt="image" src="https://github.com/user-attachments/assets/6024c966-34d8-42ef-b32f-b25aef2bc8b6" />

