using System.Collections.Generic;
using System.Linq;
using PicoGraffiti.Model;
using UnityEngine;

namespace PicoGraffiti.Framework
{
    public class AutoGenerator
    {
        public class Phrase
        {
            public int[] Codes;
            public int[][] Motifs;
            public int[][] SubMotifs;

            public Phrase()
            {
                Codes = GetCodes(Random.Range(0, 5));
                var motifA = Motif(false);
                var motifB = Motif(false);
                Motifs = new[] { motifA, motifB };
                var subMotifA = Motif(true);
                var subMotifB = Motif(true);
                SubMotifs = new[] { subMotifA, subMotifB };
            }

            public int[] Melody()
            {
                var res = new List<int>();
                foreach (var motif in Motifs)
                {
                    foreach (var m in motif)
                    {
                        res.Add(m);
                    }
                }

                return res.ToArray();
            }
            public int[] SubMelody()
            {
                var res = new List<int>();
                foreach (var motif in SubMotifs)
                {
                    foreach (var m in motif)
                    {
                        res.Add(m);
                    }
                }

                return res.ToArray();
            }

            public int[] Base(bool isWalk)
            {
                var res = new List<int>();
                for (var x = 0; x < 8; x++)
                {
                    foreach (var code in Codes)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            res.Add(code + (isWalk && i % 2 == 0 ? 0 : 12));
                            res.Add(code + (isWalk && i % 2 == 0 ? 0 : 12));
                        }
                    }
                }

                return res.ToArray();
            }
        }
        
        public static void Generate()
        {
            AppGlobal.Instance.ScoreRepository.Instance.Score.BPM = 144;
            AppGlobal.Instance.ScoreRepository.Instance.Score.Trans = 2;

            var i = new Phrase();
            var a = new Phrase();
            var b = new Phrase();
            var s = new Phrase();
            var c = new Phrase();
            WritePhrase(i, 0);
            WritePhrase(i, 1);
            WritePhrase(a, 2);
            WritePhrase(a, 3);
            WritePhrase(b, 4);
            WritePhrase(s, 5);
            WritePhrase(s, 6);
            WritePhrase(i, 7);
            WritePhrase(a, 8);
            WritePhrase(a, 9);
            WritePhrase(b, 10);
            WritePhrase(s, 11);
            WritePhrase(s, 12);
            WritePhrase(c, 13);
            WritePhrase(c, 14);
            WritePhrase(s, 15);
            WritePhrase(s, 16);
            WritePhrase(i, 17);
            WritePhrase(i, 18);
        }

        public static int[] GetCodes(int pattern)
        {
            var tonic = new[] { 1, 10 };
            var domi = new[] { 3, 8 };
            var sub = new[] { 6 };

            if (pattern == 0)
            {
                return new[] { Select(tonic), Select(sub), Select(domi), Select(tonic) };
            }

            if (pattern == 1)
            {
                return new[] { Select(sub), Select(tonic), Select(domi), Select(tonic) };
            }

            if (pattern == 3)
            {
                return new[] { Select(tonic), Select(domi), Select(tonic), Select(sub) };
            }

            return new[] { Select(domi), Select(tonic), Select(sub), Select(domi) };
        }

        public static int Select(int[] array)
        {
            return array[Random.Range(0, array.Length)];
        }

        public static int[] Motif(bool reponse)
        {
            var res = new List<int>();
            var scale = new[] { 1, 3, 6, 8, 10 };
            for (int i = 0; i < 8; i++)
            {
                if (i % 2 == 1 && Random.Range(0, 100) < 90)
                {
                    res.Add(-1);
                }
                else if (Random.Range(0, 100) < 30 || (reponse && i < 4))
                {
                    res.Add(-1);
                }
                else
                {
                    res.Add(Select(scale));
                }
            }

            return res.ToArray();
        }

        public static void WritePhrase(Phrase phrase, int indexOffset)
        {
            WriteScore(indexOffset, 0, 4, phrase.Melody());
            WriteScore(indexOffset, 5, 5, phrase.SubMelody());
            // WriteScore(indexOffset, 2, 3, phrase.Base(false));
            WriteScore(indexOffset, 1, 2, phrase.Base(true));
        }

        public static void WriteScore(int indexOffset, int trackIndex, int octave, int[] melody)
        {
            var index = indexOffset * Track.NOTE_GRID_SIZE / 2 * melody.Length;
            foreach (var melo in melody)
            {
                var value = (melo + octave * 12) / 89.0;
                if (melo == -1)
                {
                    index += Track.NOTE_GRID_SIZE / 2;
                    continue;
                }
                for (int i = 0; i < Track.NOTE_GRID_SIZE / 4; i++)
                {
                    if(i < Track.NOTE_GRID_SIZE / 4 - 1)
                    // if (index % (Track.NOTE_GRID_SIZE / 2) < Track.NOTE_GRID_SIZE / 4)
                    {
                        AppGlobal.Instance.ScoreRepository.Instance.Score.Tracks[trackIndex]
                            .SetNote(index, value);
                    }

                    index++;
                }
            }
        }

        public static void Base()
        {
            var index = 0;

            for (int scoreIndex = 0; scoreIndex < 4; scoreIndex++)
            {
                for (int i = 0; i < 8; i++)
                {
                    var codes = GetCodes(Random.Range(0, 5));
                    foreach (var root in codes)
                    {
                        for (int k = 0; k < 8; k++)
                        {
                            var octave = 2 + (k % 2 == 0 ? 0 : 1);
                            var value = (root + octave * 12) / 89.0;
                            for (int j = 0; j < Track.NOTE_GRID_SIZE / 2; j++)
                            {
                                if (index % (Track.NOTE_GRID_SIZE / 2) < Track.NOTE_GRID_SIZE / 4)
                                {
                                    AppGlobal.Instance.ScoreRepository.Instance.Score.Tracks[1]
                                        .SetNote(index, value);
                                }

                                index++;
                            }
                        }
                    }
                }
            }
        }

        public static void Kick()
        {
            var index = 0;
            for (int scoreIndex = 0; scoreIndex < 4; scoreIndex++)
            {
                for (int i = 0; i < 8; i++)
                {
                    var codes = GetCodes(Random.Range(0, 5));
                    foreach (var root in codes)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            var octave = 3;
                            var value = (octave * 12) / 89.0;
                            for (int j = 0; j < Track.NOTE_GRID_SIZE; j++)
                            {
                                if (index % (Track.NOTE_GRID_SIZE) < Track.NOTE_GRID_SIZE / 2)
                                {
                                    AppGlobal.Instance.ScoreRepository.Instance.Score.Tracks[6]
                                        .SetNote(index, value);
                                }

                                index++;
                            }
                        }
                    }
                }
            }
        }

        public static void Snare()
        {
            var index = 0;
            for (int scoreIndex = 0; scoreIndex < 4; scoreIndex++)
            {
                for (int i = 0; i < 8; i++)
                {
                    var codes = GetCodes(Random.Range(0, 5));
                    foreach (var root in codes)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            var octave = 3;
                            var value = (octave * 12) / 89.0;
                            for (int j = 0; j < Track.NOTE_GRID_SIZE; j++)
                            {
                                if (index % (Track.NOTE_GRID_SIZE) > Track.NOTE_GRID_SIZE / 2)
                                {
                                    AppGlobal.Instance.ScoreRepository.Instance.Score.Tracks[8]
                                        .SetNote(index, value);
                                }

                                index++;
                            }
                        }
                    }
                }
            }
        }
    }
}