// 텍스트 형식의 레벨 데이터를 붙여넣으면 LevelData 에셋으로 변환해주는 Editor 툴
// 상단 메뉴 Tools → Water Sort → 레벨 임포터 로 열 수 있다.
// 여러 레벨을 한 번에 붙여넣으면 순서대로 에셋을 생성한다.

using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using WaterSortPuzzle.Data;

public class LevelImporterWindow : EditorWindow
{
    // 전역 팔레트 (고정 11색)
    // 인덱스: 0=빨강 1=주황 2=노랑 3=초록 4=파랑 5=남색 6=보라 7=분홍 8=흰색 9=검정 10=회색
    private static readonly Color[] GlobalPalette = new Color[]
    {
        new Color(0.937f, 0.267f, 0.267f, 1f), // 0 빨강   #EF4444
        new Color(0.976f, 0.451f, 0.086f, 1f), // 1 주황   #F97316
        new Color(0.980f, 0.800f, 0.082f, 1f), // 2 노랑   #FACC15
        new Color(0.133f, 0.773f, 0.369f, 1f), // 3 초록   #22C55E
        new Color(0.231f, 0.510f, 0.965f, 1f), // 4 파랑   #3B82F6
        new Color(0.388f, 0.400f, 0.945f, 1f), // 5 남색   #6366F1
        new Color(0.659f, 0.333f, 0.969f, 1f), // 6 보라   #A855F7
        new Color(0.925f, 0.286f, 0.600f, 1f), // 7 분홍   #EC4899
        new Color(1.000f, 1.000f, 1.000f, 1f), // 8 흰색   #FFFFFF
        new Color(0.094f, 0.094f, 0.106f, 1f), // 9 검정   #18181B
        new Color(0.612f, 0.639f, 0.686f, 1f), // 10 회색  #9CA3AF
    };

    private string _input      = "";             // 붙여넣는 텍스트
    private string _savePath   = "Assets/Levels"; // 저장 경로
    private int    _startIndex = 0;              // 첫 번째 레벨의 파일 번호 (LevelData_00부터)
    private string _errorMessage = "";
    private Vector2 _scrollPos;

    [MenuItem("Tools/Water Sort/레벨 임포터")]
    public static void ShowWindow()
    {
        GetWindow<LevelImporterWindow>("레벨 임포터");
    }

    // 창이 포커스를 받을 때마다 시작 번호를 자동으로 갱신한다
    private void OnFocus()
    {
        _startIndex = GetNextAvailableIndex();
    }

    private void OnGUI()
    {
        GUILayout.Label("레벨 데이터 붙여넣기", EditorStyles.boldLabel);
        GUILayout.Space(4);

        // 팔레트 미리보기
        GUILayout.Label("전역 팔레트:", EditorStyles.miniLabel);
        GUILayout.Label("0:빨강  1:주황  2:노랑  3:초록  4:파랑  5:남색  6:보라  7:분홍  8:흰색  9:검정  10:회색",
            EditorStyles.miniLabel);
        GUILayout.Space(8);

        // 텍스트 입력 영역
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(280));
        _input = EditorGUILayout.TextArea(_input, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        GUILayout.Space(8);
        _savePath = EditorGUILayout.TextField("저장 경로", _savePath);

        EditorGUILayout.BeginHorizontal();
        _startIndex = EditorGUILayout.IntField("시작 파일 번호", _startIndex);
        if (GUILayout.Button("자동", GUILayout.Width(50)))
            _startIndex = GetNextAvailableIndex();
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(8);

        if (GUILayout.Button("LevelData 생성", GUILayout.Height(40)))
        {
            _errorMessage = "";
            TryImport();
        }

        if (!string.IsNullOrEmpty(_errorMessage))
            EditorGUILayout.HelpBox(_errorMessage, MessageType.Error);

        GUILayout.Space(8);
        EditorGUILayout.HelpBox(
            "형식 예시 ([Level N] 헤더 없어도 됨):\n\n" +
            "tubeCapacity: 4\n" +
            "colors: 0, 1, 2\n" +
            "tubes:\n" +
            "  [0]: segments = [2, 2, 1, 0]\n" +
            "  [1]: segments = []\n\n" +
            "tubeCapacity: 4\n" +
            "colors: 2, 3, 4\n" +
            "tubes:\n" +
            "  [0]: segments = [1, 0, 2, 1]\n" +
            "  [1]: segments = []",
            MessageType.Info);
    }

