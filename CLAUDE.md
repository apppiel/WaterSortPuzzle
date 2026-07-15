# CLAUDE.md

Water Sort Puzzle 게임 프로젝트 가이드.
Claude Code가 이 저장소에서 작업할 때 따라야 할 구조와 규칙을 정의한다.

---

## 🔥 다음 세션 시작 시 (2026-07-15 마감 시점)

### 최근 완료 (2026-07-15)
- **Target API Level 35 (Android 15) 명시 설정** — 기존 `AndroidTargetSdkVersion: 0`(Automatic)에서 명시적 35로 변경. Play Store 2025년 8월 요구사항 선반영. 빌드 환경 독립적 결과 보장.
- **앱 재설치 감지 이식** (`LevelProgressManager.cs`) — NO.1의 `ClearPrefsIfReinstalled` 로직을 static method + `RuntimeInitializeOnLoadMethod` 어트리뷰트로 이식. Android `PackageManager.firstInstallTime` 비교 → 재설치 감지 시 `PlayerPrefs.DeleteAll()`. 앱 유지 중 진행도는 그대로 유지, 삭제 후 재설치는 초기화. Editor에선 미동작 (테스트 편의). **실기 검증 필요**.
- **리워드 코드 시스템 완료** (로직·UI 모두 완성, 실기 검증만 남음)
  - Firebase 프로젝트 `nana-no2` 생성 + Firestore(asia-northeast3) 활성화
  - Firebase Unity SDK 13.13.0 이식 (NO.1의 `Assets/Firebase/` 그대로 복사 → 버전 완벽 일치)
  - `Assets/Scripts/Game/RewardManager.cs` 신규 — NO.1과 **동일 스키마**
    - `rewards/{deviceId}` : 기기당 1개 (중복 발급 방지)
    - `code_index/{code}`  : 코드→기기 역조회 (CS 대응용)
    - 코드 포맷: `XXXX-XXXX` (0/O/1/I 제외 32자 charset)
  - `GameManager.PlayClearSequence()`에서 마지막 레벨이면 `_rewardManager?.IssueCode()` 호출
  - RewardManager는 **마지막 레벨에서만 생성** (Firebase init 오버헤드 최소화)
  - UI는 `OnCodeIssued` 이벤트로 분리 (NO.1은 UI Toolkit 하드코딩, NO.2는 `RewardPopup.cs` 가 이벤트 구독)
  - `RewardPopup.cs` uGUI: ClearPopup과 동일한 등장 연출(오버레이 페이드 + 스케일 인 + 타이틀 펀치). 마지막 레벨에서는 ClearPopup 대신 이 팝업이 뜬다.
  - Editor Play 모드에서 발급/저장/팝업 표시 시각 검증 완료

### 다음 세션 즉시 할 일
1. **릴리즈 빌드 + 갤럭시 실기 end-to-end 검증**
   - Build Settings에서 `Development Build` **해제** → Android 빌드 → 갤럭시 설치
   - **재설치 감지 검증**: 앱 설치 → 몇 레벨 클리어 → 앱 삭제 → 재설치 → 레벨 1로 초기화됐는지 확인
   - **리워드 검증**: Level 100 실제 클리어 → RewardPopup 뜨는지 → 코드 표시 → 복사 버튼 동작 → Firestore에 실기 device ID로 문서 생성 확인
   - 동시에 (a) Console 오버레이 배너 사라졌는지 (b) UI pink 정상 (c) 실광고 로드 확인
2. **Firebase Crashlytics + Analytics 초기화** (30분)
   - SDK는 이미 임포트됨 (`Firebase.Analytics.dll`). 사용 코드가 없음
   - Firebase Console에서 Crashlytics 활성화
   - `Crashlytics.ReportUncaughtExceptionsAsFatal = true` 한 줄 + 주요 이벤트 로그 (레벨 시작/클리어/포기, 광고 시청, 리워드 발급)
   - 출시 후 크래시/이탈 데이터 없으면 NO.3 만들 때 판단 근거 사라짐
