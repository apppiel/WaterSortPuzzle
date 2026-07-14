// Unity 에디터 전용 레벨 자동 생성 툴
// 상단 메뉴 Tools → Water Sort → 레벨 생성기 로 열 수 있다.
//
// 알고리즘: "랜덤 분포 + solver 검증"
//   1. 색 개수 × 튜브 용량 만큼의 세그먼트를 만들어 랜덤 튜브에 분포
//   2. WaterSortSolver로 실제 풀 수 있는지 검증
//   3. 풀 수 있으면 저장 + 풀이 시퀀스를 콘솔에 로그
//   4. 풀 수 없으면 새 시드로 재시도 (최대 MaxRetries번)
//
// 왜 reverse-shuffle이 아닌가:
//   완성 상태에서 시작하면 게임 룰상 forward 이동으로는 색이 절대 섞이지 않음.
//   (완성된 단색 튜브 → 빈 튜브 이동은 그저 위치 교환일 뿐)

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WaterSortPuzzle.Data;
using WaterSortPuzzle.EditorTools;

public class LevelGeneratorWindow : EditorWindow
{
    // 전역 팔레트 (LevelImporterWindow와 동일 순서/색)
    // 인덱스: 0=빨강 1=주황 2=노랑 3=초록 4=파랑 5=남색 6=보라 7=분홍 8=검정 9=회색 10=청록
    private static readonly Color[] GlobalPalette = new Color[]
    {
        new Color(0.937f, 0.267f, 0.267f, 1f), // 0  빨강
        new Color(0.976f, 0.451f, 0.086f, 1f), // 1  주황
        new Color(0.980f, 0.800f, 0.082f, 1f), // 2  노랑
        new Color(0.133f, 0.773f, 0.369f, 1f), // 3  초록
        new Color(0.231f, 0.510f, 0.965f, 1f), // 4  파랑
        new Color(0.388f, 0.400f, 0.945f, 1f), // 5  남색
        new Color(0.659f, 0.333f, 0.969f, 1f), // 6  보라
        new Color(0.925f, 0.286f, 0.600f, 1f), // 7  분홍
        new Color(0.094f, 0.094f, 0.106f, 1f), // 8  검정
        new Color(0.612f, 0.639f, 0.686f, 1f), // 9  회색
        new Color(0.078f, 0.722f, 0.651f, 1f), // 10 청록
    };

    private const int SolverMaxStates = 200000; // solver 상태 탐색 상한 (timeout 대신)

    // 풀이 텍스트가 저장되는 폴더 (프로젝트 루트 기준, .gitignore에 등록됨)
    private const string SolutionsFolder = "LevelSolutions";

    // ── 인스펙터 상태 ──────────────────────────────────
    private int    _startLevel      = 51;
    private int    _endLevel        = 60;
    private string _savePath        = "Assets/Levels";
    private bool   _manualConfig    = true;  // true면 아래 값 사용, false면 자동 progression
    private int    _colorCount      = 10;
    private int    _emptyTubeCount  = 2;
    private int    _tubeCapacity    = 4;
    private int    _minSolutionMoves = 0;   // 최소 풀이 수 필터 (0이면 필터 없음)
    private int    _maxRetries      = 30;   // 레벨 하나당 최대 재시도 횟수

    [MenuItem("Tools/Water Sort/레벨 생성기")]
    public static void ShowWindow()
    {
        GetWindow<LevelGeneratorWindow>("레벨 생성기");
    }

    private void OnGUI()
    {
        GUILayout.Label("레벨 생성 설정", EditorStyles.boldLabel);
        GUILayout.Space(8);

        _startLevel = EditorGUILayout.IntField("시작 레벨 (1부터)", _startLevel);
        _endLevel   = EditorGUILayout.IntField("끝 레벨 (포함)",   _endLevel);
        _savePath   = EditorGUILayout.TextField("저장 경로",       _savePath);

        GUILayout.Space(10);
        _manualConfig = EditorGUILayout.Toggle("수동 난이도 설정", _manualConfig);

        using (new EditorGUI.DisabledScope(!_manualConfig))
        {
            EditorGUI.indentLevel++;
            _colorCount     = EditorGUILayout.IntSlider("색 개수",      _colorCount,     3, GlobalPalette.Length);
            _emptyTubeCount = EditorGUILayout.IntSlider("빈 튜브 개수", _emptyTubeCount, 1, 3);
            _tubeCapacity   = EditorGUILayout.IntSlider("튜브 용량",   _tubeCapacity,   3, 6);
            EditorGUI.indentLevel--;
        }

        GUILayout.Space(6);
        GUILayout.Label("고급: 난이도 필터", EditorStyles.miniBoldLabel);
        _minSolutionMoves = EditorGUILayout.IntSlider("최소 풀이 수 (0=필터 없음)", _minSolutionMoves, 0, 100);
        _maxRetries       = EditorGUILayout.IntSlider("레벨당 최대 재시도",         _maxRetries,       10, 200);

        GUILayout.Space(12);

        if (GUILayout.Button("레벨 생성 (풀이 검증 포함)", GUILayout.Height(44)))
            GenerateLevels();

        GUILayout.Space(8);
        EditorGUILayout.HelpBox(
            "생성 흐름:\n" +
            "  1. 랜덤 분포 → Solver 검증 → 풀리면 저장, 안 풀리면 재시도\n" +
            "  2. 각 레벨은 Console에 최단 풀이 시퀀스가 로그됨\n" +
            "  3. LevelData_00, 01... 형태로 저장 (기존 파일 덮어씀)\n\n" +
            "수동 설정 OFF: 레벨 번호에 따라 자동으로 색/빈튜브 개수 결정\n" +
            "수동 설정 ON:  전 범위에 동일한 색/빈튜브 개수 적용",
            MessageType.Info);
    }

