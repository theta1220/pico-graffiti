using System;
using System.Collections.Generic;
using System.Linq;
using PicoGraffiti.Framework;
using Stocker.Framework;
using UnityEngine;

namespace PicoGraffiti.Model
{
    [Serializable]
    public class Track : ICloneable<Track>
    {
        public const int NOTE_GRID_SIZE = 128 / 2;

        public Dictionary<int, Note> Notes = null;
        public ulong Id { get; private set; }
        public Score ParentScore { get; set; }
        public WaveType WaveType { get; set; }
        public WaveType OverrideWaveType { get; set; }
        public double OverrideWaveTime { get; set; }
        public WaveType SecondOverrideWaveType { get; set; }
        public double SecondOverrideWaveTime { get; set; }
        public bool IsKick { get; set; }
        public bool IsChorus { get; set; }
        public bool IsCode { get; set; }
        public int Harmony { get; set; }
        public double Pan { get; set; }
        
        [field: NonSerialized] public Wave Wave { get; set; }

        public Track(
            ulong id, Score score,
            WaveType waveType,
            WaveType overrideWaveType, double overrideWaveTime,
            WaveType secondOverrideWaveType, double secondOverrideWaveTime,
            bool isKick,
            bool isChorus,
            bool isCode,
            int harmony,
            double pan)
        {
            Id = id;
            ParentScore = score;
            WaveType = waveType;
            Notes = new Dictionary<int, Note>();
            OverrideWaveType = overrideWaveType;
            OverrideWaveTime = overrideWaveTime;
            SecondOverrideWaveType = secondOverrideWaveType;
            SecondOverrideWaveTime = secondOverrideWaveTime;
            IsKick = isKick;
            IsChorus = isChorus;
            IsCode = isCode;
            Harmony = harmony;
            Pan = pan;
        }

        public void Initialize()
        {
            Wave = new Wave(Id);
        }

        public Track DeepClone()
        {
            var obj = new Track(
                Id, ParentScore,
                WaveType, OverrideWaveType, OverrideWaveTime,
                SecondOverrideWaveType, SecondOverrideWaveTime,
                IsKick, IsChorus, IsCode, Harmony, Pan);
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
            // var swingRate = 1.0;
            // var swingSize = 16.0;
            // if (index / len % NOTE_GRID_SIZE < NOTE_GRID_SIZE / 2.0)
            // {
            //     index = (long)(Math.Floor(index / (len * NOTE_GRID_SIZE)) * (len * NOTE_GRID_SIZE)
            //                    + index % (len * NOTE_GRID_SIZE) * ((swingSize - swingRate) / swingSize));
            // }
            // else
            // {
            //     index = (long)(Math.Floor(index / (len * NOTE_GRID_SIZE)) * (len * NOTE_GRID_SIZE)
            //                    + index % (len * NOTE_GRID_SIZE) * ((swingSize + swingRate) / swingSize));
            // }
            var i = (int)(index / len);
            if (!Notes.ContainsKey(i)) return null;
            return Notes[i];
        }

        public int GetSize()
        {
            if (Notes.Count == 0) return 0;
            return Notes.Max(_ => _.Key);
        }
    }
}