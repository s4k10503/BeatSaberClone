using System.Collections.Generic;

namespace BeatSaberClone.Domain
{
    public sealed class NoteScheduler : INoteScheduler
    {
        // Calculate the notes generation schedule that supports the two -stage moving speed
        public List<(float spawnTime, NoteInfo note)> ScheduleNotes(
            IEnumerable<NoteInfo> notes,
            float totalDistance,
            float initialSpeed,
            float finalSpeed,
            float slowDownDistanceFromPlayer)
        {
            var scheduledNotes = new List<(float spawnTime, NoteInfo note)>();
            if (initialSpeed <= 0 || finalSpeed <= 0)
                return scheduledNotes;

            // Fast phase travel distance (from SPAWN point to player, until just before late)
            float fastPhaseDistance = totalDistance - slowDownDistanceFromPlayer;

            // TimeToreachPlayer once declares once with a scope of the entire method
            float timeToReachPlayer;
            if (fastPhaseDistance < 0)
            {
                // If SlowdownDistanceFromplayer is larger than the overall distance, always move at a slow speed.
                timeToReachPlayer = totalDistance / finalSpeed;
            }
            else
            {
                // Overall travel time = fast phase time + slow phase time
                timeToReachPlayer = fastPhaseDistance / initialSpeed + slowDownDistanceFromPlayer / finalSpeed;
            }

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
