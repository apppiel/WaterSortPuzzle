// Water Sort Puzzle 자동 풀이 검증기 (Editor 전용)
// LevelGeneratorWindow가 생성한 레벨이 실제로 풀 수 있는지 확인하고,
// 풀 수 있으면 최단 이동 시퀀스를 반환한다.
//
// 알고리즘:
//   - BFS + 상태 캐싱 (canonical 형태로 정규화해 대칭 상태 중복 방지)
//   - Canonical 형태 = 각 튜브 내용을 문자열로 만들고, 튜브들을 정렬 후 concat
//     (튜브 자체는 서로 구별 없으므로 위치가 달라도 내용이 같으면 같은 상태)

using System;
using System.Collections.Generic;
using System.Text;

namespace WaterSortPuzzle.EditorTools
{
    public static class WaterSortSolver
    {
        // 이동 하나를 표현하는 struct — (출발 튜브, 도착 튜브)
        public struct Move
        {
            public int From;
            public int To;
            public Move(int from, int to) { From = from; To = to; }
            public override string ToString() => $"{From}→{To}";
        }

        // 풀이 결과. Solvable=true면 Solution에 최단 이동 시퀀스가 담긴다.
        // 실패 원인이 시간초과라면 TimedOut=true.
        public class Result
        {
            public bool       Solvable;
            public bool       TimedOut;
            public int        StatesExplored;
            public List<Move> Solution = new List<Move>();
        }

        // 주어진 튜브 배치를 solved 상태로 만드는 최단 이동 시퀀스를 찾는다.
        // capacity: 각 튜브 최대 세그먼트 수
        // tubes: 초기 상태 (각 튜브의 세그먼트 배열, index 0 = 바닥)
        // maxStates: 안전장치. 이 이상 상태를 탐색하면 중단.
        public static Result Solve(int capacity, int[][] tubes, int maxStates = 200000)
        {
            var result = new Result();

            // 이미 solved면 바로 반환
            if (IsSolved(tubes, capacity))
            {
                result.Solvable = true;
                return result;
            }

            // BFS 큐: (현재 상태, 여기까지의 이동 리스트)
            // 상태는 각 튜브를 byte[]로 저장 (빈 슬롯은 -1 대신 배열 길이로 표현)
            var startState = CloneState(tubes);
            var queue      = new Queue<byte[][]>();
            var pathMap    = new Dictionary<string, (string prevHash, Move move)>();
            var visited    = new HashSet<string>();

            string startHash = Canonical(startState);
            queue.Enqueue(startState);
            visited.Add(startHash);
            pathMap[startHash] = (null, default);

            int tubeCount = tubes.Length;

            while (queue.Count > 0)
            {
                if (visited.Count > maxStates)
                {
                    result.TimedOut       = true;
                    result.StatesExplored = visited.Count;
                    return result;
                }

                var state = queue.Dequeue();
                string stateHash = Canonical(state);

                // 이 상태에서 시도 가능한 모든 (from, to) 이동 탐색
                for (int from = 0; from < tubeCount; from++)
                {
                    int fromLen = state[from].Length;
                    if (fromLen == 0) continue; // 빈 튜브는 출발 불가

                    byte topColor    = state[from][fromLen - 1];
                    int  topRunLen   = TopRunLength(state[from]);
                    // 완성된 튜브(단색 + 가득)는 굳이 옮길 필요 없음 → 가지치기
                    if (fromLen == capacity && topRunLen == capacity) continue;

                    for (int to = 0; to < tubeCount; to++)
                    {
                        if (from == to) continue;

                        int toLen = state[to].Length;
                        if (toLen >= capacity) continue; // 도착 튜브가 가득함
                        // 도착이 비어있거나 top이 같은 색일 때만 가능
                        if (toLen > 0 && state[to][toLen - 1] != topColor) continue;

                        // 최적화: 도착이 비어있고 출발이 단색으로 이루어져 있으면
                        // 그냥 위치만 바뀌는 자명한 이동이라 스킵 (탐색 폭발 방지)
                        if (toLen == 0 && topRunLen == fromLen) continue;

                        // 새 상태 생성
                        var next = CloneState(state);
                        int moveCount = Math.Min(topRunLen, capacity - toLen);
                        for (int i = 0; i < moveCount; i++)
                        {
                            // pop from
                            var arrFrom = next[from];
                            var newFrom = new byte[arrFrom.Length - 1];
                            Array.Copy(arrFrom, newFrom, arrFrom.Length - 1);
                            next[from] = newFrom;

                            // push to
                            var arrTo  = next[to];
                            var newTo  = new byte[arrTo.Length + 1];
                            Array.Copy(arrTo, newTo, arrTo.Length);
                            newTo[arrTo.Length] = topColor;
                            next[to] = newTo;
                        }

                        string nextHash = Canonical(next);
                        if (visited.Contains(nextHash)) continue;
                        visited.Add(nextHash);
                        pathMap[nextHash] = (stateHash, new Move(from, to));

                        if (IsSolvedByte(next, capacity))
                        {
                            // 경로 복원
                            result.Solvable       = true;
                            result.StatesExplored = visited.Count;
                            result.Solution       = ReconstructPath(pathMap, nextHash);
                            return result;
                        }

                        queue.Enqueue(next);
                    }
                }
            }

            // 큐가 비었는데 solved 못 찾음 = 풀 수 없는 배치
            result.Solvable       = false;
            result.StatesExplored = visited.Count;
            return result;
        }

