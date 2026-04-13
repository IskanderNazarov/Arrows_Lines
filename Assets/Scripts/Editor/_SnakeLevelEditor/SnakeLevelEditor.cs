using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using _ScriptableObjects._LevelData;

public class SnakeLevelEditor : EditorWindow {
    private enum EditorState { Start, SetupGrid, Editing }

    private EditorState _currentState = EditorState.Start;

    private int _levelIndex = 0;
    private Vector2Int _gridSize = new Vector2Int(6, 6);
    private List<SnakeConfig> _snakes = new List<SnakeConfig>();

    private LevelData _loadedLevelData;
    private int _activeSnakeIndex = -1;
    
    private Vector2 _scrollPosition;
    private Vector2 _levelsScrollPos;
    
    private bool _hasUnsavedChanges = false; 
    private string _currentLevelName = "";
    private string _lastAutoName = ""; 

    private List<LevelData> _allLevelsCache = new List<LevelData>();

    // --- НАСТРОЙКИ ВИЗУАЛА РЕДАКТОРА ---
    private float _editorLineWidth = 0.45f;
    private float _editorDotSize = 0.45f;
    private bool _solidLines = true; 
    private Color _gridBgColor = new Color(0.2f, 0.2f, 0.2f); 
    
    // --- ПЕРЕМЕННЫЕ ДЛЯ РАЗДЕЛИТЕЛЯ (SPLITTER) ---
    private float _settingsColumnWidth = 430f; 
    private bool _isResizingSettings = false;

    // --- НАСТРОЙКИ ГЕНЕРАТОРА ---
    private bool _showGenerator = false;
    private int _minSnakeLength = 3;
    private int _maxSnakeLength = 6;
    private float _targetFillPercentage = 1.0f; 
    private float _twistFactor = 0.5f; 
    private AnimationCurve _lengthDistribution = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    // --- НАСТРОЙКИ БАТЧ-ГЕНЕРАТОРА (МАССОВОЕ СОЗДАНИЕ) ---
    private bool _showBatchGenerator = true; 
    private int _batchCount = 5;
    private bool _batchUseTemplates = false; 
    
    private int _globalTemplateCounter = 0; 
    
    private Vector2Int _batchGridWidthBounds = new Vector2Int(15, 25);
    private Vector2Int _batchGridHeightBounds = new Vector2Int(20, 35);
    private Vector2Int _batchMinLengthBounds = new Vector2Int(4, 7);
    private Vector2Int _batchMaxLengthBounds = new Vector2Int(15, 30);
    private float _batchTwistMin = 0.3f;
    private float _batchTwistMax = 0.8f;
    private float _batchFillMin = 0.95f;
    private float _batchFillMax = 1.0f;

    // --- ПАТТЕРН (ПОДСВЕТКА РИСУНКА) ---
    private bool _showPatternTool = false;
    private string _patternText = "";
    private bool _invertPattern = false; 
    private HashSet<Vector2Int> _patternCells = new HashSet<Vector2Int>();

    [MenuItem("Tools/Snake Level Editor")]
    public static void ShowWindow() {
        var window = GetWindow<SnakeLevelEditor>("Level Editor");
        window.minSize = new Vector2(1200, 700); 
    }

    private void OnEnable() {
        RefreshLevelsList();
        _globalTemplateCounter = EditorPrefs.GetInt("SnakeLevelEditor_TemplateCounter", 0);
        _settingsColumnWidth = EditorPrefs.GetFloat("SnakeLevelEditor_SettingsWidth", 430f);
    }

    private void OnDisable() {
        EditorPrefs.SetInt("SnakeLevelEditor_TemplateCounter", _globalTemplateCounter);
        EditorPrefs.SetFloat("SnakeLevelEditor_SettingsWidth", _settingsColumnWidth);
    }

    private void OnGUI() {
        switch (_currentState) {
            case EditorState.Start: DrawStartState(); break;
            case EditorState.SetupGrid: DrawSetupGridState(); break;
            case EditorState.Editing: DrawEditingState(); break;
        }
    }

    private void MarkAsDirty() {
        _hasUnsavedChanges = true;
        GUI.changed = true;
    }

    // ==========================================
    // СТЕЙТ 1: СТАРТ
    // ==========================================
    private void DrawStartState() {
        GUILayout.Space(20);
        GUILayout.Label("Snake Level Editor", EditorStyles.boldLabel);
        GUILayout.Space(20);

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical("box", GUILayout.Width(450));
        GUILayout.Label("Single Level Management", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        if (GUILayout.Button("Start new level", GUILayout.Height(40))) {
            _snakes.Clear();
            _patternCells.Clear();
            _patternText = "";
            _currentLevelName = "";
            _lastAutoName = "";

            string dbPath = "Assets/Scriptable/_LevelsDatabase/LevelsDatabase.asset";
            LevelsDatabase db = AssetDatabase.LoadAssetAtPath<LevelsDatabase>(dbPath);
            _levelIndex = (db != null && db.levelDatas != null) ? db.levelDatas.Count : 0;

            _loadedLevelData = null;
            _hasUnsavedChanges = false;
            
            GUI.FocusControl(null);
            EditorGUIUtility.editingTextField = false;
            
            _currentState = EditorState.SetupGrid;
        }

        GUILayout.Space(20);
        GUILayout.Label("...or edit existing level:");
        
        _loadedLevelData = (LevelData)EditorGUILayout.ObjectField("Load Level", _loadedLevelData, typeof(LevelData), false);

        if (_loadedLevelData != null && GUILayout.Button("Edit Level", GUILayout.Height(30))) {
            LoadLevel(_loadedLevelData);
            _currentState = EditorState.Editing;
        }
        GUILayout.EndVertical();

        GUILayout.Space(20);

        GUILayout.BeginVertical(GUILayout.Width(450));
        DrawBatchGeneratorSection();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    // ==========================================
    // СТЕЙТ 2: НАСТРОЙКА СЕТКИ
    // ==========================================
    private void DrawSetupGridState() {
        GUILayout.Space(20);
        GUILayout.Label("Setup Grid Size", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Width (X):", GUILayout.Width(70));
        _gridSize.x = EditorGUILayout.IntField(_gridSize.x, GUILayout.Width(50));
        if (GUILayout.Button("-", GUILayout.Width(30))) _gridSize.x--;
        if (GUILayout.Button("+", GUILayout.Width(30))) _gridSize.x++;
        _gridSize.x = Mathf.Max(1, _gridSize.x); 
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Height (Y):", GUILayout.Width(70));
        _gridSize.y = EditorGUILayout.IntField(_gridSize.y, GUILayout.Width(50));
        if (GUILayout.Button("-", GUILayout.Width(30))) _gridSize.y--;
        if (GUILayout.Button("+", GUILayout.Width(30))) _gridSize.y++;
        _gridSize.y = Mathf.Max(1, _gridSize.y); 
        GUILayout.EndHorizontal();

        GUILayout.Space(20);
        if (GUILayout.Button("Create Grid", GUILayout.Height(40))) {
            _currentState = EditorState.Editing;
        }

        if (GUILayout.Button("Back")) _currentState = EditorState.Start;
    }

    // ==========================================
    // СТЕЙТ 3: РЕДАКТИРОВАНИЕ (ГЛАВНЫЙ ЭКРАН)
    // ==========================================
    private void DrawEditingState() {
        GUIStyle bannerStyle = new GUIStyle(EditorStyles.helpBox) {
            fontSize = 22,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(1f, 0.8f, 0.2f) }
        };
        GUI.backgroundColor = _hasUnsavedChanges ? new Color(0.3f, 0.15f, 0.15f) : new Color(0.15f, 0.15f, 0.15f);
        
        string unsavedText = _hasUnsavedChanges ? " (UNSAVED)" : "";
        GUILayout.Box($"★ CURRENTLY EDITING LEVEL: {_levelIndex} ★{unsavedText}", bannerStyle, GUILayout.ExpandWidth(true), GUILayout.Height(45));
        
        GUI.backgroundColor = Color.white;
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();

        // КОЛОНКА 1: Сетка
        GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        DrawGrid();
        GUILayout.EndVertical();

        // --- ИНТЕРАКТИВНЫЙ РАЗДЕЛИТЕЛЬ (SPLITTER) ---
        Rect splitterRect = GUILayoutUtility.GetRect(4f, 4f, GUILayout.ExpandHeight(true));
        EditorGUI.DrawRect(splitterRect, new Color(0.2f, 0.2f, 0.2f)); 
        EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeHorizontal);
        
        if (Event.current != null) {
            if (Event.current.type == EventType.MouseDown && splitterRect.Contains(Event.current.mousePosition)) {
                _isResizingSettings = true;
                Event.current.Use();
            }
            else if (Event.current.type == EventType.MouseDrag && _isResizingSettings) {
                _settingsColumnWidth -= Event.current.delta.x; 
                _settingsColumnWidth = Mathf.Clamp(_settingsColumnWidth, 380f, 800f);
                Repaint();
                Event.current.Use();
            }
            else if (Event.current.type == EventType.MouseUp && _isResizingSettings) {
                _isResizingSettings = false;
                Event.current.Use();
            }
        }

        // КОЛОНКА 2: Настройки уровня с ИЗМЕНЯЕМОЙ ШИРИНОЙ
        GUILayout.BeginVertical(GUILayout.Width(_settingsColumnWidth)); 
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.ExpandHeight(true));
        
        GUILayout.BeginVertical("box", GUILayout.Width(_settingsColumnWidth - 20f)); 
        
        DrawLevelInfoAndVisualSettings();
        GUILayout.Space(10);
        DrawSaveSection();
        GUILayout.Space(10);
        DrawPatternSection();
        GUILayout.Space(10);
        DrawGeneratorSection();
        GUILayout.Space(10);
        DrawBatchGeneratorSection();
        GUILayout.Space(10);
        DrawSnakeControlButtons();
        GUILayout.Space(10);
        DrawSnakesList();
        
        GUILayout.EndVertical(); 
        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        // КОЛОНКА 3: База данных
        GUILayout.BeginVertical("box", GUILayout.Width(260));
        DrawDatabaseSection();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        if (GUI.changed) Repaint();
    }

