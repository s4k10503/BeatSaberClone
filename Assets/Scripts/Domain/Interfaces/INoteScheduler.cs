using System;
using System.Collections.Generic;

namespace BeatSaberClone.Domain
{
    public interface INoteScheduler : IDisposable
    {
        List<(float spawnTime, NoteInfo note)> ScheduleNotes(
            IEnumerable<NoteInfo> notes, float distance, float boxMoveSpeed);
    }
}