3. **UMP SDK** (GDPR 동의 UI) — EU 유저 대응, AdMob 정책상 필수
4. **정식 앱 아이콘** — 현재 임시 (Assets/Media/NO2Icon.png 재사용)
5. **개인정보 처리방침 URL** — Play Console 필수
6. **튜토리얼** — 첫 레벨 손가락 가이드. 리텐션 결정적.

> 상세 체크리스트는 `RELEASE_CHECKLIST.md` 참조.

### 알아둘 것 (놓치기 쉬운 함정)
- **AdMob App ID**: `Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset` 의 `adMobAndroidAppId` 필드에 설정. **Custom Main Manifest 활성화 금지** (Unity 기본 manifest가 대체돼 launcher activity 사라짐 = 앱 실행 안 됨).
- **AdMob 광고 ID 분기**: `Debug.isDebugBuild ? TestId : RealId` — `Development Build` 체크 시 자동으로 테스트 ID 사용. 릴리즈는 반드시 해제.
- **Debug.isDebugBuild는 메인 스레드에서만 호출**. AdMob 초기화 콜백 같은 백그라운드 스레드에서 부르면 UnityException. 필드에 캐싱해서 쓸 것.
- **Firebase 초기화는 Editor에서 ~10초** 걸림 (실기에서는 훨씬 빠름). RewardManager는 마지막 레벨 진입 시점에 미리 생성해서 유저가 레벨 푸는 몇 분 동안 init 완료되게 설계.
- **Firebase Firestore 콜백은 main thread를 블로킹하면 안 옴**. `Thread.Sleep` 폴링 금지, `ContinueWithOnMainThread` 사용.
- **google-services.json** (`Assets/` 루트) + **google-services-desktop.json** (`Assets/StreamingAssets/`) 둘 다 필요. 전자는 Android/iOS, 후자는 Editor. 후자는 Firebase Editor가 전자로부터 자동 생성.
- **Firebase 프로젝트 분리 원칙**: NO.1은 `nana-53f2e`, NO.2는 `nana-no2`. Firestore 데이터 절대 섞지 말 것 (어느 게임에서 클리어했는지 구분 필요).
- **재설치 = 초기화 정책** (의도적): `LevelProgressManager.ClearPrefsIfReinstalled` 가 Android auto-backup으로 복원된 진행도를 지운다. "이건 버그 아닌가?" 싶어도 절대 되돌리지 말 것 — NO.1과 동일한 UX 정책이며 개발 중 테스트도 편해짐. 유저가 앱 유지하는 한 진행도는 보존됨.
- **google-services.xml (Android 실기용) 갱신 함정**: `Assets/Plugins/Android/FirebaseApp.androidlib/res/values/google-services.xml` 은 EDM4U가 `google-services.json`으로부터 **처음 한 번만** 생성한다. NO.1에서 폴더 복사 시 이 XML도 딸려오면서 nana-53f2e 값이 그대로 남아 실기에서 NO.1 DB로 데이터가 흘러간 사고가 있었음 (2026-07-15). Firebase 프로젝트 갈아탈 때는 이 XML의 `project_id`, `google_app_id`, `google_api_key` 등을 반드시 눈으로 확인. Editor는 `StreamingAssets/google-services-desktop.json` 을 보므로 Editor 테스트론 못 잡힘 — **실기에서 항상 Firestore Console 눈으로 확인 필수**.
- **레벨 생성기**: 랜덤 분포 + solver 검증 방식. reverse-shuffle 불가능한 게임 룰. **최소 빈 튜브 2개 필수** (1개면 대부분 unsolvable).
- **파일명 규칙**: `LevelData_NN.asset` = 0-based (NN=99 → 레벨 100), `level_NN.txt` = 1-based (게임 UI 번호와 일치).
- **UI 종횡비 대응 원칙**: 배경 스프라이트는 `preserveAspect=true` + 뒤에 solid color fill Image로 letterbox 채우기. Canvas Scaler는 항상 `Match=Width(0)` 유지.

---

## 프로젝트 개요

