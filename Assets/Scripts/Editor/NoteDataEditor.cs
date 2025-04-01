using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using BeatSaberClone.Domain;

namespace BeatSaberClone.Editor
{

    // A custom editor window for creating and editing note data in a Beat Saber clone.
    // This editor provides a timeline-based interface where users can:
    // - Visualize and edit notes on a timeline
    // - Create new notes with specific properties
    // - View notes in both timeline and front view perspectives
    // - Save and load note data to/from JSON files

    public class NoteDataEditor : EditorWindow
    {
        // Timeline configuration constants
        private const float MIN_TIMELINE_DURATION = 60f;
        private const float MAX_TIMELINE_DURATION = 600f;
        private const float MIN_ZOOM_LEVEL = 0.1f;
        private const float MAX_ZOOM_LEVEL = 5f;
        private const float ZOOM_STEP = 0.1f;
        private const float DURATION_STEP = 10f;

        // Timeline grid configuration
        private const float MAJOR_TICK_INTERVAL = 1.0f;
        private const float MINOR_TICK_INTERVAL = 0.25f;

        // Note visualization constants
        private const float BASE_NOTE_SIZE = 20f;
        private const float NOTE_LAYER_SIZE_FACTOR = 0.15f;
        private const float NOTE_BRIGHTNESS_FACTOR = 0.25f;

        // Core data
        private List<NoteInfo> _notes = new();
        private Vector2 _scrollPosition;
        private float _time = 0f;
        private int _lineIndex = 0;
        private int _lineLayer = 0;
        private int _type = 0;
        private int _cutDirection = 0;
        private NoteInfo _selectedNote = null;
        private float _selectedTime = 0f;

        // Timeline view settings
        private float _timelineStart = 0f;
        private float _timelineEnd = 300f;
        private float _laneWidth = 100f;
        private float _zoomLevel = 1f;
        private float _timeScale = 1f;

        // Double-click functionality
        private float _lastClickTime = 0f;
        private const float DOUBLE_CLICK_TIME = 0.3f;

        [MenuItem("Beat Saber Clone/Note Data Editor")]
        public static void ShowWindow()
        {
            GetWindow<NoteDataEditor>("Note Data Editor");
        }

        private void OnGUI()
        {
            DrawToolbar();
            DrawTimeline();
            DrawFrontView();
            DrawNoteProperties();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            DrawSaveLoadButtons();
            GUILayout.FlexibleSpace();
            DrawTimelineDurationControls();
            GUILayout.Space(10);
            DrawZoomControls();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSaveLoadButtons()
        {
            if (GUILayout.Button("Save", EditorStyles.toolbarButton)) SaveToJson();
            if (GUILayout.Button("Load", EditorStyles.toolbarButton)) LoadFromJson();
        }

        private void DrawTimelineDurationControls()
        {
            EditorGUILayout.LabelField("Duration:", GUILayout.Width(60));
            if (GUILayout.Button("-10s", EditorStyles.toolbarButton, GUILayout.Width(40)))
            {
                _timelineEnd = Mathf.Max(MIN_TIMELINE_DURATION, _timelineEnd - DURATION_STEP);
            }
            EditorGUILayout.LabelField($"{_timelineEnd:F0}s", GUILayout.Width(40));
            if (GUILayout.Button("+10s", EditorStyles.toolbarButton, GUILayout.Width(40)))
            {
                _timelineEnd = Mathf.Min(MAX_TIMELINE_DURATION, _timelineEnd + DURATION_STEP);
            }
        }

        private void DrawZoomControls()
        {
            EditorGUILayout.LabelField("Zoom:", GUILayout.Width(50));
            if (GUILayout.Button("-", EditorStyles.toolbarButton, GUILayout.Width(30)))
            {
                _zoomLevel = Mathf.Max(MIN_ZOOM_LEVEL, _zoomLevel - ZOOM_STEP);
            }
            EditorGUILayout.LabelField($"{_zoomLevel:F1}x", GUILayout.Width(50));
            if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(30)))
            {
                _zoomLevel = Mathf.Min(MAX_ZOOM_LEVEL, _zoomLevel + ZOOM_STEP);
            }
            if (GUILayout.Button("Reset", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                _zoomLevel = 1f;
            }
        }

        // Draws the main timeline view including grid, notes, and time indicators.
        // The timeline provides a visual representation of the song's notes over time.
        private void DrawTimeline()
        {
            var timelineRect = SetupTimelineView();
            var mousePos = HandleMouseInput(timelineRect);
            DrawTimelineGrid(timelineRect);
            DrawNotes(timelineRect);
            DrawTimePositionIndicator(timelineRect, mousePos);
            GUI.EndScrollView();
        }

