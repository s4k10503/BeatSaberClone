using BeatSaberClone.Domain;
using Cysharp.Threading.Tasks;
using System;
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

        private float _initialMoveSpeed;
        private float _finalMoveSpeed;
        private float _slowDownDistance;

        private readonly INotesRepository _notesRepository;
        private readonly IDistanceCalculator _distanceCalculator;
        private readonly INoteScheduler _noteScheduler;
        private readonly INoteSpawnService _noteSpawnService;

        [Inject]
        public NotesManagementUseCase(
            INotesRepository notesRepository,
            IDistanceCalculator distanceCalculator,
            INoteScheduler noteScheduler,
            INoteSpawnService noteSpawnService,
            [Inject(Id = "BoxInitialMoveSpeed")] float initialMoveSpeed,
            [Inject(Id = "BoxFinalMoveSpeed")] float finalMoveSpeed,
            [Inject(Id = "BoxSlowDownDistance")] float slowDownDistance)
        {
            _notesRepository = notesRepository;
            _distanceCalculator = distanceCalculator;
            _noteScheduler = noteScheduler;
            _noteSpawnService = noteSpawnService;
            _initialMoveSpeed = initialMoveSpeed;
            _finalMoveSpeed = finalMoveSpeed;
            _slowDownDistance = slowDownDistance;
        }

        public void Dispose()
        {
            _notesRepository.Dispose();
            _distanceCalculator.Dispose();
            _noteScheduler.Dispose();
            _noteSpawnService.Dispose();
        }

        public void SetDistance(Transform spawnPoint, Transform playerPoint)
        {
            SpawnToPlayerDistance = _distanceCalculator.CalculateDistance(spawnPoint, playerPoint);
        }

        public void PrepareNotes()
        {
            if (_initialMoveSpeed <= 0 || _finalMoveSpeed <= 0)
            {
                throw new ArgumentException("Box move speeds must be greater than 0.");
            }

            var notes = _notesRepository.LoadNotesData();
            // To Notescheduler, pass information on the overall distance, fast speed, slow speed, slow distance
            var scheduledNotes = _noteScheduler.ScheduleNotes(
                notes,
                SpawnToPlayerDistance,
                _initialMoveSpeed,
                _finalMoveSpeed,
                _slowDownDistance
            );
            _scheduledNotes = new Queue<(float spawnTime, NoteInfo note)>(scheduledNotes);
        }

        public async UniTask<List<NoteInfo>> GetNotesToSpawnAsync(float currentTime, CancellationToken ct)
        {
            return await UniTask.RunOnThreadPool(() =>
            {
                if (currentTime < 0)
                {
                    throw new ArgumentException("Current time cannot be negative.");
                }

                return _noteSpawnService.GetNotesToSpawn(_scheduledNotes, currentTime);
            }, cancellationToken: ct);
        }
    }
}