- 장르: Water Sort Puzzle (색깔 물 분류 퍼즐)
- 엔진: Unity **6000.3.13f1**
- 프로젝트명: **WaterSortPuzzle**
- 형태: 2D · 싱글플레이
- 광고: Unity Ads 예정 (리셋 버튼 등 보상형 광고)
- 데이터베이스: 추후 추가 예정
- 플랫폼: 모바일 (Android / iOS), **세로(portrait) 고정**
- 버전 관리: Git / GitHub

## 플랫폼 / 화면

- 대상: Android · iOS, 세로 고정
- **기준 해상도: 1080×1920 (9:16)** — 디자인·Canvas Scaler의 기준값일 뿐, 이 해상도에 하드코딩하지 않는다.
- 실제 기기는 더 길쭉하므로(19.5:9, 20:9 등) 세로 종횡비 **약 9:16 ~ 9:21** 범위를 지원한다.
- UI Canvas Scaler: `Scale With Screen Size`, Reference Resolution `1080×1920`, Match = **Width(0)**
  (가로 폭 기준으로 스케일 → 길쭉한 기기는 위아래 여백만 늘어나고 레이아웃은 유지)
- **Safe Area**: 노치·펀치홀·홈 인디케이터를 피하도록 `Screen.safeArea` 를 UI에 반영한다.
- 태블릿(넓적한 비율)은 여백 처리로 대응. 태블릿 전용 레이아웃은 후순위.


## 게임 규칙 (도메인)

- 여러 개의 튜브(tube)에 색 세그먼트(segment)가 아래부터 쌓여 있다.
- 한 튜브의 맨 위 색을 다른 튜브로 붓는다(pour). 조건:
  - 대상 튜브가 비어 있거나, 대상 맨 위 색이 부으려는 색과 같을 것
  - 대상 튜브에 빈 공간이 있을 것
  - 맨 위의 "같은 색 연속 세그먼트"를, 대상 공간이 허용하는 만큼 한 번에 옮긴다
- 승리 조건: 모든 튜브가 비어 있거나 한 색으로 가득 차 있을 것

## 아키텍처

핵심 원칙: **게임 로직(순수 C#)과 Unity 표현(MonoBehaviour)을 분리한다.**
Core는 UnityEngine에 의존하지 않는다. 로직이 엔진과 분리돼 있어야 테스트가 쉽고 규칙 변경이 안전하다.

```
Assets/Scripts/
  Core/   # 순수 C#. UnityEngine 참조 금지. 퍼즐 규칙 전부 여기.
  Game/   # MonoBehaviour. Core를 구동하고 화면에 반영.
  Data/   # ScriptableObject 레벨 데이터
  UI/     # 메뉴, HUD
Assets/Levels/   # LevelData 에셋
Assets/Tests/    # EditMode 테스트
```

### Core (순수 C#)
- `Tube` : 세그먼트 스택, 용량, 맨 위 색/연속 개수, 붓기 가능 여부 조회
- `Board` : 튜브 집합 = 현재 게임 상태
- `Board.TryPour(from, to)` : 규칙 검증 + 이동 수행. 옮긴 세그먼트 수 반환(애니메이션용)
- `WinChecker` : 승리 판정
- Undo : 수행한 이동 `(from, to, count)` 을 스택에 기록하고 역연산으로 되돌린다
- 색은 `Color`가 아니라 정수 인덱스(`ColorId`)로 표현한다. 실제 색 매핑은 Data/Game이 담당.

Core는 별도 assembly definition(`WaterSortPuzzle.Core.asmdef`)으로 둔다.
UnityEngine을 참조하지 않으므로, 컴파일러가 경계를 강제한다.

### Game (MonoBehaviour)
- `GameManager` : Board를 소유하고 흐름 제어(레벨 로드 → 입력→Pour → 승리 처리). 인스펙터 필드: `levelData`, `tubeSprite`, `koreanFont`
- `TubeView` : 튜브 하나를 렌더링하고 붓기 애니메이션 재생. Core `Tube`를 읽어 표현만 한다.
- `ClearPopup` : 클리어 시 팝업 UI. Canvas를 코드로 생성. DOTween으로 오버레이 페이드 인 → 패널 스케일 인(OutBack) → 타이틀 펀치 순서로 연출.
- `TubeClickTarget` : Physics2D 클릭 감지용 컴포넌트

### Data
- `LevelData` (ScriptableObject) : 튜브 개수, 튜브 용량, 각 튜브의 초기 세그먼트 배열, 색 팔레트
- 레벨 에셋은 `Assets/Levels/` 에 저장

## 브랜드 컬러

- 라이트 핑크: `#FFC2C2` (RGB 1f, 0.761f, 0.761f)
- 딥 핑크: `#FF8C8C` (RGB 1f, 0.541f, 0.541f)
- UI 포인트 컬러로 사용. 메인 버튼은 딥 핑크, 보조 버튼은 라이트 핑크.

## 주석 규칙

**프로젝트 전체에 한국어 주석을 달아야 한다.** 작성자가 Unity 초보라 코드를 읽으며 배운다.

- 클래스 상단: `//` 로 역할 설명
- 메서드 상단: `//` 로 무엇을 하는지 한 줄 설명
- 주요 필드/프로퍼티: 옆에 `// 설명` 인라인 주석
- 비직관적인 로직: 줄 위 또는 옆에 `// 이유/의미` 설명
- 새 파일 작성 시, 기존 파일 수정 시 모두 주석 포함
- XML `/// <summary>` 사용 금지. 일반 `//` 주석만 사용.

## 코딩 컨벤션

- 네임스페이스: `WaterSortPuzzle.Core`, `.Game`, `.Data`, `.UI`
- 클래스 / 메서드 / 프로퍼티 / 상수: PascalCase
- private 필드: `_camelCase`, 인스펙터 노출 시 `[SerializeField] private`
- 파일 1개당 클래스 1개, 파일명 = 클래스명
- Core에서는 `using UnityEngine;` 금지

## 테스트

- Unity Test Framework (EditMode)로 Core 로직을 테스트한다.
- 최소: `TryPour` 규칙 · 승리 판정 · Undo
- 위치: `Assets/Tests/EditMode/`

## Git

- 기본 브랜치: `main`
- 작업은 feature 브랜치 → PR로 `main` 병합
- 커밋 메시지: Conventional Commits (`feat:` `fix:` `refactor:` `test:` `chore:` `docs:`)
  - 예) `feat: 튜브 붓기 규칙 구현`
