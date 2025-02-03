using System;
using System.Collections.Generic;

namespace BeatSaberClone.Domain
{
    public interface INotesRepository : IDisposable
    {
        List<NoteInfo> LoadNotesData();
    }
}