    // 지정한 범위의 레벨을 생성한다.
    // 진행률 바를 표시하고, 유저가 Cancel을 누르면 지금까지 생성된 것만 저장 후 중단.
    private void GenerateLevels()
    {
        if (!System.IO.Directory.Exists(_savePath))
            System.IO.Directory.CreateDirectory(_savePath);

        int generated = 0;
        int failed    = 0;
        int totalLevels = _endLevel - _startLevel + 1;
        bool cancelled = false;

        try
        {
            for (int levelNum = _startLevel; levelNum <= _endLevel; levelNum++)
            {
                int    idx    = levelNum - 1;
                var    config = _manualConfig ? ManualConfig() : AutoConfig(idx);
                int    levelPos = levelNum - _startLevel;

                var (data, solution, retries, wasCancelled) = GenerateSolvableLevelWithProgress(
                    config, idx, levelNum, levelPos, totalLevels, generated, failed);

                if (wasCancelled)
                {
                    cancelled = true;
                    break;
                }

                if (data == null)
                {
                    Debug.LogError($"Level {levelNum}: {_maxRetries}회 재시도 후에도 조건 통과 배치 못 찾음 (최소 풀이 수: {_minSolutionMoves}). 스킵.");
                    failed++;
                    continue;
                }

                string path = $"{_savePath}/LevelData_{idx:D2}.asset";
                if (AssetDatabase.LoadAssetAtPath<LevelData>(path) != null)
                    AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(data, path);
                generated++;

                // 풀이를 사람이 읽기 좋게 로그 + 텍스트 파일로 저장
                string moveStr = string.Join(", ", solution.ConvertAll(m => m.ToString()));
                Debug.Log($"✅ Level {levelNum}: {solution.Count}수 | 재시도 {retries}회 | {moveStr}");
                SaveSolutionToFile(levelNum, solution.Count, moveStr);
            }
        }
        finally
        {
            // 크래시/취소 상관없이 반드시 프로그레스 바 정리
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        if (cancelled)
            Debug.LogWarning($"── 취소됨: 성공 {generated} / 실패 {failed} (남은 레벨은 저장 안 됨) ──");
        else
            Debug.Log($"── 생성 완료: 성공 {generated} / 실패 {failed} ──");
    }

    // 수동 config
    private LevelConfig ManualConfig() => new LevelConfig
    {
        colorCount     = _colorCount,
        emptyTubeCount = _emptyTubeCount,
        tubeCapacity   = _tubeCapacity,
    };

    // 자동 progression: 레벨 번호에 따라 색/빈튜브 조정
    private LevelConfig AutoConfig(int levelIndex)
    {
        // 0~19: 쉬움 (3~4색), 20~59: 보통 (4~7색), 60~99: 어려움 (7~10색)
        if (levelIndex < 20)
            return new LevelConfig
            {
                colorCount     = Mathf.Clamp(3 + levelIndex / 7, 3, 4),
                emptyTubeCount = 1,
                tubeCapacity   = 4,
            };

        if (levelIndex < 60)
        {
            int t = levelIndex - 20;
            return new LevelConfig
            {
                colorCount     = Mathf.Clamp(4 + t / 8, 4, 7),
                emptyTubeCount = 1,
                tubeCapacity   = 4,
            };
        }

        int t2 = levelIndex - 60;
        return new LevelConfig
        {
            colorCount     = Mathf.Clamp(7 + t2 / 10, 7, 10),
            emptyTubeCount = t2 < 15 ? 2 : 1, // 60~74: 여유 있음, 이후 조임
            tubeCapacity   = 4,
        };
    }

    // 한 레벨을 solver 검증 + 최소 풀이 수 필터 통과할 때까지 생성 시도한다.
    // 매 재시도 사이에 프로그레스 바를 갱신하고, 유저가 Cancel 누르면 wasCancelled=true로 반환.
    private (LevelData data, List<WaterSortSolver.Move> solution, int retries, bool wasCancelled)
        GenerateSolvableLevelWithProgress(
            LevelConfig config, int levelIndex, int levelNum,
            int levelPos, int totalLevels, int successSoFar, int failsSoFar)
    {
        for (int retry = 0; retry < _maxRetries; retry++)
        {
            // 프로그레스 바 갱신 (매 재시도 시 — solver 자체는 blocking이라 solver 도중엔 못 함)
            float overallProgress = (levelPos + (float)retry / _maxRetries) / totalLevels;
            string info = $"Level {levelNum} 시도 {retry + 1}/{_maxRetries} (성공 {successSoFar}, 실패 {failsSoFar})";
            if (EditorUtility.DisplayCancelableProgressBar("레벨 생성 중", info, overallProgress))
                return (null, null, retry, true); // 유저가 Cancel

            // 매 시도마다 다른 시드로 (levelIndex + retry로 조합)
            var rng = new System.Random(levelIndex * 1000 + retry);
            int[][] tubes = RandomDistribute(config, rng);

            var solveResult = WaterSortSolver.Solve(config.tubeCapacity, tubes, SolverMaxStates);
            if (!solveResult.Solvable) continue;

            // 최소 풀이 수 필터: 너무 쉬운 배치는 재시도
            // (91~100 같은 보스 배치용 — solver의 min moves 기준으로 필터)
            if (solveResult.Solution.Count < _minSolutionMoves) continue;

            var data = BuildLevelData(config, tubes);
            return (data, solveResult.Solution, retry, false);
        }

        return (null, null, _maxRetries, false);
    }

    // 색 개수 × 튜브 용량 만큼의 세그먼트를 만들어 랜덤 셔플 후 채워진 튜브들에 분포
    // 마지막 emptyTubeCount 개는 빈 튜브로 둔다
    private int[][] RandomDistribute(LevelConfig config, System.Random rng)
    {
        int cap        = config.tubeCapacity;
        int filledCnt  = config.colorCount;
        int totalTubes = filledCnt + config.emptyTubeCount;

        // 전체 세그먼트 pool 생성 (색 c 를 cap개씩)
        var pool = new List<int>(filledCnt * cap);
        for (int c = 0; c < filledCnt; c++)
            for (int i = 0; i < cap; i++)
                pool.Add(c);

        // Fisher-Yates 셔플
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        // 튜브 채우기: 처음 filledCnt개는 pool에서 cap개씩 취함, 나머지는 빈 상태
        var tubes = new int[totalTubes][];
        int idx = 0;
        for (int t = 0; t < filledCnt; t++)
        {
            tubes[t] = new int[cap];
            for (int s = 0; s < cap; s++)
                tubes[t][s] = pool[idx++];
        }
        for (int t = filledCnt; t < totalTubes; t++)
            tubes[t] = new int[0];

        return tubes;
    }

    // int[][] 튜브 상태로 LevelData ScriptableObject 생성
    private LevelData BuildLevelData(LevelConfig config, int[][] tubes)
    {
        var data = ScriptableObject.CreateInstance<LevelData>();
        data.tubeCapacity = config.tubeCapacity;

        // 팔레트: 전역 팔레트에서 처음 colorCount개
        data.palette = new Color[config.colorCount];
        for (int i = 0; i < config.colorCount; i++)
            data.palette[i] = GlobalPalette[i];

        data.tubes = new TubeInitData[tubes.Length];
        for (int i = 0; i < tubes.Length; i++)
            data.tubes[i] = new TubeInitData { segments = tubes[i] };

        return data;
    }

    // 풀이 시퀀스를 프로젝트 루트/LevelSolutions/level_XX.txt 로 저장한다.
    // 폴더는 .gitignore 처리돼 있어 커밋되지 않음.
    private static void SaveSolutionToFile(int levelNum, int moveCount, string moveStr)
    {
        // Application.dataPath = <project>/Assets → 상위 폴더가 프로젝트 루트
        string rootDir     = System.IO.Path.GetDirectoryName(Application.dataPath);
        string solutionDir = System.IO.Path.Combine(rootDir, SolutionsFolder);
        if (!System.IO.Directory.Exists(solutionDir))
            System.IO.Directory.CreateDirectory(solutionDir);

        string filePath = System.IO.Path.Combine(solutionDir, $"level_{levelNum:D2}.txt");
        string content  = $"Level {levelNum} - {moveCount}수\n{moveStr}\n";
        System.IO.File.WriteAllText(filePath, content);
    }

    // 레벨 파라미터 struct
    private struct LevelConfig
    {
        public int colorCount;
        public int emptyTubeCount;
        public int tubeCapacity;
    }
}