- `.gitignore` 는 Unity 공식 템플릿 사용 (Library/, Temp/, Obj/, Build/, Logs/, *.csproj, *.sln 등 제외)
- 생성물(Library/ 등)이 커밋에 들어가지 않게 주의

## 작업 시 유의

- 규칙 변경은 Core에서만. Game/UI는 Core 결과를 표현만 한다.
- 새 기능은 먼저 Core에 순수 로직으로 넣고, 그 다음 View를 붙인다.
- 과하게 일반화하지 말 것. Water sort에 필요한 만큼만 만든다.
- LevelData Palette의 Color는 반드시 Alpha=1(255)로 설정해야 한다. 기본값이 Alpha=0이라 투명하게 렌더링됨.
- 레벨을 추가하면 `GameManager._levels`(인스펙터 배열)와 `LevelSelectManager._totalLevels`를 함께 갱신할 것. 두 숫자가 어긋나면 팬텀 버튼(존재하지 않는 레벨)이 눌릴 수 있음. GameManager.Start()에 방어 코드가 있어 크래시는 막지만 유저가 레벨선택으로 튕겨나가는 어색한 UX가 발생한다.

## 구현 현황 및 TODO

### 완료
- [x] `Core/Tube` — 세그먼트 스택, TopRunLength, CanAccept, IsComplete
- [x] `Core/Board` — TryPour, TryUndo (히스토리 스택)
- [x] `Core/WinChecker` — 승리 판정
- [x] `Core/Move` — Undo용 이동 기록 struct
- [x] `Tests/EditMode` — TubeTests, BoardTests, WinCheckerTests (전부 통과)
- [x] `Data/LevelData` — ScriptableObject (튜브 수, 용량, 팔레트, 초기 세그먼트)
- [x] `Game/TubeView` — 색 세그먼트 렌더링, 선택 하이라이트, 클리어 바운스 애니메이션
- [x] `Game/GameManager` — 레벨 로드, 클릭 입력, Pour 실행, Undo, 승리 감지, ClearPopup 연동
- [x] `Game/ClearPopup` — 클리어 팝업 (오버레이 + 패널 스케일 인 + 타이틀 펀치)
- [x] 붓기 애니메이션 (DOTween 포물선 궤적)
- [x] HUD Undo 버튼 (에디터에서 생성, GameManager.HandleUndo() 연결)
- [x] 한글 폰트 — NanumSquareRoundEB SDF (`Assets/TextMesh Pro/Fonts/`)
- [x] 튜브 스프라이트 — Frame 1.png (`Assets/Sprites/`)
- [x] 프로토타입 플레이 가능 (클리어 확인)
- [x] `UI/SceneNames` — 씬 이름 상수 모음
- [x] `UI/SceneLoader` — 씬 전환 유틸리티
- [x] `UI/LoadingSceneManager` — 로딩 씬 (대기 후 메인메뉴 이동)
- [x] `UI/MainMenuManager` — 메인메뉴 씬 (타이틀 + 플레이 버튼)
- [x] `UI/LevelSelectManager` — 레벨선택 씬 (버튼 100개 자동 생성, 스크롤)
- [x] 씬 전환 시스템 (로딩 → 메인메뉴 → 레벨선택 → 게임 → 클리어 → 다음레벨)
- [x] GameManager 레벨 연동 (PlayerPrefs로 선택 레벨 전달)
- [x] 클리어 팝업 다음 버튼 — 다음 레벨 이동 / 마지막 레벨 시 레벨선택 복귀
- [x] 레벨 진행 상태 저장/해금 시스템 (LevelProgressManager, PlayerPrefs)
- [x] 터치 입력 대응 (Pointer.current로 마우스/터치 통합)
- [x] 모바일 프레임 드랍 개선 (targetFrameRate=60, ContentSizeFitter 비활성화)
- [x] `Editor/LevelGeneratorWindow` — 레벨 자동 생성 툴 (역방향 셔플)
- [x] `Editor/LevelImporterWindow` — 텍스트 붙여넣기로 LevelData 에셋 생성
- [x] 게임 화면 레벨 번호 텍스트 표시 (Level N)
- [x] 물병 스프라이트 코드 생성 (Tube.png / TubeMask.png, 플라스크 모양)
- [x] SpriteMask로 세그먼트를 병 모양으로 클리핑 (물 같은 비주얼)
- [x] 튜브 선택 시 위로 올라오는 하이라이트 연출
- [x] 병 자체가 이동·기울어지며 붓는 애니메이션 (arcsin 공식으로 입구 정렬)
- [x] 튜브 다중 행 레이아웃 — 6개 이상이면 위/아래 두 줄 자동 배치, 크기도 자동 축소
- [x] Google AdMob 연동 (SDK v11.2.0) — 전면·보상형 광고
- [x] 리셋 버튼 (첫 이동 후 활성화, 보상형 광고 시청 후 리셋)
- [x] DOTween 씬 전환 에러 수정 (MainMenuManager, UIButtonEffect SetLink 적용)