        // ── 헬퍼 ────────────────────────────────────────────

        // int[][] 초기 상태 → byte[][]로 복사
        private static byte[][] CloneState(int[][] src)
        {
            var dst = new byte[src.Length][];
            for (int i = 0; i < src.Length; i++)
            {
                dst[i] = new byte[src[i].Length];
                for (int j = 0; j < src[i].Length; j++)
                    dst[i][j] = (byte)src[i][j];
            }
            return dst;
        }

        // byte[][] 상태 복사
        private static byte[][] CloneState(byte[][] src)
        {
            var dst = new byte[src.Length][];
            for (int i = 0; i < src.Length; i++)
            {
                dst[i] = new byte[src[i].Length];
                Array.Copy(src[i], dst[i], src[i].Length);
            }
            return dst;
        }

        // 튜브 배열의 canonical form (튜브 순서 무관)
        // 각 튜브를 hex 문자열로 만들고 정렬 후 concat.
        private static string Canonical(byte[][] state)
        {
            var strs = new string[state.Length];
            for (int i = 0; i < state.Length; i++)
            {
                var sb = new StringBuilder(state[i].Length * 2 + 1);
                for (int j = 0; j < state[i].Length; j++)
                    sb.Append(state[i][j].ToString("X2"));
                strs[i] = sb.ToString();
            }
            Array.Sort(strs, StringComparer.Ordinal);
            return string.Join("|", strs);
        }

        // 튜브 상단의 같은 색 연속 세그먼트 수
        private static int TopRunLength(byte[] tube)
        {
            if (tube.Length == 0) return 0;
            byte top = tube[tube.Length - 1];
            int  n   = 1;
            for (int i = tube.Length - 2; i >= 0; i--)
            {
                if (tube[i] == top) n++;
                else break;
            }
            return n;
        }

        // 각 튜브가 비어있거나 (단색으로 가득참) 이면 solved
        private static bool IsSolved(int[][] state, int capacity)
        {
            foreach (var tube in state)
            {
                if (tube.Length == 0) continue;
                if (tube.Length != capacity) return false;
                int first = tube[0];
                for (int i = 1; i < tube.Length; i++)
                    if (tube[i] != first) return false;
            }
            return true;
        }

        private static bool IsSolvedByte(byte[][] state, int capacity)
        {
            foreach (var tube in state)
            {
                if (tube.Length == 0) continue;
                if (tube.Length != capacity) return false;
                byte first = tube[0];
                for (int i = 1; i < tube.Length; i++)
                    if (tube[i] != first) return false;
            }
            return true;
        }

        // pathMap에서 endHash부터 거꾸로 올라가며 이동 시퀀스 복원
        private static List<Move> ReconstructPath(
            Dictionary<string, (string prevHash, Move move)> pathMap,
            string endHash)
        {
            var moves = new List<Move>();
            string cur = endHash;
            while (pathMap.TryGetValue(cur, out var entry) && entry.prevHash != null)
            {
                moves.Add(entry.move);
                cur = entry.prevHash;
            }
            moves.Reverse();
            return moves;
        }
    }
}
