using System;
using System.Collections.Generic;

namespace BeatSaberClone.Domain
{
    public interface INoteSpawnService : IDisposable
    {
        List<NoteInfo> GetNotesToSpawn(
            Queue<(float spawnTime, NoteInfo note)> scheduledNotes, float currentTime);
    }
}