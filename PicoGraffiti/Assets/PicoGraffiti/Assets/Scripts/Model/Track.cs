using System.Collections.Generic;

namespace PicoGraffiti.Model
{
    public class Track
    {
        public Dictionary<int, Note> Notes = new Dictionary<int, Note>();

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
        }
    }
}