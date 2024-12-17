using BeatSaberClone.Domain;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Zenject;

namespace BeatSaberClone.UseCase
{
    public sealed class NotesManagementUseCase : INotesManagementUseCase
    {
        private Queue<(float spawnTime, NoteInfo note)> _scheduledNotes;
        public IReadOnlyCollection<(float spawnTime, NoteInfo note)> ScheduledNotes => _scheduledNotes;

        public float SpawnToPlayerDistance { get; private set; }

        private readonly INotesRepository _notesRepository;
        private readonly IDistanceCalculator _distanceCalculator;
        private readonly INoteScheduler _noteScheduler;
        private readonly INoteSpawnService _noteSpawnService;
        private readonly ILoggerService _logger;

        [Inject]
        public NotesManagementUseCase(
            INotesRepository notesRepository,
            IDistanceCalculator distanceCalculator,
            INoteScheduler noteScheduler,
            INoteSpawnService noteSpawnService,
            ILoggerService logger)
        {
            _notesRepository = notesRepository;
            _distanceCalculator = distanceCalculator;
            _noteScheduler = noteScheduler;
            _noteSpawnService = noteSpawnService;
            _logger = logger;
        }

        public void Dispose()
        {
            //_logger.Log("Dispose NotesManagementUseCase");
            _notesRepository.Dispose();
            _distanceCalculator.Dispose();
            _noteScheduler.Dispose();
            _noteSpawnService.Dispose();
            _logger.Dispose();
        }

        public void SetDistance(Transform spawnPoint, Transform playerPoint)
        {
            SpawnToPlayerDistance = _distanceCalculator.CalculateDistance(spawnPoint, playerPoint);
            if (SpawnToPlayerDistance == 0f && (spawnPoint == null || playerPoint == null))
            {
                _logger.LogError("Spawn point or player point is null.");
            }
        }

        public void PrepareNotes(float boxMoveSpeed)
        {
            if (boxMoveSpeed <= 0)
            {
                _logger.LogError("Box move speed must be greater than 0.");
                return;
            }

            var notes = _notesRepository.LoadNotesData();
            var scheduledNotes = _noteScheduler.ScheduleNotes(notes, SpawnToPlayerDistance, boxMoveSpeed);
            _scheduledNotes = new Queue<(float spawnTime, NoteInfo note)>(scheduledNotes);
        }

        public async UniTask<List<NoteInfo>> GetNotesToSpawn(float currentTime, CancellationToken ct)
        {
            if (currentTime < 0)
            {
                _logger.LogError("Current time cannot be negative.");
            }

            return await UniTask.RunOnThreadPool(() =>
            {
                return _noteSpawnService.GetNotesToSpawn(_scheduledNotes, currentTime);
            }, cancellationToken: ct);
        }

        public void RemoveSpawnedNote(NoteInfo note)
        {
            if (note == null)
            {
                _logger.LogError("Note to remove is null.");
                return;
            }
        }
    }
}
