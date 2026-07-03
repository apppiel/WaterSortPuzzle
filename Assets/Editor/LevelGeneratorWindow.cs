// Unity 에디터 전용 레벨 자동 생성 툴
// 상단 메뉴 Tools → Water Sort → 레벨 생성기 로 열 수 있다.
// 완성 상태에서 역방향으로 섞어 항상 풀 수 있는 퍼즐을 생성한다.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WaterSortPuzzle.Data;

public class LevelGeneratorWindow : EditorWindow
{

  private int _startLevel = 1;   // 생성 시작 레벨 번호
  private int _endLevel = 100; // 생성 끝 레벨 번호
  private string _savePath = "Assets/Levels"; // LevelData 에셋 저장 경로

  // 메뉴에 항목 추가
  [MenuItem("Tools/Water Sort/레벨 생성기")]
  public static void ShowWindow()
  {
    GetWindow<LevelGeneratorWindow>("레벨 생성기");
  }

  // 에디터 창 UI를 그린다
  private void OnGUI()
  {
    GUILayout.Label("레벨 생성 설정", EditorStyles.boldLabel);
    GUILayout.Space(8);

    _startLevel = EditorGUILayout.IntField("시작 레벨", _startLevel);
    _endLevel = EditorGUILayout.IntField("끝 레벨", _endLevel);
    _savePath = EditorGUILayout.TextField("저장 경로", _savePath);

    GUILayout.Space(12);

    if (GUILayout.Button("레벨 생성", GUILayout.Height(40)))
    {
      GenerateLevels();
    }

    GUILayout.Space(8);
    EditorGUILayout.HelpBox(
        "생성된 레벨은 저장 경로에 LevelData_00, LevelData_01... 형태로 저장됩니다.\n" +
        "생성 후 GameManager의 Levels 배열에 순서대로 드래그해 주세요.",
        MessageType.Info);
  }

  // 지정한 범위의 레벨을 생성한다
  private void GenerateLevels()
  {
    // 저장 경로 폴더가 없으면 생성
    if (!System.IO.Directory.Exists(_savePath))
      System.IO.Directory.CreateDirectory(_savePath);

    int generated = 0;
    for (int i = _startLevel - 1; i < _endLevel; i++)
    {
      LevelConfig config = GetConfig(i);
      LevelData data = GenerateLevel(config, i);

      // 파일명: LevelData_00, LevelData_01 ... (두 자리)
      string fileName = $"LevelData_{i:D2}.asset";
      string path = $"{_savePath}/{fileName}";

      // 이미 있는 파일은 덮어쓰지 않는다
      if (AssetDatabase.LoadAssetAtPath<LevelData>(path) != null)
        AssetDatabase.DeleteAsset(path);

      AssetDatabase.CreateAsset(data, path);
      generated++;
    }

    AssetDatabase.SaveAssets();
    AssetDatabase.Refresh();
    Debug.Log($"레벨 {_startLevel}~{_endLevel} 총 {generated}개 생성 완료! ({_savePath})");
  }

  // 레벨 번호에 따라 난이도 설정을 반환한다
  private LevelConfig GetConfig(int levelIndex)
  {
    // 0~19: 쉬움 / 20~59: 보통 / 60~99: 어려움
    if (levelIndex < 20)
    {
      return new LevelConfig
      {
        colorCount = Mathf.Clamp(3 + levelIndex / 7, 3, 4), // 3→4색
        emptyTubeCount = 1,                                       // 빈 튜브 1개
        tubeCapacity = 4,
        shuffleSteps = 100 + levelIndex * 5                    // 100~195
      };
    }
    else if (levelIndex < 60)
    {
      int t = levelIndex - 20;
      return new LevelConfig
      {
        colorCount = Mathf.Clamp(4 + t / 8, 4, 7),          // 4→7색
        emptyTubeCount = 1,
        tubeCapacity = 4,
        shuffleSteps = 200 + t * 5                             // 200~395
      };
    }
    else
    {
      int t = levelIndex - 60;
      return new LevelConfig
      {
        colorCount = Mathf.Clamp(7 + t / 10, 7, 10),        // 7→10색
        emptyTubeCount = 1,
        tubeCapacity = 4,
        shuffleSteps = 400 + t * 5                             // 400~595
      };
    }
  }

