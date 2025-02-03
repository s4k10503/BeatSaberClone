using System.Collections.Generic;

namespace BeatSaberClone.Domain
{
    public sealed class NoteSpawnService : INoteSpawnService
    {
        public List<NoteInfo> GetNotesToSpawn(
            Queue<(float spawnTime, NoteInfo note)> scheduledNotes, float currentTime)
        {
            if (currentTime < 0)
                return new List<NoteInfo>();

            var notesToSpawn = new List<NoteInfo>();

            while (scheduledNotes.Count > 0 && currentTime >= scheduledNotes.Peek().spawnTime)
            {
                var note = scheduledNotes.Dequeue().note;
                notesToSpawn.Add(note);
            }

            return notesToSpawn;
        }

        public void Dispose()
        {
        }
    }
}