        // Sets up the timeline view with proper scrolling and scaling.
        private Rect SetupTimelineView()
        {
            float contentWidth = (_timelineEnd - _timelineStart) * _laneWidth * _zoomLevel;
            Rect viewRect = EditorGUILayout.GetControlRect(false, 300f);
            Rect contentRect = new Rect(0, 0, contentWidth, viewRect.height);

            Vector2 currentScrollPosition = GUI.BeginScrollView(viewRect, _scrollPosition, contentRect, false, false);
            _scrollPosition = currentScrollPosition;

            Rect timelineRect = new Rect(0, 0, contentWidth, viewRect.height);
            GUI.Box(timelineRect, "");
            _timeScale = _laneWidth * _zoomLevel;

            return timelineRect;
        }

        // Handles mouse input for the timeline view, including selection and dragging.
        private Vector2 HandleMouseInput(Rect timelineRect)
        {
            Event e = Event.current;
            Vector2 mousePos = e.mousePosition;
            Vector2 scrollAdjustedMousePos = GUI.matrix.MultiplyPoint3x4(mousePos);

            if (timelineRect.Contains(scrollAdjustedMousePos))
            {
                HandleTimelineMouseEvents(e, scrollAdjustedMousePos);
            }

            return scrollAdjustedMousePos;
        }

        // Processes mouse events for timeline interaction.
        private void HandleTimelineMouseEvents(Event e, Vector2 mousePos)
        {
            if (e.type == EventType.MouseDown)
            {
                float currentTime = Time.realtimeSinceStartup;
                float clickTime = (mousePos.x / _timeScale) + _timelineStart;
                _selectedTime = Mathf.Clamp(clickTime, _timelineStart, _timelineEnd);

                // Check if clicked on a note
                float laneHeight = 300f / 4; // Using the timeline height from SetupTimelineView
                float noteYOffset = (laneHeight - BASE_NOTE_SIZE) / 2;
                float layerOffset = BASE_NOTE_SIZE * 0.8f;

                foreach (var note in _notes)
                {
                    float x = (note._time - _timelineStart) * _timeScale;
                    float y = note._lineIndex * laneHeight + noteYOffset - note._lineLayer * layerOffset;
                    float noteSize = BASE_NOTE_SIZE * (1.0f + (note._lineLayer * NOTE_LAYER_SIZE_FACTOR));

                    Rect noteRect = new Rect(
                        x - noteSize / 2,
                        y,
                        noteSize,
                        noteSize
                    );

                    if (noteRect.Contains(mousePos))
                    {
                        _selectedNote = note;

                        // Check for double click
                        if (currentTime - _lastClickTime < DOUBLE_CLICK_TIME)
                        {
                            // Cycle through layers (0 -> 1 -> 2 -> 0)
                            _selectedNote._lineLayer = (_selectedNote._lineLayer + 1) % 3;
                        }

                        _lastClickTime = currentTime;
                        e.Use();
                        return;
                    }
                }

                // If no note was clicked, deselect current note
                _selectedNote = null;
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && _selectedNote != null)
            {
                // Update selected note's time and lane when dragging
                float dragTime = (mousePos.x / _timeScale) + _timelineStart;
                _selectedNote._time = Mathf.Clamp(dragTime, _timelineStart, _timelineEnd);
                _selectedTime = _selectedNote._time;

                // Calculate lane based on vertical position
                float laneHeight = 300f / 4;
                int newLaneIndex = Mathf.FloorToInt(mousePos.y / laneHeight);
                _selectedNote._lineIndex = Mathf.Clamp(newLaneIndex, 0, 3);

                e.Use();
            }
        }

        // Draws the time position indicator and label at the current selection.
        private void DrawTimePositionIndicator(Rect timelineRect, Vector2 mousePos)
        {
            if (timelineRect.Contains(mousePos))
            {
                float selectedX = (_selectedTime - _timelineStart) * _timeScale;
                DrawTimeIndicatorLine(selectedX, timelineRect.height);
                DrawTimeLabel(selectedX);
            }
        }

        // Draws the vertical line indicating the current time position.
        private void DrawTimeIndicatorLine(float x, float height)
        {
            Color timelineColor = new Color(1f, 1f, 1f, 0.5f);
            EditorGUI.DrawRect(
                new Rect(x - 1, 0, 2, height),
                timelineColor
            );
        }