  // 역방향 셔플로 항상 풀 수 있는 퍼즐을 생성한다
  // levelIndex를 시드로 사용해 레벨마다 다른 결과가 나오도록 한다
  private LevelData GenerateLevel(LevelConfig config, int levelIndex)
  {
    int cap = config.tubeCapacity;
    int colors = config.colorCount;
    int total = colors + config.emptyTubeCount; // 전체 튜브 수

    // ── 1. 완성 상태 만들기 ──────────────────────────────
    // 각 튜브에 같은 색으로 가득 채운다
    List<List<int>> tubes = new List<List<int>>();
    for (int c = 0; c < colors; c++)
    {
      var tube = new List<int>();
      for (int s = 0; s < cap; s++)
        tube.Add(c);
      tubes.Add(tube);
    }
    // 빈 튜브 추가
    for (int e = 0; e < config.emptyTubeCount; e++)
      tubes.Add(new List<int>());

    // ── 2. 랜덤하게 섞기 (유효한 붓기를 역방향으로 수행) ──
    // levelIndex를 시드로 사용해 레벨마다 다른 패턴이 나오도록 한다
    System.Random rng = new System.Random(levelIndex * 1000 + config.colorCount);
    int steps = config.shuffleSteps;

    for (int step = 0; step < steps; step++)
    {
      // 유효한 붓기 목록을 수집한다
      List<(int from, int to)> validPours = new List<(int, int)>();

      for (int from = 0; from < total; from++)
      {
        if (tubes[from].Count == 0) continue; // 빈 튜브는 출발 불가
        int topColor = tubes[from][tubes[from].Count - 1];

        for (int to = 0; to < total; to++)
        {
          if (from == to) continue;
          if (tubes[to].Count >= cap) continue; // 가득 찬 튜브는 불가

          // 비어있거나 맨 위 색이 같으면 붓기 가능
          if (tubes[to].Count == 0 || tubes[to][tubes[to].Count - 1] == topColor)
            validPours.Add((from, to));
        }
      }

      if (validPours.Count == 0) break; // 더 이상 섞을 수 없으면 종료

      // 유효한 붓기 중 랜덤하게 하나 실행
      var (f, t) = validPours[rng.Next(validPours.Count)];
      int color = tubes[f][tubes[f].Count - 1];

      // 세그먼트 1개씩 이동 (한 번에 전부 옮기면 색이 섞이지 않음)
      tubes[f].RemoveAt(tubes[f].Count - 1);
      tubes[t].Add(color);
    }

    // ── 3. LevelData 에셋 생성 ────────────────────────────
    LevelData data = ScriptableObject.CreateInstance<LevelData>();
    data.tubeCapacity = cap;

    // 색 팔레트 생성 (HSV로 색상환에서 균등하게 선택)
    data.palette = GeneratePalette(colors);

    // 튜브 데이터 저장
    data.tubes = new TubeInitData[total];
    for (int i = 0; i < total; i++)
    {
      data.tubes[i] = new TubeInitData
      {
        segments = tubes[i].ToArray()
      };
    }

    return data;
  }

  // HSV 색상환에서 균등하게 색을 뽑아 팔레트를 생성한다
  private Color[] GeneratePalette(int colorCount)
  {
    // 미리 정의된 색 팔레트 (자연스럽고 구분하기 쉬운 색상들)
    Color[] preset = new Color[]
    {
            new Color(1f,    0.35f, 0.35f), // 빨강
            new Color(0.35f, 0.6f,  1f),    // 파랑
            new Color(0.4f,  0.85f, 0.4f),  // 초록
            new Color(1f,    0.85f, 0.2f),  // 노랑
            new Color(0.85f, 0.4f,  1f),    // 보라
            new Color(1f,    0.6f,  0.2f),  // 주황
            new Color(0.2f,  0.85f, 0.85f), // 하늘
            new Color(1f,    0.5f,  0.75f), // 핑크
            new Color(0.5f,  0.35f, 0.2f),  // 갈색
            new Color(0.6f,  0.6f,  0.6f),  // 회색
    };

    Color[] palette = new Color[colorCount];
    for (int i = 0; i < colorCount; i++)
    {
      palette[i] = preset[i % preset.Length];
      palette[i].a = 1f; // 반드시 Alpha=1 (기본값 0이면 투명)
    }
    return palette;
  }

  // 레벨 난이도 파라미터를 담는 구조체
  private struct LevelConfig
  {
    public int colorCount;     // 색 종류 수
    public int emptyTubeCount; // 빈 튜브 수
    public int tubeCapacity;   // 튜브 용량
    public int shuffleSteps;   // 섞기 횟수 (많을수록 어려움)
  }
}