    private void DrawLevelInfoAndVisualSettings() {
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("Level Info", EditorStyles.boldLabel);
        
        // --- ДИНАМИЧЕСКОЕ ИЗМЕНЕНИЕ РАЗМЕРА СЕТКИ В РЕДАКТОРЕ ---
        GUILayout.BeginHorizontal();
        GUILayout.Label("Width (X):", GUILayout.Width(65));
        EditorGUI.BeginChangeCheck();
        int newX = EditorGUILayout.IntField(_gridSize.x, GUILayout.Width(35));
        if (GUILayout.Button("-", EditorStyles.miniButtonLeft, GUILayout.Width(22))) newX--;
        if (GUILayout.Button("+", EditorStyles.miniButtonRight, GUILayout.Width(22))) newX++;
        
        GUILayout.Space(10);
        
        GUILayout.Label("Height (Y):", GUILayout.Width(65));
        int newY = EditorGUILayout.IntField(_gridSize.y, GUILayout.Width(35));
        if (GUILayout.Button("-", EditorStyles.miniButtonLeft, GUILayout.Width(22))) newY--;
        if (GUILayout.Button("+", EditorStyles.miniButtonRight, GUILayout.Width(22))) newY++;
        GUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck()) {
            _gridSize.x = Mathf.Max(1, newX);
            _gridSize.y = Mathf.Max(1, newY);
            MarkAsDirty();
        }

        GUILayout.Space(5);
        GUILayout.Label($"Total Snakes: {_snakes.Count}");
        
        GUILayout.Space(5);
        
        GUI.backgroundColor = new Color(1f, 0.8f, 0.5f);
        if (GUILayout.Button("Trim Empty Space", GUILayout.Height(25))) {
            if (TrimGridInternal()) {
                MarkAsDirty();
                GUI.FocusControl(null);
                Repaint();
            }
        }
        GUI.backgroundColor = Color.white;
        
        GUILayout.Space(10);
        
        GUILayout.Label("Editor Visuals", EditorStyles.boldLabel);
        _editorLineWidth = EditorGUILayout.Slider("Line Width", _editorLineWidth, 0.1f, 1f);
        _editorDotSize = EditorGUILayout.Slider("Dot Size", _editorDotSize, 0.1f, 1f);
        _solidLines = EditorGUILayout.Toggle("Solid Lines (No AA)", _solidLines); 
        _gridBgColor = EditorGUILayout.ColorField("Grid Background", _gridBgColor); 
        
