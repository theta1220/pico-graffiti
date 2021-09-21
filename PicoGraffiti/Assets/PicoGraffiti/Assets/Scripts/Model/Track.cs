using System;
using System.Collections.Generic;
using System.Linq;
using PicoGraffiti.Framework;
using Stocker.Framework;

namespace PicoGraffiti.Model
{
    [Serializable]
    public class Track : ICloneable<Track>
    {
        public const int NOTE_GRID_SIZE = 128 / 2;

        public Dictionary<int, Note> Notes = null;
        public ulong Id { get; private set; }
        public Score ParentScore { get; set; }
        public WaveType WaveType { get; private set; }
        public WaveType OverrideWaveType { get; set; }
        
        [field: NonSerialized] public Wave Wave { get; set; }

        public Track(ulong id, Score score, WaveType waveType, WaveType overrideWaveType = WaveType.None)
        {
            Id = id;
            Wave = new Wave(Id);
            ParentScore = score;
            WaveType = waveType;
            Notes = new Dictionary<int, Note>();
            OverrideWaveType = overrideWaveType;
        }

        public Track DeepClone()
        {
            var obj = new Track(Id, ParentScore, WaveType);
            foreach (var note in Notes)
            {
                obj.Notes.Add(note.Key, note.Value.DeepClone());
            }

            return obj;
        }

        public void SetNote(int index, double melo)
        {
            Note note = null;
            if (Notes.ContainsKey(index))
            {
                note = Notes[index];
            }
            else
            {
                Notes[index] = new Note();
                note = Notes[index];
            }

            note.Melo = melo;
            note.WaveType = WaveType;
        }

        public void SetNoteVolume(int index, double vol)
        {
            Note note = null;
            if (Notes.ContainsKey(index))
            {
                note = Notes[index];
            }
            else
            {
                return;
            }

            note.Vol = vol;
        }

        public void RemoveNote(int index)
        {
            if (!Notes.ContainsKey(index)) return;
            Notes.Remove(index);
        }
        
        public Note GetNote(long index)
        {
            var bpmRate = 60.0 / (ParentScore.BPM * NOTE_GRID_SIZE);
            var len = bpmRate * Wave.SAMPLE_RATE;
            var i = (int)(index / len);
            if (!Notes.ContainsKey(i)) return null;
            return Notes[i];
        }

        public int GetSize()
        {
            if (Notes.Count == 0) return 0;
            return Notes.Last().Key;
        }
    }
}