        // Draws the time label showing the current selection time.
        private void DrawTimeLabel(float x)
        {
            GUIStyle timeStyle = new GUIStyle(EditorStyles.label);
            timeStyle.normal.textColor = Color.white;
            timeStyle.alignment = TextAnchor.UpperCenter;
            Rect timeRect = new Rect(x - 30, 20, 60, 20);
            GUI.Label(timeRect, _selectedTime.ToString("F2"), timeStyle);
        }

        // Draws the timeline grid including major ticks, minor ticks, and lane dividers.
        private void DrawTimelineGrid(Rect viewRect)
        {
            DrawMajorTicks(viewRect);
            DrawMinorTicks();
            DrawLaneDividers(viewRect);
        }

        // Draws major time markers and their labels.
        private void DrawMajorTicks(Rect viewRect)
        {
            for (float t = _timelineStart; t <= _timelineEnd; t += MAJOR_TICK_INTERVAL)
            {
                float x = (t - _timelineStart) * _timeScale;
                DrawMajorTickLabel(x, t);
                DrawMajorTickLine(x, viewRect.height);
            }
        }

        private void DrawMajorTickLabel(float x, float time)
        {
            Rect labelRect = new Rect(x - 15, 0, 30, 20);
            GUI.Label(labelRect, time.ToString("F0"));
        }

        private void DrawMajorTickLine(float x, float height)
        {
            Handles.DrawLine(
                new Vector3(x, 0, 0),
                new Vector3(x, height, 0)
            );
        }

        // Draws minor time markers for more precise time reference.
        private void DrawMinorTicks()
        {
            for (float t = _timelineStart; t <= _timelineEnd; t += MINOR_TICK_INTERVAL)
            {
                float x = (t - _timelineStart) * _timeScale;
                DrawMinorTickLine(x);
            }
        }

        private void DrawMinorTickLine(float x)
        {
            float tickHeight = 10f;
            Handles.DrawLine(
                new Vector3(x, 0, 0),
                new Vector3(x, tickHeight, 0)
            );
        }

        // Draws vertical dividers between lanes.
        private void DrawLaneDividers(Rect viewRect)
        {
            float laneHeight = viewRect.height / 4;
            for (int lane = 0; lane <= 4; lane++)
            {
                float y = lane * laneHeight;
                Handles.DrawLine(
                    new Vector3(0, y, 0),
                    new Vector3(viewRect.width, y, 0)
                );
            }
        }

        // Draws all notes on the timeline, sorted by time and layer.
        private void DrawNotes(Rect timelineRect)
        {
            var sortedNotes = GetSortedNotes();
            float laneHeight = timelineRect.height / 4;
            float noteYOffset = (laneHeight - BASE_NOTE_SIZE) / 2;
            float layerOffset = BASE_NOTE_SIZE * 0.8f;

            foreach (var note in sortedNotes)
            {
                DrawNote(note, laneHeight, noteYOffset, layerOffset);
            }
        }

        // Returns notes sorted by time and layer for proper visualization.
        private List<NoteInfo> GetSortedNotes()
        {
            var sortedNotes = new List<NoteInfo>(_notes);
            sortedNotes.Sort((a, b) =>
            {
                int timeCompare = a._time.CompareTo(b._time);
                if (timeCompare != 0) return timeCompare;
                return a._lineLayer.CompareTo(b._lineLayer);
            });
            return sortedNotes;
        }

        // Draws a single note with appropriate size and color based on its properties.
        private void DrawNote(NoteInfo note, float laneHeight, float noteYOffset, float layerOffset)
        {
            float x = (note._time - _timelineStart) * _timeScale;
            float y = note._lineIndex * laneHeight + noteYOffset - note._lineLayer * layerOffset;
            float noteSize = BASE_NOTE_SIZE * (1.0f + (note._lineLayer * NOTE_LAYER_SIZE_FACTOR));

            Rect noteRect = new Rect(
                x - noteSize / 2,
                y,
                noteSize,
                noteSize
            );

            DrawNoteOutline(note, noteRect);
            DrawNoteBody(note, noteRect);
        }

        // Draws the outline of a note, with special highlighting for selected notes.
        private void DrawNoteOutline(NoteInfo note, Rect noteRect)
        {
            Color outlineColor = note == _selectedNote ? Color.yellow : Color.white;
            float outlineWidth = note == _selectedNote ? 2f : 1f;
            EditorGUI.DrawRect(
                new Rect(
                    noteRect.x - outlineWidth,
                    noteRect.y - outlineWidth,
                    noteRect.width + outlineWidth * 2,
                    noteRect.height + outlineWidth * 2
                ),
                outlineColor
            );
        }