### TODO

> **출시 관련 상세 체크리스트는 `RELEASE_CHECKLIST.md` 참조.**

#### 비주얼 / UX
- [ ] 튜브 스프라이트 개선 (현재 코드 생성 임시 에셋)
- [x] 색 세그먼트를 물처럼 보이는 비주얼로 교체

#### UI
- [x] 설정 메뉴 (사운드 등)
- [x] Safe Area 대응 (노치/홈 인디케이터)
- [ ] 로딩 씬 — 배경 이미지/로고 추가
- [x] 메인 메뉴 UI 정렬 (모든 폰 비율 대응) — Background `preserveAspect=true` + 버튼 anchor 재설정 + **PinkFill** 레이어로 letterbox 영역까지 pink 통일 (9:16~9:22 대응)

#### 레벨
- [x] 레벨 데이터 다수 제작 (난이도 순, 100개) — 완료 (1~50 수동, 51~100 생성기)

#### 게임플레이
- [x] 레벨 리셋 버튼 (보상형 광고 시청 후 리셋 — GameManager.HandleReset())
- [x] Undo 3회 제한 (GameManager._remainingUndos, 인스펙터 연결 완료)
- [x] 붓기 실패 피드백 (대상 튜브 shake + fail 사운드 슬롯)
- [x] 메뉴 팝업 (게임 중 레벨선택으로 복귀 — MenuPopup)
- [x] 붓기 애니메이션 속도 최적화 (1.39s → 0.90s)

