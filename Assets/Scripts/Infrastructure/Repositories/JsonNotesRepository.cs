using System;
using System.Collections.Generic;
using UnityEngine;
using BeatSaberClone.Domain;
using Zenject;

namespace BeatSaberClone.Infrastructure
{
    public class JsonNotesRepository : INotesRepository
    {
        private readonly ILoggerService _logger;

        [Inject]
        public JsonNotesRepository(ILoggerService logger)
        {
            _logger = logger;
        }

        public void Dispose()
        {
            _logger.Dispose();
        }

        public List<NoteInfo> LoadNotesData()
        {
            try
            {
                TextAsset notesData = Resources.Load<TextAsset>("NotesData");
                if (notesData == null)
                {
                    _logger.LogError("Notes data not found in Resources!");
                    return new List<NoteInfo>();
                }
                return JsonUtility.FromJson<NotesContainer>(notesData.text).NotesList;
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to load notes: {e.Message}");
                return new List<NoteInfo>();
            }
        }
    }
}