        // Draws the body of a note with color based on its type and layer.
        private void DrawNoteBody(NoteInfo note, Rect noteRect)
        {
            Color noteColor = GetNoteColor(note);
            EditorGUI.DrawRect(noteRect, noteColor);
        }

        // Calculates the color of a note based on its type and layer.
        private Color GetNoteColor(NoteInfo note)
        {
            Color baseColor = note._type == 0 ? Color.red : Color.blue;
            float brightness = 0.5f + (note._lineLayer * NOTE_BRIGHTNESS_FACTOR);
            baseColor.r = Mathf.Min(1f, baseColor.r * brightness);
            baseColor.g = Mathf.Min(1f, baseColor.g * brightness);
            baseColor.b = Mathf.Min(1f, baseColor.b * brightness);
            return baseColor;
        }

        // Saves the current note data to a JSON file.
        private void SaveToJson()
        {
            var container = new NotesContainer { NotesList = _notes };
            string json = JsonUtility.ToJson(container, true);

            // Generate a default filename with current date and time
            string defaultFileName = $"NotesData_{System.DateTime.Now:yyyyMMdd_HHmmss}.json";
            string path = EditorUtility.SaveFilePanel(
                "Save Note Data",
                "Assets/Resources/NotesData",
                defaultFileName,
                "json");

            if (!string.IsNullOrEmpty(path))
            {
                // Ensure the directory exists
                string directory = System.IO.Path.GetDirectoryName(path);
                if (!System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }

                System.IO.File.WriteAllText(path, json);
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Done", "Saved Note Data", "OK");
            }
        }

        // Loads note data from a JSON file.
        private void LoadFromJson()
        {
            string path = EditorUtility.OpenFilePanel(
                "Load Note Data",
                "Assets/Resources",
                "json");

            if (!string.IsNullOrEmpty(path))
            {
                string json = System.IO.File.ReadAllText(path);
                var container = JsonUtility.FromJson<NotesContainer>(json);
                _notes = container.NotesList;
            }
        }

        // Draws the front view of notes, showing their spatial arrangement.
        // This view helps visualize how notes will appear in the game.
        private void DrawFrontView()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Front View", EditorStyles.boldLabel);

            // Front view view area
            Rect viewRect = EditorGUILayout.GetControlRect(false, 150f);
            GUI.Box(viewRect, "");

            // Draw a lane separator line
            float laneWidth = viewRect.width / 4;
            float laneHeight = viewRect.height;
            for (int lane = 0; lane <= 4; lane++)
            {
                float x = lane * laneWidth;
                Handles.DrawLine(
                    new Vector3(x, viewRect.y, 0),
                    new Vector3(x, viewRect.y + laneHeight, 0)
                );
            }

            // Draw a layer separator line
            float layerHeight = laneHeight / 3;
            for (int layer = 0; layer <= 3; layer++)
            {
                float y = viewRect.y + layer * layerHeight;
                Handles.DrawLine(
                    new Vector3(0, y, 0),
                    new Vector3(viewRect.width, y, 0)
                );
            }

            // Draw coordinate labels
            GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
            labelStyle.normal.textColor = Color.white;
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.fontSize = 10;

            for (int layer = 0; layer < 3; layer++)
            {
                for (int index = 0; index < 4; index++)
                {
                    float x = index * laneWidth;
                    float y = viewRect.y + (2 - layer) * layerHeight;
                    Rect labelRect = new Rect(x, y, laneWidth, layerHeight);
                    GUI.Label(labelRect, $"({index},{layer})", labelStyle);
                }
            }

