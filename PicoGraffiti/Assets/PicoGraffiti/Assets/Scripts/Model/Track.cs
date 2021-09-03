using System.Collections.Generic;
using System.Linq;
using PicoGraffiti.Framework;

namespace PicoGraffiti.Model
{
    public class Track
    {
        public SortedDictionary<int, Note> Notes = new SortedDictionary<int, Note>();
        public Score ParentScore { get; private set; }
        public WaveType WaveType { get; private set; }
        public const int NoteGridSize = 128;
        
        public Wave Wave { get; private set; }

        public Track(Score score, WaveType waveType)
        {
            Wave = new Wave();
            Wave.Initialize();
            ParentScore = score;
            WaveType = waveType;
        }

        public void SetNote(int index, double melo, double vol = 1.0)
        {
            Note note = null;
            if (Notes.ContainsKey(index))
            {
                note = Notes[index];
            }
            else {
                Notes[index] = new Note();
                note = Notes[index];
            }

            note.Melo = melo;
            note.Vol = vol;
            note.WaveType = WaveType;
        }

        public void RemoveNote(int index)
        {
            if (!Notes.ContainsKey(index)) return;
            Notes.Remove(index);
        }
        
        public Note GetNote(long index)
        {
            var bpmRate = 60.0 / (ParentScore.BPM * NoteGridSize);
            var len = bpmRate * Wave.SAMPLE_RATE;
            var i = (int)(index / len);
            if (!Notes.ContainsKey(i)) return null;
            return Notes[i];
        }

        public int GetSize()
        {
            return Notes.Last().Key;
        }
    }
}