    // 저장 경로에서 다음으로 사용 가능한 파일 번호를 찾는다
    private int GetNextAvailableIndex()
    {
        if (!System.IO.Directory.Exists(_savePath)) return 0;
        int index = 0;
        while (System.IO.File.Exists($"{_savePath}/LevelData_{index:D2}.asset"))
            index++;
        return index;
    }

    // 텍스트 전체를 레벨 단위로 분리해서 각각 에셋으로 만든다
    private void TryImport()
    {
        try
        {
            // [Level N] 헤더 또는 tubeCapacity: 기준으로 레벨 분리
            string text = Regex.Replace(_input.Trim(), @"\[Level\s*\d+\]", "").Trim();
            string[] blocks = Regex.Split(text, @"(?=tubeCapacity\s*:)");
            List<string> levelBlocks = new List<string>();
            foreach (string b in blocks)
            {
                string trimmed = b.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    levelBlocks.Add(trimmed);
            }

            if (levelBlocks.Count == 0)
                throw new System.Exception("레벨 데이터를 찾을 수 없습니다.");

            if (!System.IO.Directory.Exists(_savePath))
                System.IO.Directory.CreateDirectory(_savePath);

            int created = 0;
            for (int i = 0; i < levelBlocks.Count; i++)
            {
                LevelData data = ParseLevel(levelBlocks[i]);
                string path = $"{_savePath}/LevelData_{(_startIndex + i):D2}.asset";

                if (AssetDatabase.LoadAssetAtPath<LevelData>(path) != null)
                    AssetDatabase.DeleteAsset(path);

                AssetDatabase.CreateAsset(data, path);
                created++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"총 {created}개 레벨 생성 완료! ({_savePath})");
        }
        catch (System.Exception e)
        {
            _errorMessage = $"오류: {e.Message}";
        }
    }

    // 레벨 블록 하나를 파싱해서 LevelData를 반환한다
    private LevelData ParseLevel(string block)
    {
        string[] lines = block.Split('\n');

        int tubeCapacity = 4;
        int[] colorIndices = null;
        var tubes = new List<int[]>();
        bool inTubes = false;

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            // 공백이 섞여도 인식하도록 정규식으로 파싱
            if (Regex.IsMatch(line, @"^tubeCapacity\s*:"))
            {
                tubeCapacity = int.Parse(line.Split(':')[1].Trim());
                inTubes = false;
                continue;
            }

            if (Regex.IsMatch(line, @"^colors\s*:"))
            {
                // "colors: 0, 1, 2" 또는 "colors : 0, 1, 2" → [0, 1, 2]
                string[] parts = line.Split(':')[1].Split(',');
                colorIndices = new int[parts.Length];
                for (int i = 0; i < parts.Length; i++)
                    colorIndices[i] = int.Parse(parts[i].Trim());
                inTubes = false;
                continue;
            }

            if (Regex.IsMatch(line, @"^tubes\s*:")) { inTubes = true; continue; }

            // tubes 항목 파싱: [n]: segments = [a, b, c] 또는 []
            if (inTubes && line.StartsWith("["))
            {
                var segMatch = Regex.Match(line, @"segments\s*=\s*\[([^\]]*)\]");
                if (!segMatch.Success)
                    throw new System.Exception($"segments 형식 오류: {line}");

                string inner = segMatch.Groups[1].Value.Trim();
                if (string.IsNullOrEmpty(inner))
                {
                    tubes.Add(new int[0]);
                    continue;
                }

                string[] parts = inner.Split(',');
                var segs = new List<int>();
                foreach (string p in parts)
                    segs.Add(int.Parse(p.Trim()));
                tubes.Add(segs.ToArray());
            }
        }

        if (colorIndices == null) throw new System.Exception("colors 항목이 없습니다.");
        if (tubes.Count == 0)    throw new System.Exception("tubes 항목이 없습니다.");

        // 전역 팔레트에서 지정된 색만 추출
        Color[] palette = new Color[colorIndices.Length];
        for (int i = 0; i < colorIndices.Length; i++)
        {
            int idx = colorIndices[i];
            if (idx < 0 || idx >= GlobalPalette.Length)
                throw new System.Exception($"색 인덱스 {idx}는 범위를 벗어났습니다. (0~{GlobalPalette.Length - 1})");
            palette[i] = GlobalPalette[idx];
        }

        LevelData data = ScriptableObject.CreateInstance<LevelData>();
        data.tubeCapacity = tubeCapacity;
        data.palette      = palette;
        data.tubes        = new TubeInitData[tubes.Count];
        for (int i = 0; i < tubes.Count; i++)
            data.tubes[i] = new TubeInitData { segments = tubes[i] };

        return data;
    }
}
