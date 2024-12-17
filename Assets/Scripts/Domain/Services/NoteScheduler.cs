using System.Collections.Generic;

namespace BeatSaberClone.Domain
{
    public class NoteScheduler : INoteScheduler
    {
        public List<(float spawnTime, NoteInfo note)> ScheduleNotes(
            IEnumerable<NoteInfo> notes, float distance, float boxMoveSpeed)
        {
            var scheduledNotes = new List<(float spawnTime, NoteInfo note)>();
            if (boxMoveSpeed <= 0)
                return scheduledNotes;

            float timeToReachPlayer = distance / boxMoveSpeed;

            foreach (var note in notes)
            {
                if (note._time < timeToReachPlayer)
                    continue;

                float spawnTime = note._time - timeToReachPlayer;
                scheduledNotes.Add((spawnTime, note));
            }

            return scheduledNotes;
        }

        public void Dispose()
        {

        }
    }
}