        GUILayout.EndVertical();
    }

    private void DrawDatabaseSection() {
        GUILayout.Label("Levels Database", EditorStyles.boldLabel);
        
        GUI.backgroundColor = new Color(0.6f, 0.9f, 0.6f);
        if (GUILayout.Button("Update LevelsDatabase", GUILayout.Height(35))) {
            UpdateLevelsDatabaseAsset();
        }
        GUI.backgroundColor = Color.white;
        
        GUILayout.Space(5);
        if (GUILayout.Button("Refresh Folder", EditorStyles.miniButton)) {
            RefreshLevelsList();
        }
        
        GUILayout.Space(10);
        
        _levelsScrollPos = GUILayout.BeginScrollView(_levelsScrollPos);
        
        for (int i = 0; i < _allLevelsCache.Count; i++) {
            var level = _allLevelsCache[i];
            
            if (_loadedLevelData == level) {
                GUI.backgroundColor = new Color(0.3f, 0.7f, 1f); 
            } else {
                GUI.backgroundColor = Color.white;
            }
            
            if (GUILayout.Button($"{i}: {level.name}", EditorStyles.miniButtonLeft, GUILayout.Height(22))) {
                EditorApplication.delayCall += () => AttemptLoadLevel(level);
            }
        }
        
        GUI.backgroundColor = Color.white;
        GUILayout.EndScrollView();
    }

    private void RefreshLevelsList() {
        _allLevelsCache.Clear();
        string path = "Assets/Scriptable/_LevelsDatabase/_Levels";
        
        if (Directory.Exists(path)) {
            string[] guids = AssetDatabase.FindAssets("t:LevelData", new[] { path });
            foreach (string guid in guids) {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                LevelData ld = AssetDatabase.LoadAssetAtPath<LevelData>(assetPath);
                if (ld != null) _allLevelsCache.Add(ld);
            }

            _allLevelsCache = _allLevelsCache
                .OrderBy(l => l.LevelIndex)
                .ThenBy(l => l.name)
                .ToList();
        }
    }

    private void UpdateLevelsDatabaseAsset() {
        RefreshLevelsList(); 

        string dbPath = "Assets/Scriptable/_LevelsDatabase/LevelsDatabase.asset";
        LevelsDatabase db = AssetDatabase.LoadAssetAtPath<LevelsDatabase>(dbPath);
        
        if (db == null) {
            EnsureFolderExists("Assets/Scriptable/_LevelsDatabase");
            db = CreateInstance<LevelsDatabase>();
            AssetDatabase.CreateAsset(db, dbPath);
        }

        db.levelDatas = new List<LevelData>(_allLevelsCache);

        for (int i = 0; i < db.levelDatas.Count; i++) {
            var level = db.levelDatas[i];
            if (level.LevelIndex != i) {
                level.LevelIndex = i;
                EditorUtility.SetDirty(level);
            }
        }

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"<color=green>LevelsDatabase updated with {db.levelDatas.Count} levels!</color>");
        ShowNotification(new GUIContent("Database Updated!"));
    }

    private bool TrimGridInternal() {
        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;

        bool hasContent = false;

        foreach (var snake in _snakes) {
            foreach (var pos in snake.BodyPositions) {
                if (pos.x < minX) minX = pos.x;
                if (pos.x > maxX) maxX = pos.x;
                if (pos.y < minY) minY = pos.y;
                if (pos.y > maxY) maxY = pos.y;
                hasContent = true;
            }
        }

        foreach (var pos in _patternCells) {
            if (pos.x < minX) minX = pos.x;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.y < minY) minY = pos.y;
            if (pos.y > maxY) maxY = pos.y;
            hasContent = true;
        }

        if (!hasContent) return false;
        if (minX == 0 && maxX == _gridSize.x - 1 && minY == 0 && maxY == _gridSize.y - 1) return false;

        int newWidth = maxX - minX + 1;
        int newHeight = maxY - minY + 1;

        foreach (var snake in _snakes) {
            for (int i = 0; i < snake.BodyPositions.Count; i++) {
                Vector2Int oldPos = snake.BodyPositions[i];
                snake.BodyPositions[i] = new Vector2Int(oldPos.x - minX, oldPos.y - minY);
            }
        }

        HashSet<Vector2Int> newPatternCells = new HashSet<Vector2Int>();
        foreach (var pos in _patternCells) {
            newPatternCells.Add(new Vector2Int(pos.x - minX, pos.y - minY));
        }
        _patternCells = newPatternCells;

        _gridSize = new Vector2Int(newWidth, newHeight);

        return true;
    }

    private void DrawPatternSection() {
        GUI.backgroundColor = new Color(0.9f, 0.9f, 1f);
        GUILayout.BeginVertical("box");
        GUI.backgroundColor = Color.white;

        _showPatternTool = EditorGUILayout.Foldout(_showPatternTool, " ★ Pattern Guide (ASCII Art)", true, EditorStyles.boldLabel);

        if (_showPatternTool) {
            GUILayout.Space(5);
            GUILayout.Label("Paste text matrix (use '.' for empty, 'X' for highlighted):");
            
            _patternText = EditorGUILayout.TextArea(_patternText, GUILayout.Height(80));

            _invertPattern = EditorGUILayout.Toggle("Invert Pattern", _invertPattern);

            GUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(0.5f, 0.7f, 1f);
            if (GUILayout.Button("Apply Pattern & Resize Grid", GUILayout.Height(30))) {
                ApplyPatternText(_patternText, _invertPattern);
                MarkAsDirty();
                GUI.FocusControl(null);
                Repaint();
            }
            GUI.backgroundColor = Color.white;
            
            if (_patternCells.Count > 0) {
                if (GUILayout.Button("Clear", GUILayout.Width(60), GUILayout.Height(30))) {
                    _patternCells.Clear();
                    _patternText = "";
                    Repaint();
                }
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
    }

    private void ApplyPatternText(string text, bool invert) {
        if (string.IsNullOrWhiteSpace(text)) return;

        string[] lines = text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0) return;

        int newHeight = lines.Length;
        int newWidth = lines.Max(l => l.Length);

        _gridSize = new Vector2Int(newWidth, newHeight);
        _patternCells.Clear();

        for (int row = 0; row < newHeight; row++) {
            string line = lines[row];
            int y = newHeight - 1 - row; 
            
            for (int x = 0; x < newWidth; x++) {
                char c = x < line.Length ? line[x] : ' ';
                
                bool isEmptyChar = (c == '.' || c == ' ');
                bool isHighlighted = invert ? isEmptyChar : !isEmptyChar;

                if (isHighlighted) {
                    _patternCells.Add(new Vector2Int(x, y));
                }
            }
        }
    }

    // ==========================================
    // ОДИНОЧНЫЙ ГЕНЕРАТОР
    // ==========================================
    private void DrawGeneratorSection() {
        GUI.backgroundColor = new Color(0.8f, 1f, 0.8f);
        GUILayout.BeginVertical("box");
        GUI.backgroundColor = Color.white;

        _showGenerator = EditorGUILayout.Foldout(_showGenerator, " ★ Auto-Generate Level (Single)", true, EditorStyles.boldLabel);

        if (_showGenerator) {
            GUILayout.Space(5);

            _minSnakeLength = EditorGUILayout.IntSlider("Min Length", _minSnakeLength, 3, 10);
            _maxSnakeLength = EditorGUILayout.IntSlider("Max Length", _maxSnakeLength, _minSnakeLength, 50);

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Length Ease", "Выгнута вверх = больше длинных. Выгнута вниз = больше коротких."), GUILayout.Width(145));
            _lengthDistribution = EditorGUILayout.CurveField(_lengthDistribution);
            GUILayout.EndHorizontal();

            _twistFactor = EditorGUILayout.Slider("Twist / Entanglement", _twistFactor * 100f, 0f, 100f) / 100f;
            _targetFillPercentage = EditorGUILayout.Slider("Fill Density (%)", _targetFillPercentage * 100f, 30f, 100f) / 100f;

            GUILayout.Space(5);
            GUI.backgroundColor = new Color(0.4f, 1f, 0.4f);
            
            string btnText = _patternCells.Count > 0 ? "GENERATE INSIDE PATTERN" : "CREATE SINGLE LEVEL";
            
            if (GUILayout.Button(btnText, GUILayout.Height(35))) {
                GenerateLevelInternal(_patternCells.Count > 0);
                MarkAsDirty();
                GUI.FocusControl(null);
                Repaint();
            }

            GUI.backgroundColor = Color.white;
        }

        GUILayout.EndVertical();
    }

    // ==========================================
    // БЛОК: BATCH GENERATOR (МАССОВОЕ СОЗДАНИЕ)
    // ==========================================
    private void DrawBatchGeneratorSection() {
        GUI.backgroundColor = new Color(0.7f, 0.9f, 1f);
        GUILayout.BeginVertical("box");
        GUI.backgroundColor = Color.white;

        _showBatchGenerator = EditorGUILayout.Foldout(_showBatchGenerator, " ★ Batch Generate (Multiple Levels)", true, EditorStyles.boldLabel);

        if (_showBatchGenerator) {
            GUILayout.Space(5);

            _batchCount = EditorGUILayout.IntSlider("Number of Levels", _batchCount, 1, 150);

            GUILayout.Space(5);
            
            _batchUseTemplates = EditorGUILayout.Toggle("Use Templates from folder", _batchUseTemplates);
            
            if (_batchUseTemplates) {
                EditorGUILayout.HelpBox("Grid Size will be determined by the .txt templates in:\nAssets/Scriptable/_LevelsDatabase/_LevelTemplates", MessageType.Info);
                
                string templatesPath = "Assets/Scriptable/_LevelsDatabase/_LevelTemplates";
                int totalTpls = 0;
                if (Directory.Exists(templatesPath)) {
                    totalTpls = Directory.GetFiles(templatesPath, "*.txt").Length;
                }
                
                if (totalTpls > 0) {
                    int cycle = _globalTemplateCounter / totalTpls;
                    int nextIdx = _globalTemplateCounter % totalTpls;
                    bool inverted = (cycle % 2) != 0;
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"Counter: {_globalTemplateCounter}", GUILayout.Width(80));
                    GUILayout.Label($"(Next: Tpl_{nextIdx} | {(inverted ? "INVERTED" : "Normal")})", EditorStyles.boldLabel);
                    
                    if (GUILayout.Button("Reset", EditorStyles.miniButton, GUILayout.Width(50))) {
                        _globalTemplateCounter = 0;
                        EditorPrefs.SetInt("SnakeLevelEditor_TemplateCounter", 0);
                    }
                    GUILayout.EndHorizontal();
                }
            }

            GUI.enabled = !_batchUseTemplates;
            GUILayout.Space(5);
            GUILayout.Label("Grid Size Range (Random)", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            _batchGridWidthBounds.x = EditorGUILayout.IntField("Min Width", _batchGridWidthBounds.x);
            _batchGridWidthBounds.y = EditorGUILayout.IntField("Max Width", _batchGridWidthBounds.y);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            _batchGridHeightBounds.x = EditorGUILayout.IntField("Min Height", _batchGridHeightBounds.x);
            _batchGridHeightBounds.y = EditorGUILayout.IntField("Max Height", _batchGridHeightBounds.y);
            GUILayout.EndHorizontal();
            GUI.enabled = true;

            GUILayout.Space(5);
            GUILayout.Label("Snake Length Range", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            _batchMinLengthBounds.x = EditorGUILayout.IntField("Min Length", _batchMinLengthBounds.x);
            _batchMinLengthBounds.y = EditorGUILayout.IntField("Max Length", _batchMinLengthBounds.y);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            _batchMaxLengthBounds.x = EditorGUILayout.IntField("Min (MaxL)", _batchMaxLengthBounds.x);
            _batchMaxLengthBounds.y = EditorGUILayout.IntField("Max (MaxL)", _batchMaxLengthBounds.y);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUILayout.Label("Modifiers", EditorStyles.boldLabel);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Twist Factor", GUILayout.Width(145));
            _batchTwistMin = EditorGUILayout.FloatField((float)System.Math.Round(_batchTwistMin, 2), GUILayout.Width(40));
            EditorGUILayout.MinMaxSlider(ref _batchTwistMin, ref _batchTwistMax, 0f, 1f);
            _batchTwistMax = EditorGUILayout.FloatField((float)System.Math.Round(_batchTwistMax, 2), GUILayout.Width(40));
            GUILayout.EndHorizontal();
            _batchTwistMin = Mathf.Clamp(_batchTwistMin, 0f, _batchTwistMax);
            _batchTwistMax = Mathf.Clamp(_batchTwistMax, _batchTwistMin, 1f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Fill Density", GUILayout.Width(145));
            _batchFillMin = EditorGUILayout.FloatField((float)System.Math.Round(_batchFillMin, 2), GUILayout.Width(40));
            EditorGUILayout.MinMaxSlider(ref _batchFillMin, ref _batchFillMax, 0f, 1f);
            _batchFillMax = EditorGUILayout.FloatField((float)System.Math.Round(_batchFillMax, 2), GUILayout.Width(40));
            GUILayout.EndHorizontal();
            _batchFillMin = Mathf.Clamp(_batchFillMin, 0f, _batchFillMax);
            _batchFillMax = Mathf.Clamp(_batchFillMax, _batchFillMin, 1f);

            GUILayout.Space(5);
            GUI.backgroundColor = new Color(0.2f, 0.6f, 1f);
            if (GUILayout.Button($"GENERATE {_batchCount} LEVELS", GUILayout.Height(35))) {
                GenerateBatch();
            }
            GUI.backgroundColor = Color.white;
        }

        GUILayout.EndVertical();
    }

    private void GenerateBatch() {
        RefreshLevelsList();
        
        int startIndex = _allLevelsCache.Count > 0 ? _allLevelsCache.Max(l => l.LevelIndex) + 1 : 0;

        List<string> templates = new List<string>();
        if (_batchUseTemplates) {
            string templatesPath = "Assets/Scriptable/_LevelsDatabase/_LevelTemplates";
            EnsureFolderExists(templatesPath); 
            
            string[] files = Directory.GetFiles(templatesPath, "*.txt").OrderBy(f => f).ToArray();
            foreach (var f in files) {
                templates.Add(File.ReadAllText(f));
            }
            
            if (templates.Count == 0) {
                Debug.LogWarning($"[SnakeLevelEditor] No .txt templates found in {templatesPath}! Falling back to random grid sizes.");
            }
        }

        for (int i = 0; i < _batchCount; i++) {
            EditorUtility.DisplayProgressBar("Batch Generation", $"Generating level {i + 1} of {_batchCount}...", (float)i / _batchCount);

            if (_batchUseTemplates && templates.Count > 0) {
                int tplIndex = _globalTemplateCounter % templates.Count;
                int cycleIndex = _globalTemplateCounter / templates.Count;
                bool invertTemplate = (cycleIndex % 2) != 0; 

                string tpl = templates[tplIndex]; 
                ApplyPatternText(tpl, invertTemplate);
                
                _globalTemplateCounter++; 
            } else {
                _gridSize.x = Random.Range(_batchGridWidthBounds.x, _batchGridWidthBounds.y + 1);
                _gridSize.y = Random.Range(_batchGridHeightBounds.x, _batchGridHeightBounds.y + 1);
                _patternCells.Clear(); 
            }
            
            _minSnakeLength = Random.Range(_batchMinLengthBounds.x, _batchMinLengthBounds.y + 1);
            _maxSnakeLength = Random.Range(_batchMaxLengthBounds.x, _batchMaxLengthBounds.y + 1);
            if (_maxSnakeLength < _minSnakeLength) _maxSnakeLength = _minSnakeLength;

            _twistFactor = Random.Range(_batchTwistMin, _batchTwistMax);
            _targetFillPercentage = Random.Range(_batchFillMin, _batchFillMax);

            GenerateLevelInternal(_patternCells.Count > 0); 
            
            TrimGridInternal();
            
            int targetIndex = startIndex + i;
            SaveLevelHeadless(targetIndex);
        }

        EditorPrefs.SetInt("SnakeLevelEditor_TemplateCounter", _globalTemplateCounter);

        EditorUtility.ClearProgressBar();
        UpdateLevelsDatabaseAsset(); 
        
        if (_allLevelsCache.Count > 0) {
            LoadLevel(_allLevelsCache.Last());
            _currentState = EditorState.Editing; 
        }

        ShowNotification(new GUIContent($"Successfully generated {_batchCount} levels!"));
    }

    private void SaveLevelHeadless(int targetIndex) {
        if (_snakes.Count == 0) return;

        string dbFolderPath = "Assets/Scriptable/_LevelsDatabase";
        string levelsFolderPath = $"{dbFolderPath}/_Levels";
        EnsureFolderExists(dbFolderPath);
        EnsureFolderExists(levelsFolderPath);

        string fileName = $"{targetIndex}_Lvl_{_gridSize.x}x{_gridSize.y}_{_snakes.Count}";
        string newAssetPath = $"{levelsFolderPath}/{fileName}.asset";

        LevelData dataToSave = CreateInstance<LevelData>();
        dataToSave.LevelIndex = targetIndex;
        dataToSave.GridSize = _gridSize;

        dataToSave.Snakes = new List<SnakeConfig>();
        foreach (var s in _snakes) {
            dataToSave.Snakes.Add(new SnakeConfig {
                Color = s.Color,
                BodyPositions = new List<Vector2Int>(s.BodyPositions)
            });
        }

        AssetDatabase.CreateAsset(dataToSave, newAssetPath);
    }

    // ==========================================
    // ЯДРО АЛГОРИТМА ГЕНЕРАЦИИ
    // ==========================================
    private void GenerateLevelInternal(bool usePattern) {
        _snakes.Clear();
        _activeSnakeIndex = -1;

        HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();
        
        int totalCells = usePattern ? _patternCells.Count : (_gridSize.x * _gridSize.y);
        int targetFilledCells = Mathf.RoundToInt(totalCells * _targetFillPercentage);

        Vector2Int[] allDirections = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        bool placedInLastPass = true;
        int maxEmptyPasses = 5; 
        int emptyPasses = 0;

        Vector2 center = new Vector2(_gridSize.x / 2f, _gridSize.y / 2f);
        if (usePattern && _patternCells.Count > 0) {
            float avgX = (float)_patternCells.Average(p => p.x);
            float avgY = (float)_patternCells.Average(p => p.y);
            center = new Vector2(avgX, avgY);
        }

        while (occupiedCells.Count < targetFilledCells && emptyPasses < maxEmptyPasses) {
            placedInLastPass = false;

            List<Vector2Int> availableCells = new List<Vector2Int>();
            if (usePattern) {
                availableCells = _patternCells.Except(occupiedCells).ToList();
            } else {
                for (int x = 0; x < _gridSize.x; x++) {
                    for (int y = 0; y < _gridSize.y; y++) {
                        var p = new Vector2Int(x, y);
                        if (!occupiedCells.Contains(p)) availableCells.Add(p);
                    }
                }
            }

            if (availableCells.Count == 0) break;

            availableCells = availableCells.OrderBy(p => Vector2.Distance(p, center) + Random.Range(-1.5f, 1.5f)).ToList();

            foreach (var headPos in availableCells) {
                Vector2Int[] shuffledDirs = allDirections.OrderBy(x => Random.value).ToArray();
                bool placedForThisHead = false;

                foreach (var exitDir in shuffledDirs) {
                    bool exitClear = true;
                    HashSet<Vector2Int> forbiddenExitRay = new HashSet<Vector2Int>(); 

                    Vector2Int rayPos = headPos + exitDir;
                    while (rayPos.x >= 0 && rayPos.x < _gridSize.x && rayPos.y >= 0 && rayPos.y < _gridSize.y) {
                        forbiddenExitRay.Add(rayPos);
                        if (occupiedCells.Contains(rayPos)) {
                            exitClear = false;
                            break;
                        }
                        rayPos += exitDir;
                    }

                    if (!exitClear) continue; 

                    Vector2Int neckPos = headPos - exitDir;
                    
                    if (neckPos.x < 0 || neckPos.x >= _gridSize.x || neckPos.y < 0 || neckPos.y >= _gridSize.y || occupiedCells.Contains(neckPos)) {
                        continue; 
                    }

                    if (usePattern && !_patternCells.Contains(neckPos)) {
                        continue;
                    }

                    float randomT = Random.value;
                    float curveValue = Mathf.Clamp01(_lengthDistribution.Evaluate(randomT));
                    int targetLength = Mathf.RoundToInt(Mathf.Lerp(_minSnakeLength, _maxSnakeLength, curveValue));

                    List<Vector2Int> newSnakePath = new List<Vector2Int> { headPos, neckPos };
                    HashSet<Vector2Int> currentSnakeCells = new HashSet<Vector2Int> { headPos, neckPos };

                    Vector2Int currentCell = neckPos;
                    bool forceStop = false;

                    for (int step = 2; step < _maxSnakeLength * 1.5f; step++) {
                        List<Vector2Int> freeNeighbors = GetFreeNeighbors(currentCell, occupiedCells, currentSnakeCells, forbiddenExitRay, usePattern);

                        if (freeNeighbors.Count == 0) break; 

                        if (step >= targetLength) {
                            var deadEnds = freeNeighbors.Where(n => GetFreeNeighbors(n, occupiedCells, currentSnakeCells, forbiddenExitRay, usePattern).Count <= 1).ToList();
                            
                            if (deadEnds.Count > 0) {
                                freeNeighbors = deadEnds;
                            } else {
                                if (occupiedCells.Count + currentSnakeCells.Count >= targetFilledCells) {
                                    forceStop = true;
                                } else {
                                    if (Random.value > 0.7f) forceStop = true;
                                }
                            }
                        }
                        
                        if (forceStop) break;

                        float bestScore = float.MaxValue;
                        List<Vector2Int> bestNeighbors = new List<Vector2Int>();

                        Vector2Int prevDir = Vector2Int.zero;
                        if (newSnakePath.Count >= 2) {
                            prevDir = currentCell - newSnakePath[newSnakePath.Count - 2];
                        }

                        foreach (var n in freeNeighbors) {
                            int freeCount = GetFreeNeighbors(n, occupiedCells, currentSnakeCells, forbiddenExitRay, usePattern).Count;
                            float score = freeCount * 2f; 
                            
                            Vector2Int newDir = n - currentCell;
                            
                            if (newSnakePath.Count >= 2) {
                                if (newDir == prevDir) {
                                    score += Mathf.Lerp(-3f, 5f, _twistFactor); 
                                } else {
                                    score += Mathf.Lerp(3f, -2f, _twistFactor); 
                                }
                            }

                            score += Random.Range(0f, 2.5f); 

                            if (score < bestScore) {
                                bestScore = score;
                                bestNeighbors.Clear();
                                bestNeighbors.Add(n);
                            } else if (Mathf.Abs(score - bestScore) < 0.1f) {
                                bestNeighbors.Add(n);
                            }
                        }

                        Vector2Int nextCell = bestNeighbors[Random.Range(0, bestNeighbors.Count)];
                        newSnakePath.Add(nextCell);
                        currentSnakeCells.Add(nextCell);
                        currentCell = nextCell;
                    }

                    if (newSnakePath.Count >= _minSnakeLength) {
                        _snakes.Add(new SnakeConfig {
                            BodyPositions = newSnakePath,
                            Color = Random.ColorHSV(0f, 1f, 0.8f, 1f, 0.8f, 1f)
                        });

                        foreach (var cell in newSnakePath) {
                            occupiedCells.Add(cell);
                        }

                        placedInLastPass = true;
                        placedForThisHead = true;
                        break; 
                    }
                }

                if (placedForThisHead) {
                    break; 
                }
            }

            if (!placedInLastPass) {
                emptyPasses++;
            } else {
                emptyPasses = 0; 
            }
        }

        // --- УЛУЧШЕННЫЙ АЛГОРИТМ ЗАПОЛНЕНИЯ ПУСТОТ (POST-PROCESSING FILL) ---
        bool addedNewCells = true;
        bool layoutShifted = true;
        int safetyNet = 10000;
        int consecutiveShifts = 0;

        while ((addedNewCells || layoutShifted) && safetyNet-- > 0 && occupiedCells.Count < targetFilledCells) {
            addedNewCells = false;
            layoutShifted = false;
            
            var shuffledSnakes = _snakes.OrderBy(s => Random.value).ToList();

            foreach (var snake in shuffledSnakes) {
                if (occupiedCells.Count >= targetFilledCells) break;

                // 1. ПОПЫТКА РАСШИРЕНИЯ ТЕЛА (U-Bend) - впитываем 2x1 пустые пространства
                if (TryExpandBody(snake, occupiedCells, usePattern)) {
                    addedNewCells = true;
                    break; 
                }

                // 2. ПОПЫТКА ВЫТЯНУТЬ ХВОСТ (с эвристикой)
                if (TryExtendTail(snake, occupiedCells, usePattern)) {
                    addedNewCells = true;
                    break;
                }

                // 3. ПОПЫТКА ВЫТЯНУТЬ ГОЛОВУ (с эвристикой)
                if (TryExtendHead(snake, occupiedCells, usePattern)) {
                    addedNewCells = true;
                    break;
                }
            }

            // 4. СДВИГ УГЛОВ (Corner Flip)
            if (!addedNewCells && occupiedCells.Count < targetFilledCells) {
                if (consecutiveShifts < 20) { 
                    if (TryFlipCorners(occupiedCells, usePattern)) {
                        layoutShifted = true;
                        consecutiveShifts++;
                    }
                }
            } else {
                consecutiveShifts = 0; 
            }
        }

        _snakes.Reverse();
        if (_snakes.Count > 0) _activeSnakeIndex = 0;
    }

    // Симулятор прохождения уровня. Позволяет змейкам блокировать друг друга,
    // главное, чтобы весь "затор" можно было разобрать по цепочке!
    private bool IsLevelSolvable(List<SnakeConfig> snakes, HashSet<Vector2Int> occupied) {
        HashSet<Vector2Int> currentOccupied = new HashSet<Vector2Int>(occupied);
        bool[] exited = new bool[snakes.Count];
        int remainingCount = snakes.Count;

        bool progressMade = true;
        while (remainingCount > 0 && progressMade) {
            progressMade = false;
            for (int i = 0; i < snakes.Count; i++) {
                if (exited[i]) continue;

                if (CanSnakeExitEditor(snakes[i], currentOccupied)) {
                    // Змейка смогла вылететь! Убираем её тело, чтобы она не мешала остальным
                    foreach (var pos in snakes[i].BodyPositions) {
                        currentOccupied.Remove(pos);
                    }
                    exited[i] = true;
                    remainingCount--;
                    progressMade = true;
                }
            }
        }
        
        // Если все змейки вылетели - уровень собирается в правильный пазл
        return remainingCount == 0;
    }

    // Проверяет, свободен ли прямой луч от головы до края
    private bool CanSnakeExitEditor(SnakeConfig snake, HashSet<Vector2Int> occupied) {
        if (snake.BodyPositions.Count < 2) return true;
        
        Vector2Int dir = snake.BodyPositions[0] - snake.BodyPositions[1];
        Vector2Int currentCheckPos = snake.BodyPositions[0];

        int safetyLimit = 100;
        while (safetyLimit-- > 0) {
            currentCheckPos += dir;

            if (currentCheckPos.x < 0 || currentCheckPos.x >= _gridSize.x ||
                currentCheckPos.y < 0 || currentCheckPos.y >= _gridSize.y) {
                return true;
            }

            if (occupied.Contains(currentCheckPos) && !snake.BodyPositions.Contains(currentCheckPos)) {
                return false;
            }
        }
        return false;
    }

    // Упрощенный поиск свободных соседей для финального прохода (Post-Processing)
    private List<Vector2Int> GetSimpleFreeNeighbors(Vector2Int cell, HashSet<Vector2Int> occupied, bool usePattern) {
        List<Vector2Int> freeCells = new List<Vector2Int>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var dir in directions) {
            Vector2Int neighbor = cell + dir;
            if (neighbor.x >= 0 && neighbor.x < _gridSize.x &&
                neighbor.y >= 0 && neighbor.y < _gridSize.y &&
                !occupied.Contains(neighbor)) {
                
                if (usePattern && !_patternCells.Contains(neighbor)) {
                    continue;
                }

                freeCells.Add(neighbor);
            }
        }

        return freeCells;
    }

    // Оригинальный поиск соседей для первичного создания змеек
    private List<Vector2Int> GetFreeNeighbors(Vector2Int cell, HashSet<Vector2Int> globallyOccupied, HashSet<Vector2Int> locallyOccupied, HashSet<Vector2Int> forbiddenExitRay, bool usePattern) {
        List<Vector2Int> freeCells = new List<Vector2Int>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var dir in directions) {
            Vector2Int neighbor = cell + dir;
            if (neighbor.x >= 0 && neighbor.x < _gridSize.x &&
                neighbor.y >= 0 && neighbor.y < _gridSize.y &&
                !globallyOccupied.Contains(neighbor) &&
                !locallyOccupied.Contains(neighbor) &&
                !forbiddenExitRay.Contains(neighbor)) {
                
                if (usePattern && !_patternCells.Contains(neighbor)) {
                    continue;
                }

                freeCells.Add(neighbor);
            }
        }

        return freeCells;
    }

    // ==========================================
    // ОТРИСОВКА СЕТКИ И ВЗАИМОДЕЙСТВИЕ
    // ==========================================
    private void DrawGrid() {
        GUILayout.Label("LMB: Draw Path | RMB: Swap Head/Tail", EditorStyles.boldLabel);

        // Получаем ВСЕ оставшееся свободное место в текущей колонке
        Rect availableRect = GUILayoutUtility.GetRect(10f, 10000f, 10f, 10000f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        
        float cellSize = Mathf.Min(availableRect.width / _gridSize.x, availableRect.height / _gridSize.y);
        if (cellSize > 120f) cellSize = 120f; 

        float gridWidth = _gridSize.x * cellSize;
        float gridHeight = _gridSize.y * cellSize;

        // Идеально центрируем доску внутри доступной области
        Rect gridRect = new Rect(
            availableRect.x + (availableRect.width - gridWidth) / 2f,
            availableRect.y + (availableRect.height - gridHeight) / 2f,
            gridWidth,
            gridHeight
        );

        GUI.DrawTexture(gridRect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, false, 0, Color.black, 1, 0);

        Event e = Event.current;
        if (gridRect.Contains(e.mousePosition)) {
            float localX = e.mousePosition.x - gridRect.x;
            float localY = e.mousePosition.y - gridRect.y;
            int x = Mathf.FloorToInt(localX / cellSize);
            int y = _gridSize.y - 1 - Mathf.FloorToInt(localY / cellSize);
            Vector2Int hoveredCell = new Vector2Int(x, y);

            if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag) {
                if (e.button == 0) {
                    if (e.type == EventType.MouseDown) {
                        int clickedSnakeIndex = _snakes.FindIndex(s => s.BodyPositions.Contains(hoveredCell));
                        
                        if (clickedSnakeIndex != -1 && clickedSnakeIndex != _activeSnakeIndex) {
                            _activeSnakeIndex = clickedSnakeIndex;
                            GUI.FocusControl(null);
                            Repaint();
                            e.Use(); 
                        } 
                        else if (_activeSnakeIndex >= 0 && _activeSnakeIndex < _snakes.Count) {
                            HandleGridLeftClickDrag(hoveredCell, e.type);
                            e.Use();
                        } 
                    } 
                    else if (e.type == EventType.MouseDrag) {
                        if (_activeSnakeIndex >= 0 && _activeSnakeIndex < _snakes.Count) {
                            HandleGridLeftClickDrag(hoveredCell, e.type);
                            e.Use();
                        }
                    }
                } 
                else if (e.button == 1 && e.type == EventType.MouseDown) {
                    HandleGridRightClick(hoveredCell);
                    e.Use();
                }
            }
        }

        for (int x = 0; x < _gridSize.x; x++) {
            for (int y = 0; y < _gridSize.y; y++) {
                float drawY = (_gridSize.y - 1 - y) * cellSize;
                Rect cellRect = new Rect(gridRect.x + x * cellSize, gridRect.y + drawY, cellSize, cellSize);
                
                Vector2Int cellPos = new Vector2Int(x, y);
                Color cellColor = _gridBgColor; 

                if (_patternCells.Contains(cellPos)) {
                    cellColor = new Color(0.25f, 0.45f, 0.65f); 
                }

                EditorGUI.DrawRect(new Rect(cellRect.x + 1, cellRect.y + 1, cellRect.width - 2, cellRect.height - 2), cellColor);
            }
        }

        for (int sIndex = 0; sIndex < _snakes.Count; sIndex++) {
            var snake = _snakes[sIndex];
            if (snake.BodyPositions.Count == 0) continue;

            Vector3[] points = new Vector3[snake.BodyPositions.Count];
            for (int i = 0; i < snake.BodyPositions.Count; i++) {
                Vector2Int pos = snake.BodyPositions[i];
                float drawY = (_gridSize.y - 1 - pos.y) * cellSize;

                float cx = gridRect.x + pos.x * cellSize + cellSize / 2f;
                float cy = gridRect.y + drawY + cellSize / 2f;
                points[i] = new Vector3(cx, cy, 0);
            }

            Handles.color = snake.Color;
            
            float lineThickness = cellSize * _editorLineWidth;
            float dotRadius = cellSize * _editorDotSize / 2f;

            if (points.Length > 1) {
                if (_solidLines) {
                    for (int i = 0; i < points.Length - 1; i++) {
                        Vector3 p1 = points[i];
                        Vector3 p2 = points[i + 1];

                        float minX = Mathf.Min(p1.x, p2.x);
                        float maxX = Mathf.Max(p1.x, p2.x);
                        float minY = Mathf.Min(p1.y, p2.y);
                        float maxY = Mathf.Max(p1.y, p2.y);

                        Rect lineRect;
                        if (Mathf.Abs(p1.x - p2.x) > 0.1f) {
                            lineRect = new Rect(minX, p1.y - lineThickness / 2f, maxX - minX, lineThickness);
                        } else {
                            lineRect = new Rect(p1.x - lineThickness / 2f, minY, lineThickness, maxY - minY);
                        }
                        
                        EditorGUI.DrawRect(lineRect, snake.Color);
                    }
                } else {
                    Handles.DrawAAPolyLine(lineThickness, points);
                }
            }

            foreach (var pt in points) {
                Handles.DrawSolidDisc(pt, Vector3.forward, dotRadius);
            }

            Vector3 headPos = points[0];

            Handles.color = Color.white;
            Handles.DrawSolidDisc(headPos, Vector3.forward, dotRadius * 0.5f);

            GUIStyle headStyle = new GUIStyle(EditorStyles.boldLabel) {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = (snake.Color.grayscale > 0.5f) ? Color.black : Color.white }
            };
            Rect headRect = new Rect(headPos.x - cellSize / 2, headPos.y - cellSize / 2, cellSize, cellSize);
            GUI.Label(headRect, "H", headStyle);

            if (sIndex == _activeSnakeIndex) {
                Handles.color = Color.yellow;
                Handles.DrawWireDisc(headPos, Vector3.forward, dotRadius + 2f);
                Vector3 tailPos = points[points.Length - 1];
                Handles.DrawWireDisc(tailPos, Vector3.forward, dotRadius + 2f);
            }
        }
    }

    private void HandleGridLeftClickDrag(Vector2Int cell, EventType eventType) {
        var activeSnake = _snakes[_activeSnakeIndex];

        if (eventType == EventType.MouseDown && activeSnake.BodyPositions.Contains(cell)) {
            int index = activeSnake.BodyPositions.IndexOf(cell);
            if (index == activeSnake.BodyPositions.Count - 1) {
                activeSnake.BodyPositions.RemoveAt(index);
            } else {
                activeSnake.BodyPositions.RemoveRange(index + 1, activeSnake.BodyPositions.Count - (index + 1));
            }

            MarkAsDirty();
            return;
        }

        if (activeSnake.BodyPositions.Count == 0) {
            RemoveCellFromOtherSnakes(cell, activeSnake);
            activeSnake.BodyPositions.Add(cell);
            MarkAsDirty();
            return;
        }

        Vector2Int lastCell = activeSnake.BodyPositions.Last();
        if (cell == lastCell) return;

        if (activeSnake.BodyPositions.Count > 1 && cell == activeSnake.BodyPositions[activeSnake.BodyPositions.Count - 2]) {
            activeSnake.BodyPositions.RemoveAt(activeSnake.BodyPositions.Count - 1);
            MarkAsDirty();
            return;
        }

        if (IsAdjacent(cell, lastCell) && !activeSnake.BodyPositions.Contains(cell)) {
            RemoveCellFromOtherSnakes(cell, activeSnake);
            activeSnake.BodyPositions.Add(cell);
            MarkAsDirty();
        }
    }

    private void HandleGridRightClick(Vector2Int cell) {
        foreach (var snake in _snakes) {
            if (snake.BodyPositions.Contains(cell)) {
                snake.BodyPositions.Reverse();
                MarkAsDirty();
                break;
            }
        }
    }

    private void RemoveCellFromOtherSnakes(Vector2Int cell, SnakeConfig activeSnake) {
        foreach (var snake in _snakes) {
            if (snake != activeSnake && snake.BodyPositions.Contains(cell)) {
                snake.BodyPositions.Remove(cell);
            }
        }
    }

    private bool IsAdjacent(Vector2Int a, Vector2Int b) {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1;
    }

    private void DrawSnakeControlButtons() {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("+ Add Snake", GUILayout.Height(30))) {
            _snakes.Add(new SnakeConfig {
                BodyPositions = new List<Vector2Int>(),
                Color = Random.ColorHSV(0f, 1f, 0.8f, 1f, 0.8f, 1f)
            });
            _activeSnakeIndex = _snakes.Count - 1;
            MarkAsDirty();
        }

        GUI.enabled = _snakes.Count > 0;
        if (GUILayout.Button("- Remove Last", GUILayout.Height(30))) {
            _snakes.RemoveAt(_snakes.Count - 1);
            if (_activeSnakeIndex >= _snakes.Count) _activeSnakeIndex = _snakes.Count - 1;
            MarkAsDirty();
        }

        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
        if (GUILayout.Button("Clear All", GUILayout.Height(30))) {
            if (EditorUtility.DisplayDialog("Clear Snakes", "Are you sure you want to remove all snakes from the board?", "Yes", "Cancel")) {
                _snakes.Clear();
                _activeSnakeIndex = -1;
                MarkAsDirty();
                GUI.FocusControl(null);
            }
        }
        GUI.backgroundColor = Color.white;
        
        GUI.enabled = true;
        GUILayout.EndHorizontal();
    }

    private void DrawSnakesList() {
        if (_snakes.Count == 0) {
            GUILayout.Label("No snakes added yet.", EditorStyles.centeredGreyMiniLabel);
            return;
        }

        for (int i = _snakes.Count - 1; i >= 0; i--) {
            var snake = _snakes[i];

            GUI.backgroundColor = (_activeSnakeIndex == i) ? new Color(0.7f, 0.9f, 1f) : Color.white;
            GUILayout.BeginVertical("box");
            GUI.backgroundColor = Color.white;

            GUILayout.BeginHorizontal(GUILayout.Height(32));
            GUILayout.Space(5);

            Rect colorRect = GUILayoutUtility.GetRect(28, 28, GUILayout.Width(28), GUILayout.Height(28));
            colorRect.y += 2;
            
            EditorGUI.BeginChangeCheck();
            snake.Color = EditorGUI.ColorField(colorRect, GUIContent.none, snake.Color, false, false, false);
            if (EditorGUI.EndChangeCheck()) MarkAsDirty();

            GUIStyle indexStyle = new GUIStyle(EditorStyles.boldLabel) {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14
            };
            indexStyle.normal.textColor = (snake.Color.grayscale > 0.5f) ? Color.black : Color.white;
            GUI.Label(colorRect, $"{i}", indexStyle);

            GUILayout.Space(15);

            GUIStyle infoStyle = new GUIStyle(EditorStyles.label) {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 13
            };
            GUILayout.Label($"Length: {snake.BodyPositions.Count}", infoStyle, GUILayout.Height(32));

            GUIStyle xBtnStyle = new GUIStyle(GUI.skin.button);
            if (snake.BodyPositions.Count == 0) xBtnStyle.normal.textColor = Color.red;

            if (GUILayout.Button("X", xBtnStyle, GUILayout.Width(25), GUILayout.Height(25))) {
                if (snake.BodyPositions.Count > 0) {
                    snake.BodyPositions.Clear();
                    GUI.FocusControl(null);
                } else {
                    _snakes.RemoveAt(i);
                    if (_activeSnakeIndex == i) _activeSnakeIndex = -1;
                    else if (_activeSnakeIndex > i) _activeSnakeIndex--;
                    GUI.FocusControl(null);
                }
                MarkAsDirty();
                return;
            }

            GUILayout.EndHorizontal();

            Rect rowRect = GUILayoutUtility.GetLastRect();
            Event e = Event.current;

            if (e.type == EventType.MouseDown && rowRect.Contains(e.mousePosition)) {
                if (e.button == 0) {
                    _activeSnakeIndex = i;
                    GUI.FocusControl(null);
                    Repaint();
                } else if (e.button == 1) {
                    ShowContextMenu(i);
                    e.Use();
                }
            }

            GUILayout.EndVertical();
            GUILayout.Space(2);
        }
    }

    private void ShowContextMenu(int index) {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Edit"), false, () => {
            _activeSnakeIndex = index;
            Repaint();
        });
        menu.AddItem(new GUIContent("Clear Path"), false, () => {
            _snakes[index].BodyPositions.Clear();
            MarkAsDirty();
            Repaint();
        });
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Delete"), false, () => {
            _snakes.RemoveAt(index);
            if (_activeSnakeIndex == index) _activeSnakeIndex = -1;
            MarkAsDirty();
            Repaint();
        });
        menu.ShowAsContext();
    }

    // ==========================================
    // ЛОГИКА СОХРАНЕНИЯ И ПЕРЕИМЕНОВАНИЯ
    // ==========================================
    private void DrawSaveSection() {
        GUILayout.Label("Save & Load", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Quick Load:", GUILayout.Width(75));
        LevelData droppedLevel = (LevelData)EditorGUILayout.ObjectField(_loadedLevelData, typeof(LevelData), false);
        if (droppedLevel != _loadedLevelData && droppedLevel != null) {
            EditorApplication.delayCall += () => AttemptLoadLevel(droppedLevel);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        string currentAutoName = $"{_levelIndex}_Lvl_{_gridSize.x}x{_gridSize.y}_{_snakes.Count}";
        
        if (string.IsNullOrEmpty(_currentLevelName) || _currentLevelName == _lastAutoName) {
            _currentLevelName = currentAutoName;
        }
        _lastAutoName = currentAutoName;

        GUILayout.BeginHorizontal();
        _currentLevelName = EditorGUILayout.TextField("Level Name", _currentLevelName);
        
        if (_loadedLevelData != null) {
            if (GUILayout.Button("Apply", EditorStyles.miniButton, GUILayout.Width(60), GUILayout.Height(18))) {
                RenameLoadedLevel();
            }
        } else {
            if (GUILayout.Button("Auto", EditorStyles.miniButton, GUILayout.Width(60), GUILayout.Height(18))) {
                _currentLevelName = currentAutoName;
                GUI.FocusControl(null);
                EditorGUIUtility.editingTextField = false;
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        EditorGUI.BeginChangeCheck();
        _levelIndex = EditorGUILayout.IntField("Target Index", _levelIndex);
        if (EditorGUI.EndChangeCheck()) {
            MarkAsDirty();
        }

        string unsavedMark = _hasUnsavedChanges ? "*" : "";
        if (GUILayout.Button($"SAVE TO DATABASE {unsavedMark}", GUILayout.Height(40))) {
            SaveLevel();
        }

        if (GUILayout.Button("Reset & Back to Start")) {
            if (_hasUnsavedChanges) {
                if (EditorUtility.DisplayDialog("Unsaved Changes", "You have unsaved changes. Discard and go to Start?", "Discard", "Cancel")) {
                    _currentState = EditorState.Start;
                }
            } else {
                _currentState = EditorState.Start;
            }
        }
    }

    private void RenameLoadedLevel() {
        if (_loadedLevelData == null || string.IsNullOrWhiteSpace(_currentLevelName)) return;

        string assetPath = AssetDatabase.GetAssetPath(_loadedLevelData);
        if (string.IsNullOrEmpty(assetPath)) return;

        string newName = _currentLevelName.Trim();

        if (_loadedLevelData.name == newName) return; 

        string error = AssetDatabase.RenameAsset(assetPath, newName);
        if (!string.IsNullOrEmpty(error)) {
            Debug.LogWarning($"Could not rename asset to {newName}: {error}");
        } else {
            AssetDatabase.SaveAssets();
            RefreshLevelsList();
            ShowNotification(new GUIContent("Level Renamed!"));
            
            GUI.FocusControl(null);
            EditorGUIUtility.editingTextField = false;
        }
    }

    private void AttemptLoadLevel(LevelData newLevel) {
        if (newLevel == _loadedLevelData) return;

        if (_hasUnsavedChanges) {
            int option = EditorUtility.DisplayDialogComplex(
                "Unsaved Changes",
                "You have unsaved changes in the current level. Do you want to save them before loading the new one?",
                "Save",
                "Don't Save",
                "Cancel"
            );

            if (option == 0) {
                SaveLevel();
                LoadLevel(newLevel);
            } else if (option == 1) {
                LoadLevel(newLevel);
            }
        } else {
            LoadLevel(newLevel);
        }
    }

    private void SaveLevel() {
        if (_snakes.Count == 0) {
            EditorUtility.DisplayDialog("Error", "Level must have at least one snake!", "OK");
            return;
        }

        if (_snakes.Any(s => s.BodyPositions.Count < 3)) {
            EditorUtility.DisplayDialog(
                "Error",
                "All snakes must have a minimum length of 3 parts (Head, Neck, Tail)!\nCheck your list and fix or remove short snakes.",
                "OK"
            );
            return;
        }

        string dbFolderPath = "Assets/Scriptable/_LevelsDatabase";
        string levelsFolderPath = $"{dbFolderPath}/_Levels";
        EnsureFolderExists(dbFolderPath);
        EnsureFolderExists(levelsFolderPath);

        string dbPath = $"{dbFolderPath}/LevelsDatabase.asset";

        LevelsDatabase db = AssetDatabase.LoadAssetAtPath<LevelsDatabase>(dbPath);
        if (db == null) {
            db = CreateInstance<LevelsDatabase>();
            db.levelDatas = new List<LevelData>();
            AssetDatabase.CreateAsset(db, dbPath);
        }

        if (db.levelDatas == null) db.levelDatas = new List<LevelData>();

        _levelIndex = Mathf.Max(0, _levelIndex);
        if (_levelIndex >= db.levelDatas.Count) {
            _levelIndex = db.levelDatas.Count;
            db.levelDatas.Add(null);
        }

        string currentAutoName = $"{_levelIndex}_Lvl_{_gridSize.x}x{_gridSize.y}_{_snakes.Count}";
        string newFileName = string.IsNullOrWhiteSpace(_currentLevelName) ? currentAutoName : _currentLevelName.Trim();
        newFileName = string.Join("_", newFileName.Split(Path.GetInvalidFileNameChars()));

        string newAssetPath = $"{levelsFolderPath}/{newFileName}.asset";

        LevelData dataToSave = _loadedLevelData;
        bool isNew = (dataToSave == null);

        if (!isNew) {
            string oldAssetPath = AssetDatabase.GetAssetPath(dataToSave);
            if (!string.IsNullOrEmpty(oldAssetPath) && oldAssetPath != newAssetPath) {
                string error = AssetDatabase.RenameAsset(oldAssetPath, newFileName);
                if (!string.IsNullOrEmpty(error)) {
                    Debug.LogWarning($"Could not rename asset to {newFileName}: {error}");
                } else {
                    _currentLevelName = newFileName;
                }
            }
        }

        if (dataToSave == null) {
            dataToSave = CreateInstance<LevelData>();
            isNew = true;
        }

        dataToSave.LevelIndex = _levelIndex;
        dataToSave.GridSize = _gridSize;

        dataToSave.Snakes = new List<SnakeConfig>();
        foreach (var s in _snakes) {
            dataToSave.Snakes.Add(new SnakeConfig {
                Color = s.Color,
                BodyPositions = new List<Vector2Int>(s.BodyPositions)
            });
        }

        if (isNew) {
            AssetDatabase.CreateAsset(dataToSave, newAssetPath);
            _loadedLevelData = dataToSave; 
            _currentLevelName = newFileName;
        } else {
            EditorUtility.SetDirty(dataToSave);
        }

        db.levelDatas[_levelIndex] = dataToSave;
        EditorUtility.SetDirty(db);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        RefreshLevelsList();

        _hasUnsavedChanges = false; 

        Debug.Log($"<color=green>Level {_levelIndex} saved successfully and added to Database!</color>");
        ShowNotification(new GUIContent($"Level {_levelIndex} Saved!"));
    }

    private void LoadLevel(LevelData data) {
        _loadedLevelData = data;
        _levelIndex = data.LevelIndex;
        _gridSize = data.GridSize;
        
        _patternCells.Clear();
        _patternText = "";
        _currentLevelName = data.name; 

        _snakes.Clear();
        foreach (var s in data.Snakes) {
            _snakes.Add(new SnakeConfig {
                Color = s.Color,
                BodyPositions = new List<Vector2Int>(s.BodyPositions)
            });
        }

        _lastAutoName = $"{_levelIndex}_Lvl_{_gridSize.x}x{_gridSize.y}_{_snakes.Count}";

        _activeSnakeIndex = _snakes.Count > 0 ? 0 : -1;
        _hasUnsavedChanges = false; 
        
        GUI.FocusControl(null);
        EditorGUIUtility.editingTextField = false;
        
        Repaint();
    }

    private void EnsureFolderExists(string path) {
        if (!AssetDatabase.IsValidFolder(path)) {
            string[] parts = path.Split('/');
            string currentPath = parts[0];
            for (int i = 1; i < parts.Length; i++) {
                if (!AssetDatabase.IsValidFolder(currentPath + "/" + parts[i])) {
                    AssetDatabase.CreateFolder(currentPath, parts[i]);
                }
                currentPath += "/" + parts[i];
            }
        }
    }

    // ==========================================
    // МЕТОДЫ ПОСТОБРАБОТКИ (ADVANCED FILL)
    // ==========================================

    private bool TryExtendTail(SnakeConfig snake, HashSet<Vector2Int> occupied, bool usePattern) {
        if (snake.BodyPositions.Count == 0) return false;
        Vector2Int tail = snake.BodyPositions.Last();
        
        var neighbors = GetSimpleFreeNeighbors(tail, occupied, usePattern)
            .OrderBy(n => GetSimpleFreeNeighbors(n, occupied, usePattern).Count)
            .ThenBy(n => Random.value)
            .ToList();

        foreach (var n in neighbors) {
            snake.BodyPositions.Add(n);
            occupied.Add(n);

            if (IsLevelSolvable(_snakes, occupied)) {
                return true;
            } else {
                snake.BodyPositions.RemoveAt(snake.BodyPositions.Count - 1);
                occupied.Remove(n);
            }
        }
        return false;
    }

    private bool TryExtendHead(SnakeConfig snake, HashSet<Vector2Int> occupied, bool usePattern) {
        if (snake.BodyPositions.Count == 0) return false;
        Vector2Int head = snake.BodyPositions[0];
        
        var neighbors = GetSimpleFreeNeighbors(head, occupied, usePattern)
            .OrderBy(n => GetSimpleFreeNeighbors(n, occupied, usePattern).Count)
            .ThenBy(n => Random.value)
            .ToList();

        foreach (var n in neighbors) {
            snake.BodyPositions.Insert(0, n);
            occupied.Add(n);

            if (IsLevelSolvable(_snakes, occupied)) {
                return true;
            } else {
                snake.BodyPositions.RemoveAt(0);
                occupied.Remove(n);
            }
        }
        return false;
    }

    private bool TryExpandBody(SnakeConfig snake, HashSet<Vector2Int> occupied, bool usePattern) {
        if (snake.BodyPositions.Count < 2) return false;
        
        List<int> indices = Enumerable.Range(0, snake.BodyPositions.Count - 1).OrderBy(x => Random.value).ToList();

        foreach (int i in indices) {
            Vector2Int current = snake.BodyPositions[i];
            Vector2Int next = snake.BodyPositions[i + 1];
            
            Vector2Int dir = next - current;
            
            Vector2Int perp1 = new Vector2Int(-dir.y, dir.x);
            Vector2Int perp2 = new Vector2Int(dir.y, -dir.x);

            Vector2Int[] perps = new Vector2Int[] { perp1, perp2 }.OrderBy(x => Random.value).ToArray();

            foreach (var p in perps) {
                Vector2Int e1 = current + p;
                Vector2Int e2 = next + p;

                if (IsCellFreeAndValid(e1, occupied, usePattern) && IsCellFreeAndValid(e2, occupied, usePattern)) {
                    snake.BodyPositions.Insert(i + 1, e1);
                    snake.BodyPositions.Insert(i + 2, e2);
                    occupied.Add(e1);
                    occupied.Add(e2);

                    if (IsLevelSolvable(_snakes, occupied)) {
                        return true;
                    } else {
                        snake.BodyPositions.RemoveAt(i + 2);
                        snake.BodyPositions.RemoveAt(i + 1);
                        occupied.Remove(e2);
                        occupied.Remove(e1);
                    }
                }
            }
        }
        return false;
    }

    private bool TryFlipCorners(HashSet<Vector2Int> occupied, bool usePattern) {
        var shuffledSnakes = _snakes.OrderBy(s => Random.value).ToList();

        foreach (var snake in shuffledSnakes) {
            if (snake.BodyPositions.Count < 3) continue;
            List<int> indices = Enumerable.Range(0, snake.BodyPositions.Count - 2).OrderBy(x => Random.value).ToList();
            
            foreach (int i in indices) {
                Vector2Int p1 = snake.BodyPositions[i];
                Vector2Int p2 = snake.BodyPositions[i + 1];
                Vector2Int p3 = snake.BodyPositions[i + 2];

                if (p1.x != p3.x && p1.y != p3.y) {
                    Vector2Int p4 = new Vector2Int(p1.x == p2.x ? p3.x : p1.x, p1.y == p2.y ? p3.y : p1.y);

                    if (IsCellFreeAndValid(p4, occupied, usePattern)) {
                        snake.BodyPositions[i + 1] = p4;
                        occupied.Remove(p2);
                        occupied.Add(p4);

                        if (IsLevelSolvable(_snakes, occupied)) {
                            return true; 
                        } else {
                            snake.BodyPositions[i + 1] = p2;
                            occupied.Remove(p4);
                            occupied.Add(p2);
                        }
                    }
                }
            }
        }
        return false;
    }

    private bool IsCellFreeAndValid(Vector2Int cell, HashSet<Vector2Int> occupied, bool usePattern) {
        if (cell.x < 0 || cell.x >= _gridSize.x || cell.y < 0 || cell.y >= _gridSize.y) return false;
        if (occupied.Contains(cell)) return false;
        if (usePattern && !_patternCells.Contains(cell)) return false;
        return true;
    }
}