#### 리워드
- [x] Firebase 프로젝트 `nana-no2` 생성 + Firestore(asia-northeast3) 활성화
- [x] Firebase Unity SDK 13.13.0 이식 (NO.1에서 복사, 버전 일치)
- [x] `RewardManager.cs` — XXXX-XXXX 코드 발급, deviceId 중복 방지, `rewards`/`code_index` 컬렉션 (NO.1과 동일 스키마)
- [x] `GameManager` 훅 — 마지막 레벨 클리어 시 `IssueCode()` 자동 호출
- [x] Editor에서 발급/저장 검증 완료
- [x] uGUI 리워드 팝업 (`RewardPopup.cs`) — ClearPopup 대신 마지막 레벨에서 표시. 코드 박스(딥 플럼 #3D1E5E on 라이트 핑크 #FFEBEB) + 복사 버튼 + 닫기(→레벨선택) 버튼
- [ ] 실기(갤럭시) end-to-end 검증 — 실제 device ID로 Firestore 문서 생성 확인 + 실기에서 팝업 배치 확인
- 결정된 사항: **Firebase 프로젝트는 게임별 분리** (`nana-no2` ≠ `nana-53f2e`). 어느 게임에서 클리어했는지 구분해야 함.

#### 광고 / 수익화
- [x] Google AdMob 연동 (SDK v11.2.0)
- [x] 전면 광고 — 레벨 클리어 후 "다음 레벨" 버튼 시 노출
- [x] 보상형 광고 — 리셋 버튼
- [x] AdManager 스레드 안전성 수정 — `Debug.isDebugBuild`를 Awake에서 필드 캐싱 (MobileAds.Initialize 콜백은 non-main thread라 UnityException 났었음)
- [ ] UMP SDK (GDPR 동의 UI) — EU 유저 대응, AdMob 승인 조건

> **⚠️ 빌드 주의**: `Development Build` 체크 시 Google 테스트 광고 ID 자동 사용.
> 릴리즈 빌드는 반드시 `Development Build` **해제** 후 빌드할 것.
> (AdManager.cs에서 `Debug.isDebugBuild`로 자동 분기)

> **⚠️ AdMob App ID 설정 방법**: `Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset` 의 `adMobAndroidAppId` 필드에 입력.
> `Assets/Plugins/Android/AndroidManifest.xml` 에 하드코딩 하지 말 것 — Custom Main Manifest가 활성화되면 Unity 기본 manifest를 대체해서 launcher activity가 사라져 앱이 실행 안 됨.

#### 기술
- [x] 사운드 (붓기 효과음, BGM)
- [x] 모바일 빌드 설정 (Android / iOS)
- [x] 터치 입력 대응 (Pointer.current로 마우스/터치 통합)
- [x] Android 빌드 성공 (첫 실기 테스트: 갤럭시에서 실행 확인)

### 레벨 생성기 (Editor 툴)

- `Assets/Editor/LevelGeneratorWindow.cs` — 랜덤 분포 + solver 검증 방식
  - reverse-shuffle 불가능 (게임 룰상 완성 상태에서 forward 이동은 permutation만)
  - **최소 빈 튜브 2개 필수** (1개면 random 분포가 대부분 unsolvable)
- `Assets/Editor/WaterSortSolver.cs` — BFS + canonical hashing, 최단 풀이 반환
- 생성 시 각 레벨의 풀이 시퀀스가 `LevelSolutions/level_NN.txt` 로 저장됨 (.gitignore)
- **파일명 규칙**: `LevelData_NN.asset` 은 0-based (NN=99 → 레벨 100), `level_NN.txt` 는 1-based (매칭됨: 게임 UI 번호)