            // Draw notes near the selection time
            float timeWindow = 0.1f; // Time range (seconds) to display
            foreach (var note in _notes)
            {
                if (Mathf.Abs(note._time - _selectedTime) <= timeWindow)
                {
                    float x = note._lineIndex * laneWidth;
                    float y = viewRect.y + (2 - note._lineLayer) * layerHeight; // Position Layer 2 above

                    // Use constant size for all notes
                    float noteSize = 20f;
                    Rect noteRect = new Rect(
                        x + (laneWidth - noteSize) / 2,
                        y + (layerHeight - noteSize) / 2,
                        noteSize,
                        noteSize
                    );

                    // Use simple color based on note type only
                    Color noteColor = note._type == 0 ? Color.red : Color.blue;

                    // Draw notes border
                    Color outlineColor = note == _selectedNote ? Color.yellow : Color.white;
                    float outlineWidth = note == _selectedNote ? 2f : 1f;
                    EditorGUI.DrawRect(new Rect(noteRect.x - outlineWidth, noteRect.y - outlineWidth,
                        noteRect.width + outlineWidth * 2, noteRect.height + outlineWidth * 2), outlineColor);

                    // Draw the Notes body
                    EditorGUI.DrawRect(noteRect, noteColor);

                    // Draw cut direction triangle
                    float triangleSize = noteSize * 0.6f;
                    Vector2 center = new Vector2(noteRect.x + noteRect.width / 2, noteRect.y + noteRect.height / 2);
                    Vector2[] trianglePoints = GetTrianglePoints(center, triangleSize, note._cutDirection);

                    // Draw white triangle
                    Color originalColor = Handles.color;
                    Handles.color = Color.white;
                    for (int i = 0; i < 3; i++)
                    {
                        int nextIndex = (i + 1) % 3;
                        Handles.DrawLine(trianglePoints[i], trianglePoints[nextIndex]);
                    }
                    Handles.color = originalColor;
                }
            }

            EditorGUILayout.EndVertical();
        }

        private Vector2[] GetTrianglePoints(Vector2 center, float size, int cutDirection)
        {
            float halfSize = size / 2;
            Vector2[] points = new Vector2[3];

            switch (cutDirection)
            {
                case 0: // Down to Up
                    points[0] = new Vector2(center.x, center.y - halfSize);
                    points[1] = new Vector2(center.x - halfSize, center.y + halfSize);
                    points[2] = new Vector2(center.x + halfSize, center.y + halfSize);
                    break;
                case 1: // Up to Down
                    points[0] = new Vector2(center.x, center.y + halfSize);
                    points[1] = new Vector2(center.x - halfSize, center.y - halfSize);
                    points[2] = new Vector2(center.x + halfSize, center.y - halfSize);
                    break;
                case 2: // Right to Left
                    points[0] = new Vector2(center.x - halfSize, center.y);
                    points[1] = new Vector2(center.x + halfSize, center.y - halfSize);
                    points[2] = new Vector2(center.x + halfSize, center.y + halfSize);
                    break;
                case 3: // Left to Right
                    points[0] = new Vector2(center.x + halfSize, center.y);
                    points[1] = new Vector2(center.x - halfSize, center.y - halfSize);
                    points[2] = new Vector2(center.x - halfSize, center.y + halfSize);
                    break;
            }

            return points;
        }

        // Draws the note properties panel for editing selected notes or creating new ones.
        private void DrawNoteProperties()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Note Properties", EditorStyles.boldLabel);

            if (_selectedNote != null)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Selected Note", EditorStyles.boldLabel);
                if (GUILayout.Button("Deselect", GUILayout.Width(80)))
                {
                    _selectedNote = null;
                }
                EditorGUILayout.EndHorizontal();

                _selectedNote._time = EditorGUILayout.FloatField("Time", _selectedNote._time);
                _selectedNote._lineIndex = EditorGUILayout.IntSlider("Line Index", _selectedNote._lineIndex, 0, 3);
                _selectedNote._lineLayer = EditorGUILayout.IntSlider("Line Layer", _selectedNote._lineLayer, 0, 2);
                _selectedNote._type = EditorGUILayout.IntSlider("Type", _selectedNote._type, 0, 1);
                _selectedNote._cutDirection = EditorGUILayout.IntSlider("Cut Direction", _selectedNote._cutDirection, 0, 3);

                if (GUILayout.Button("Delete Note"))
                {
                    _notes.Remove(_selectedNote);
                    _selectedNote = null;
                }
            }
            else
            {
                // Input fields for creating new notes
                GUILayout.Label("New Note", EditorStyles.boldLabel);
                _time = EditorGUILayout.FloatField("Time", _time);
                _lineIndex = EditorGUILayout.IntSlider("Line Index", _lineIndex, 0, 3);
                _lineLayer = EditorGUILayout.IntSlider("Line Layer", _lineLayer, 0, 2);
                _type = EditorGUILayout.IntSlider("Type", _type, 0, 1);
                _cutDirection = EditorGUILayout.IntSlider("Cut Direction", _cutDirection, 0, 3);

                if (GUILayout.Button("Add Note"))
                {
                    _notes.Add(new NoteInfo
                    {
                        _time = _time,
                        _lineIndex = _lineIndex,
                        _lineLayer = _lineLayer,
                        _type = _type,
                        _cutDirection = _cutDirection
                    });
                }
            }

            EditorGUILayout.EndVertical();
        }
    }
}
