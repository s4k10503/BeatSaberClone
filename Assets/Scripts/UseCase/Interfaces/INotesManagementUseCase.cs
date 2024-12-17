using System.Collections.Generic;
using UnityEngine;
using BeatSaberClone.Domain;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace BeatSaberClone.UseCase
{
    public interface INotesManagementUseCase : IDisposable
    {
        void SetDistance(Transform spawnPoint, Transform playerPoint);
        void PrepareNotes(float boxMoveSpeed);
        UniTask<List<NoteInfo>> GetNotesToSpawn(float currentTime, CancellationToken ct);
        void RemoveSpawnedNote(NoteInfo note);
    }
}
