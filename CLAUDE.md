# CLAUDE.md

Water Sort Puzzle 게임 프로젝트 가이드.
Claude Code가 이 저장소에서 작업할 때 따라야 할 구조와 규칙을 정의한다.

---

## 🔥 다음 세션 시작 시 (2026-07-20 밤 마감 시점)

### 최근 완료 (2026-07-20 밤) — 🎉 iOS Archive 3중 이슈 다 뚫고 TestFlight 업로드 완료

**최종 상태**: App Store Connect 서버 processing 완료. TestFlight 에 빌드 "1.0 (1) 제출 준비 완료" 상태. 남은 건 지인 Apple ID 이메일 받아서 내부 테스팅 그룹 만들기 + 초대 발송.

**뚫은 방식 (역순으로 순차 해결)**:
- **이슈 1 해결: SPM Firebase 이중 정의** (`Redefinition of module 'Firebase'`)
  - Xcode workspace → Project Navigator → Unity-iPhone → Package Dependencies 탭 → `firebase-ios-sdk` 선택 → `—` 버튼으로 수동 제거
  - Unity 재빌드 시 EDM4U `SwiftPackageManagerEnabled=false` 유지되므로 재발 없음
- **이슈 2 해결: `Multiple commands produce Metadata.appintents`**
  - Podfile 에서 `use_frameworks! :linkage => :static` → `use_frameworks!` (dynamic) 변경
  - + `post_install` hook 으로 `APPINTENTS_METADATA_ENABLED=NO` + `LM_ENABLE_APP_INTENTS_METADATA_EXTRACTION=NO` + AppIntentsMetadataProcessor build phase 제거
- **이슈 3 발견 & 해결 🔴 (진짜 큰 놈, 여태 안 잡혔던 것): `Undefined symbol: _Firebase_XXX_CSharp_*` 대량 발생**
  - 원인: NO.1 → NO.2 Firebase 이식 시 iOS 네이티브 `.a` **3개 누락**. `Assets/Plugins/iOS/Firebase/` 에 App/Crashlytics 만 있고 Analytics/Auth/Firestore 없음
  - 왜 여태 안 드러났나: Unity Editor 는 `Assets/Firebase/Plugins/*.dll` (managed) 만 있으면 컴파일 통과. iOS Archive 링크 단계에서 처음 발견. 이슈 1, 2 뚫려야 링크까지 도달해서 이슈 3 이 노출됨.
  - 해결: NO.1 (`/Users/choiseungjin/letme/unity_/NANA_puzzle/Assets/Plugins/iOS/Firebase/`) 에서 3개 `.a` + `.meta` 복사 후 Unity iOS 재빌드
  - 자세한 원리·재발 방지: [[project-firebase-ios-native-libs]]

**TestFlight 업로드 프로세스 (다음 iOS 배포 참고)**:
- Xcode Product → Archive → Organizer 자동 오픈 → Distribute App → App Store Connect → Upload → 서명 확인 → Upload
- 서버 processing 10~30분 (자동, 대기)
- Apple ID 이메일로 "빌드 사용 준비 완료" 알림 옴
- 이 시점 App Store Connect → 앱 → TestFlight 탭에 빌드 표시 "제출 준비 완료"

**Podfile 상태 (`/Users/choiseungjin/Desktop/project builds/IOS_Builds2/Podfile`)**:
- Firebase 5종 pods (EDM4U 자동 삽입, SPM 껐으니 정상 동작 확인)
- `use_frameworks!` (dynamic) ✅ 검증됨
- `post_install` hook (App Intents 무효화) ✅ 검증됨
- Unity 재빌드 시 이 편집 초기화. **다음 iOS 빌드 세션 때 위 2개 재적용 필요**. [[project-podfile-manual-fixes]] 참조.

**iOS 규정 준수 (ITSAppUsesNonExemptEncryption)**: TestFlight 배포 시 팝업에 "둘 다 사용하지 않음" 눌러놨음. TestFlight Internal 은 심사 없어 무관. 정식 App Store 심사 제출 전엔 정답인 "표준 암호화만 사용" 으로 App Store Connect 에서 편집(재빌드 불필요).

### (이전) 최근 완료 (2026-07-20 낮) — iOS 이식 대부분 완료, Archive 만 남았던 시점

**외부 준비물 4개 발급 완료**:
- Apple Developer Bundle ID: `com.nanabox.watersortpuzzle` 등록 (Description: `NanaBox Water Sort`)
- Team ID 확인: `X9W8QX2MMN`
- AdMob iOS 앱 이미 등록돼있었음 (`나나박스 WSP`) — 값 재확인만 완료:
  - iOS App ID: `ca-app-pub-3079888946602647~6304172559`
  - Interstitial: `ca-app-pub-3079888946602647/5277463126` (`AdManager.cs:24` 와 일치)
  - Rewarded: `ca-app-pub-3079888946602647/8786865662` (`AdManager.cs:25` 와 일치)
- `GoogleService-Info.plist` (Firebase Console `nana-no2` → iOS 앱 추가 → 다운로드) → `Assets/` 루트 배치. `BUNDLE_ID`/`PROJECT_ID`/`GOOGLE_APP_ID` 다 확인.
- App Store Connect: `나나박스 워터소트` 앱 생성 (SKU `watersortpuzzle-ios`, 한국어, iOS only)

**프로젝트 안 세팅 이식 완료**:
- Unity Player Settings iOS 탭: Bundle Identifier `com.nanabox.watersortpuzzle`, Version `1.0`, Build `1`, Team ID `X9W8QX2MMN`, Automatically Sign 체크 (나중에 Xcode 에서 Manual 로 뒤집음), Deployment Target `15.0`
- `GoogleMobileAdsSettings.asset` `adMobIOSAppId` 필드에 iOS App ID 입력
- iOS 아이콘 19개 슬롯 (Application 5 + Spotlight 4 + Settings 5 + Notifications 4 + Marketing 1) 프로그래매틱으로 전부 `NO2Icon` 세팅 (GUID `d29748adaffc34e5faaa1fc438d7b5b7`)
- `Assets/Plugins/iOS/ATTBridge.mm` 신규 (NO.1에서 참고, 게임 무관 wrapper — 그대로 카피)
- `Assets/Editor/iOSPostBuildProcessor.cs` 확장: `GADApplicationIdentifier` + `NSUserTrackingUsageDescription` 삽입 + `AppTrackingTransparency.framework` Xcode 프로젝트에 링크
- `AdManager.cs` iOS ATT 초기화 로직 추가: `#if UNITY_IOS && !UNITY_EDITOR` 로 ATT 먼저 요청 후 콜백에서 `MobileAds.Initialize`. Editor 는 else 브랜치라 `EntryPointNotFoundException` 회피.
- **EDM4U `SwiftPackageManagerEnabled = false` 로 변경** — 이거 안 하면 Firebase pod 가 Podfile 에 안 들어감 (SPM 에 위임). [[project-podfile-manual-fixes]] 참조.

**아이콘 재디자인** (Play Console `NO.2` 랭킹 오해 방지):
- 이전: `나나박스 games NO.2` (`NO.2` 문구가 랭킹으로 오탐 여지)
- 새로: `나나박스 games / water sort puzzle` — 게임 장르 텍스트 추가. `NO.2` 삭제.
- 파일 위치 동일 (`Assets/Media/NO2Icon.png`, GUID 유지) → 모든 아이콘 슬롯 자동 갱신됨. 코드 변경 X.
- 파일명 대소문자 이슈 (macOS APFS case-insensitive) 로 잠깐 `no2Icon` 로 잘못 저장됐다가 `NO2Icon` 대문자로 재정규화 완료.
- ⚠️ **Play Console 스토어 페이지 아이콘도 별도 재업로드 필요** (앱 번들과 분리 관리). App Store Connect 도 심사 제출 전 1024×1024 재업로드.

**iOS 첫 Xcode Archive 시도 → 3중 이슈 뚫는 중**:
- 빌드 위치: `/Users/choiseungjin/Desktop/project builds/IOS_Builds2/` (프로젝트 밖. `~/Desktop/` 안전). 어제 소실 사고 재발 방지, [[build-output-outside-project]] 참조.
- Unity iOS 빌드 자체는 성공 (`Info.plist` 에 `GADApplicationIdentifier`/`NSUserTrackingUsageDescription` 삽입 확인, AppTrackingTransparency framework 링크 확인, CocoaPods 초기 실행됨)
- **뚫은 이슈 1: Signing** — Xcode Automatic Signing 이 Development 프로파일용 UDID 없어서 Archive 블록. 해결: NO.1 방식 그대로 Manual Signing. `App Store Connect` 타입 Distribution 프로파일 `NanaBox WSP App Store` 생성 → `.mobileprovision` 다운받아 더블클릭 설치 → Xcode 에서 Automatically manage signing 해제 → Release 섹션에 이 프로파일 지정. [[ios-signing-needs-udid]] 참조.
- **뚫은 이슈 2: Firebase pods 누락** — Unity 가 생성한 Podfile 에 AdMob pods 만 있고 Firebase 5종 (Core/Analytics/Auth/Firestore/Crashlytics) 이 없었음. 원인: EDM4U `SwiftPackageManagerEnabled=true` 라 SPM 에 위임했지만 실제 SPM 통합은 안 됨 → 링크 실패. 즉시 우회: Podfile 수동 편집으로 Firebase pods 추가 + `pod install`. 영구 수정: Unity EDM4U 설정 `SwiftPackageManagerEnabled=false` 변경 (완료). 다음 Unity 빌드부턴 Podfile 에 Firebase 자동 포함됨.
- **뚫는 중 이슈 3: `Multiple commands produce Metadata.appintents`** — Firebase iOS 12.15.0 + Xcode + static frameworks 조합 알려진 버그. 시도한 것:
  1. Podfile `post_install` hook 으로 `APPINTENTS_METADATA_ENABLED = NO` + `LM_ENABLE_APP_INTENTS_METADATA_EXTRACTION = NO` 설정 + Pod target 의 AppIntents build phase 제거 → **실패** (Unity-iPhone 메인 타겟에서 자동 생성되는 phase 는 못 막음)
  2. `use_frameworks! :linkage => :static` → `use_frameworks!` (dynamic) 로 변경 → **또 실패**. 이번엔 다른 에러 발생.
- **현재 걸린 곳: SPM Firebase 이중 정의** — 지금 이 시점 stuck. 에러: `Redefinition of module 'Firebase'` in `module.modulemap`. 원인: 과거 EDM4U SPM=true 였을 때 Xcode 프로젝트에 SPM `firebase-ios-sdk` 참조가 baked-in 됨. 지금 CocoaPods 로도 Firebase 넣으니 양쪽 다 활성 → 모듈 이름 충돌. 해결책 (다음 세션 첫 액션): Xcode 프로젝트 → Package Dependencies 탭에서 `firebase-ios-sdk` 수동 삭제.

**Podfile 수동 편집 상태** (`/Users/choiseungjin/Desktop/project builds/IOS_Builds2/Podfile`):
- Firebase 5종 pod 라인 수동 추가 (12.15.0)
- `use_frameworks!` (dynamic 로 변경)
- `post_install do |installer|` hook (App Intents 관련 설정 + phase 삭제)
- 이 수정은 이 빌드 인스턴스에만 있음. Unity 재빌드 시 Podfile 초기화됨. **Unity EDM4U 세팅으로 Firebase 자동 추가되게 해뒀지만 dynamic frameworks / post_install hook 은 여전히 수동** — 다음 iOS 빌드 세션 때 재적용 필요. [[project-podfile-manual-fixes]] 참조.

### 최근 완료 (2026-07-19)
- **프로젝트 폴더 소실 → 복구** (실수로 로컬 폴더 통째로 삭제됨)
  - GitHub (`apppiel/WaterSortPuzzle`) 에서 clone 으로 소스 복구
  - Unity 6000.3.13f1 로 재오픈 → Library/ 2.2GB 자동 재생성 완료 (에러 0)
  - 소실된 것: **배포 keystore** (`/Users/choiseungjin/letme/unity_/Keystore/NANA_user.keystore`). 프로젝트 밖 폴더라 git 미포함. Trash/Time Machine/클라우드 백업 전부 없음 → 원본 완전 소실.
  - **keystore 재생성**: Unity Editor Publishing Settings → Keystore Manager 로 새 파일 생성. 경로·alias(`nana_key`) 기존 그대로 유지. Google Drive 에 즉시 백업. 비번 별도 위치 기록.
  - 지난 커밋 (3ac8184) 에서 SS1/2/3.jpeg.meta 실수 누락 발견 → 함께 커밋 후 push (`e0b77c4`)
  - **심사 중인 앱에는 영향 없음** (원본 keystore 로 서명된 AAB 가 이미 Play 서버에 업로드돼있어 심사 그대로 진행). 다만 **v1.1+ 업데이트 낼 때는 Play Console App integrity → upload key reset 신청 필수** (NO.1·NO.2 각각). 상세는 [[project-keystore-replaced]] memory.

### 최근 완료 (2026-07-16)
- **튜토리얼 시스템 완료** (Level 1 첫 이동 손 아이콘 안내)
  - `Assets/Scripts/Game/TutorialManager.cs` 신규 — World space SpriteRenderer, DOTween pulse/move/fade
  - `GameManager.Start()` 훅: Level 1 진입 + `tutorial_done` PlayerPref 미저장 시 자동 스폰
  - `GameManager.HandleTubeClicked()` 훅: `NotifyTubeSelected(index)` + `NotifyPourSucceeded()`
  - **엄격 매칭**: 힌트한 FROM 튜브(Tube 1) 정확히 눌러야만 손이 TO 로 이동. 엉뚱한 튜브는 무반응 (예전엔 아무 튜브나 누르면 손이 움직여 혼란 있었음)
  - PlayerPrefs `tutorial_done` 저장 → 재출현 방지. 재설치 시 `ClearPrefsIfReinstalled` 로 auto-reset.
  - Editor 전용 `Reset Tutorial On Play` 체크박스 (`#if UNITY_EDITOR` 감쌈, 릴리즈 미포함) — 데모/QA 시연용
  - 손 스프라이트: `Assets/Thoth/Cartoon UI Pack/Material/Cursor_Hand3.png` (textureType 7→8 재임포트)
  - GameScene 의 GameManager Inspector `_tutorialHandSprite` 필드에 연결 완료
- **UMP SDK 검토 후 스킵 결정** — 한국만 타겟으로 초기 출시 확정
  - AdManager 에 UMP 흐름(`ConsentInformation.Update`, `LoadAndShowConsentFormIfRequired`) 잠깐 넣었다가 원복
  - 이유: Play Console 배포 지역 = 한국 only 로 하면 EU 유저 접근 불가 → UMP 불필요
  - 향후 글로벌 확장 시 재작업 (동의 UI + Privacy Options 버튼)
- **개인정보 처리방침 갱신** (`https://nanabox.co.kr/privacy.html`)
  - 시행일: 2026-07-04 → 2026-07-16
  - 제1조 수집 항목 표: **리워드 코드** 행 추가 (기기 식별자, 발급 코드, 수령 여부, 발급 시각)
  - 제3조 보관 기간: 리워드 코드 발급 기록 3년 (전자상거래법 준용)
  - 제5조 처리 위탁: **Firebase 3종** 추가 (Crashlytics, Analytics, Firestore)
  - **제9조 신설**: 리워드 코드 발급 및 보관 (Nana WSP 전용, 삭제 요청 방법 명시)
  - 조항 번호: 기존 9→10, 10→11 밀림
  - Cafe24 편집기에 붙여넣기 배포 완료
- **Android 앱 아이콘 세팅** (`ProjectSettings/ProjectSettings.asset`)
  - 발견: `NO2Icon.png` 는 프로젝트에 있었지만 Player Settings 의 Adaptive/Round/Legacy 슬롯이 **모두 비어있어** 유니티 기본 아이콘이 실기 홈화면에 나가던 상태였음
  - `PlayerSettings.SetPlatformIcons` 로 3종 슬롯 전부 NO2Icon 세팅 (Adaptive 6x2 layers + Round 6 + Legacy 6 + Application 6)
  - 실기 확인: 홈화면에 NO2Icon 정상. Adaptive launcher 마스킹으로 좌우 텍스트 일부 크롭되지만 사장님 승인 완료 (원본 로고 유지)
  - 콘솔에 "Compressed texture NO2Icon is used as icon" 경고 → 압축 sprite 상태로 사용됨. 육안 차이 미미. 필요 시 텍스처 압축 None 으로 갱신 가능.
- **리셋 광고 3라운드 삽질 → "리셋 먼저, 광고 나중" 순서 뒤집기로 최종 해결** (`AdManager.ShowRewarded`)
  - 실기(갤럭시) 확인: 광고 다 봐도 리셋 안 되고 두 번째 클릭에서만 리셋되는 버그
  - **1차 시도** (`fix: 50934c9`): `earned` 플래그 제거하고 close 이벤트에서 무조건 발화 → 실패 (close 이벤트 자체가 안 옴)
  - **2차 시도** (`fix: aa01986`): `OnApplicationFocus` 백업 경로 추가 → 실패 (실기에서 focus 이벤트도 안 잡힘)
  - **3차 최종** (`fix: 73e3200`): 유저 제안 "그냥 순서 뒤집으면 되지 않냐" 채택. `onRewarded` 를 Show 앞에 즉시 실행 → 이벤트 의존 자체 제거. 광고는 리셋된 새 판 위에서 표시.
  - `HandleReset` 시그니처도 변경: `ShowRewarded(onRewarded)` (onFailed 파라미터 제거)
  - **교훈**: 불안정한 SDK 이벤트는 폴백 계층 쌓지 말고 순서 뒤집기·의존 제거를 먼저 고려. 자세한 원칙은 memory 참조.
- **실기 QA 라운드 통과** (갤럭시 S22+)
  - ✅ 튜토리얼 (Level 1 손 아이콘 등장·이동·페이드)
  - ✅ 앱 아이콘 (마스킹 크롭 승인됨)
  - ✅ 튜브 크기 (4~5 튜브 자연스러움, segSize 0.62)
  - ✅ 전면 광고 3라운드 게이팅 (레벨 3/6/9)
  - ✅ 리셋 즉시 실행 + 광고 표시 (3차 fix 후)
  - ✅ Firebase Analytics 실기 이벤트 도착 (level_start, level_clear, screen_view 등)
  - ⏳ Crashlytics: "더 많은 사용자 참여 대기" (유저 수 임계치 도달 필요, 정상)
  - ⏳ AdMob 실제 광고: 24-72h 검증 대기 중 (테스트 광고만 확인)
- **Play Console NO.2 앱 등록 + 배포 초안 저장 완료** (심사 요청 직전 상태)
  - **Keystore 재사용**: NO.1의 `/Users/choiseungjin/letme/unity_/Keystore/NANA_user.keystore` (alias `nana_key`) 를 그대로. `PlayerSettings.Android.keystoreName`/`keyaliasName` 세팅, `useCustomKeystore=true`.
  - **Release AAB 빌드**: `Development Build` 해제 + `buildAppBundle=true` → `WaterSortPuzzle-1.0.aab` 생성. Target SDK 35, arm64-v8a IL2CPP.
  - **앱 만들기**: 앱 이름 `나나박스 워터소트`, 기본 언어 한국어, 게임, 무료, 패키지명 `com.nanabox.watersortpuzzle`
  - **앱 콘텐츠**: 개인정보처리방침 URL, 광고 있음, 로그인 없음, 데이터 삭제 요청 = 처리방침 URL, 콘텐츠 등급 (전체이용가/PEGI 3), 타겟층 13-15/16-17/18+ 세 개 (아동 배제 = COPPA/GDPR-K 회피), 정부앱·금융·건강 모두 X
  - **데이터 보안**: 앱 상호작용(Analytics), 크래시 로그(Crashlytics), 진단(기기·OS), 기기 ID(deviceUniqueIdentifier+GAID) 4개 카테고리만. 각각 수집·공유·필수·처리방침과 일치. 목적: Analytics=애널리틱스, 진단·기기ID=애널리틱스+광고, 기기ID=앱기능+광고.
  - **스토어 등록정보**: 카테고리 퍼즐, 짧은/긴 설명 작성 (Option C 톤 — 캐주얼·중독성 강조), 연락처 `nanabox79@gmail.com` (developer@nanabox.co.kr 는 미수신이라 실 수신 계정 우선). 앱 아이콘 NO2Icon.png, Feature graphic 1024x500 (SVG로 프로그래매틱 생성 후 qlmanage → 1024x1024 → sips crop → 1024x500), 휴대전화 스샷 3장, 태블릿 스샷 3장 (폰 스샷 1080x2340 을 좌우 핑크 패딩으로 1316x2340=9:16 만들어 7"·10" 슬롯 둘 다 동일 이미지 업로드).
  - **프로덕션 트랙**: 국가/지역 = 대한민국 만. AAB 업로드 완료. 출시명 `1 (1.0)`, 출시 노트 작성.
  - **경고 2건 무시**: R8/proguard mapping 없음, 네이티브 디버그 심볼 없음 — 심사 통과에 지장 없고 개발자 디버깅 편의 관련.
  - **현재 상태**: "저장" 은 눌러 초안 완성. **"검토를 위해 저장" 은 아직 X** — NO.1 심사 결과 나올 때까지 대기 후 유저 판단으로 클릭.

### 다음 세션 즉시 할 일

**핵심**: iOS TestFlight 지인 배포. Xcode 관련 작업 다 끝났음. App Store Connect 웹에서만 진행.

0. **시작 시 상태 sanity check** (30초)
   - iOS 빌드 폴더 그대로 유지 필요 없음 (재빌드 안 함): `/Users/choiseungjin/Desktop/project builds/IOS_Builds2/`. 지워도 됨. 다음 iOS 빌드 필요할 때 새로 만듬.
   - Unity 콘솔 에러 0 확인
   - App Store Connect → 나나박스 워터소트 → TestFlight → iOS 빌드 → `1.0 (1)` 상태가 "제출 준비 완료" 여야 함

1. **🔴 최우선: TestFlight Internal Testing 그룹 만들고 지인 초대**
   - 사전 준비: 지인 아이폰의 **App Store 로그인 이메일 (= Apple ID)** 받아둘 것. iCloud/Gmail 다 OK, 하지만 Apple ID 로 등록된 이메일이어야 함. 아니면 초대 링크 클릭해도 "이 초대가 로그인된 계정과 일치하지 않음" 에러.
   - App Store Connect → 나나박스 워터소트 → **TestFlight** 탭 → 왼쪽 사이드바 **"내부 테스팅"** 옆 **파란 +** 클릭
   - 그룹 이름 예: `지인 테스트`. **"업로드된 빌드 자동 배포 활성화"** 체크 (권장 — 다음 빌드도 자동 이 그룹에 배포됨).
   - 만든 그룹 화면 → **테스터** 탭 → **+** → 지인 이메일 + 이름 입력 → 추가. 지인에게 TestFlight 초대 메일 자동 발송됨.
   - **빌드** 탭 → **+** → `1.0 (1)` 선택. 테스트 내용 텍스트박스에 짧게 입력 (예: `첫 iOS 테스트 빌드입니다. 튜토리얼→레벨 진행→광고→리셋 테스트 부탁드립니다.`). 저장.

2. **지인 아이폰에서 QA**
   - 지인이 App Store 에서 **TestFlight** 앱 설치
   - 초대 메일에서 링크 클릭 → TestFlight 앱 자동 실행 → 나나박스 워터소트 다운로드
   - QA 체크리스트: 앱 실행, 튜토리얼 (Level 1 손 아이콘), 튜브 조작, 리셋 (광고는 실기 새 앱이라 no-fill 정상 — 24-72h 후 real ads), Firebase 이벤트 도착 (Firebase Console DebugView 로 확인)
   - iOS 특유 확인 포인트: ATT 팝업 뜨는지 (첫 실행 시 "이 앱이 다른 회사의 앱 및 웹사이트에서 사용자의 활동을 추적하도록 허용" 다이얼로그)

3. **iOS 배포 후 Play Console (Android) 스토어 아이콘 재업로드**
   - Play Console → 나나박스 워터소트 → 스토어 등록정보 → 앱 아이콘 슬롯에 새 `NO2Icon.png` (또는 512×512 리사이즈본) 재업로드
   - 앱 번들의 아이콘과 스토어 페이지 아이콘은 분리 관리됨. `NO.2` 문구 삭제된 새 디자인으로 스토어 페이지도 통일.
   - 이 작업 안 하면 심사엔 통과해도 스토어 리스트에 `NO.2` 남아있어 랭킹 오탐 리스크

4. **NO.1 심사 결과 확인 → NO.2 도 심사 요청** (별도 계열, 병행 가능)
   - NO.1 (한붓그리기) Play Console 상태 확인
   - NO.1 승인 시 → NO.2 도 "검토를 위해 저장" 클릭

5. **정식 App Store 심사 준비** (지인 QA 통과 후)
   - App Store Connect → 나나박스 워터소트 → **배포** 탭 → iOS 앱 1.0 페이지 채우기:
     - 미리보기 및 스크린샷 (iPhone 6.5" 필수, iPad 선택)
     - 앱 정보 (설명, 키워드, 카테고리, URL)
     - 앱 심사 정보 (연락처, 데모 계정 X)
     - 개인정보 처리방침 URL: `https://nanabox.co.kr/privacy.html`
     - App Store 정보 (등급, 가격, 지역)
     - **ITSAppUsesNonExemptEncryption**: "표준 암호화만 사용" 으로 편집 (지금은 "둘 다 사용하지 않음" 되어있음)
   - 심사 제출 → Apple 심사 (수 일~수 주)

6. **다음 iOS 빌드 세션 시 재적용 필요** (Podfile 수동 편집 유실 방지)
   - Unity 로 iOS 재빌드하면 Podfile 새로 생성됨 → 지난 수정 사라짐
   - 다시 필요한 것: (a) `use_frameworks!` (dynamic 로 유지), (b) `post_install` hook (App Intents 관련). Firebase pod 라인은 EDM4U 가 자동 (SPM 껐음).
   - 근본 해결: `Assets/Editor/iOSPostBuildProcessor.cs` 에 Podfile 편집 후처리 로직 추가하면 자동화 가능 (선택). [[project-podfile-manual-fixes]] 참조.

### 대기 중 (시간 지나면 자동 해결되는 항목)
- **AdMob 실제 광고 iOS 새 앱 검증** (24~72h) — TestFlight 첫 실행 시 no-fill 정상
- **Firebase Crashlytics 세션 카운트** — 활성 유저 임계치 도달 후 표시
- **iOS Test ad ID 분기** (`AdManager.cs:15-16`) — 현재 Android test ID 만 하드코딩. TestFlight 는 Release 라 Real ID 사용해서 상관 X, 다만 나중 Development Build 로 iOS 실기 테스트 시엔 NO.1 처럼 iOS test ID 로 분기 필요.

> 상세 체크리스트는 `RELEASE_CHECKLIST.md` 참조.

### 알아둘 것 (놓치기 쉬운 함정)
- **AdMob App ID**: `Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset` 의 `adMobAndroidAppId` 필드에 설정. **Custom Main Manifest 활성화 금지** (Unity 기본 manifest가 대체돼 launcher activity 사라짐 = 앱 실행 안 됨).
- **AdMob 광고 ID 분기**: `Debug.isDebugBuild ? TestId : RealId` — `Development Build` 체크 시 자동으로 테스트 ID 사용. 릴리즈는 반드시 해제.
- **Debug.isDebugBuild는 메인 스레드에서만 호출**. AdMob 초기화 콜백 같은 백그라운드 스레드에서 부르면 UnityException. 필드에 캐싱해서 쓸 것.
- **Firebase 초기화는 Editor에서 ~10초** 걸림 (실기에서는 훨씬 빠름). RewardManager는 마지막 레벨 진입 시점에 미리 생성해서 유저가 레벨 푸는 몇 분 동안 init 완료되게 설계.
- **Firebase Firestore 콜백은 main thread를 블로킹하면 안 옴**. `Thread.Sleep` 폴링 금지, `ContinueWithOnMainThread` 사용.
- **google-services.json** (`Assets/` 루트) + **google-services-desktop.json** (`Assets/StreamingAssets/`) 둘 다 필요. 전자는 Android/iOS, 후자는 Editor. 후자는 Firebase Editor가 전자로부터 자동 생성.
- **Firebase 프로젝트 분리 원칙**: NO.1은 `nana-53f2e`, NO.2는 `nana-no2`. Firestore 데이터 절대 섞지 말 것 (어느 게임에서 클리어했는지 구분 필요).
- **재설치 = 초기화 정책** (의도적): `LevelProgressManager.ClearPrefsIfReinstalled` 가 Android auto-backup으로 복원된 진행도를 지운다. "이건 버그 아닌가?" 싶어도 절대 되돌리지 말 것 — NO.1과 동일한 UX 정책이며 개발 중 테스트도 편해짐. 유저가 앱 유지하는 한 진행도는 보존됨. **`tutorial_done` 플래그도 함께 리셋됨** → 재설치 유저는 튜토리얼 다시 봄 (의도된 동작).
- **google-services.xml (Android 실기용) 갱신 함정**: `Assets/Plugins/Android/FirebaseApp.androidlib/res/values/google-services.xml` 은 EDM4U가 `google-services.json`으로부터 **처음 한 번만** 생성한다. NO.1에서 폴더 복사 시 이 XML도 딸려오면서 nana-53f2e 값이 그대로 남아 실기에서 NO.1 DB로 데이터가 흘러간 사고가 있었음 (2026-07-15). Firebase 프로젝트 갈아탈 때는 이 XML의 `project_id`, `google_app_id`, `google_api_key` 등을 반드시 눈으로 확인. Editor는 `StreamingAssets/google-services-desktop.json` 을 보므로 Editor 테스트론 못 잡힘 — **실기에서 항상 Firestore Console 눈으로 확인 필수**.
- **전면 광고 게이팅 원칙**: `AdManager.ShowInterstitialGated` 로 3라운드마다 한 번만 노출. `ShowInterstitial` 직접 호출은 매번 광고 나오게 됨 (유저 이탈 심각). 새 광고 노출 지점 추가할 때 반드시 게이팅 확인.
- **리셋 순서 = "리셋 → 광고" (의도적, 이벤트 의존 제거)**: `ShowRewarded` 는 `onRewarded` 를 Show 앞에 즉시 실행. 광고는 리셋된 새 판 위에 표시. 이유: AdMob `OnAdFullScreenContentClosed` 가 실기(갤럭시)에서 발화 안 되는 케이스가 있어 close 이벤트에 리셋을 걸면 두 번째 클릭에서만 리셋되던 버그. `OnApplicationFocus` 백업도 실패. 순서 뒤집기로 이벤트 의존 자체를 제거. 광고 수익은 유지됨 (Show 는 여전히 호출). "버튼 눌러도 반응 없음" UX 리스크가 광고 수익 손해보다 훨씬 큼.
- **AdMob 새 앱 검증 대기**: 앱 등록 후 24-72시간은 실제 광고 재고 안 뿌림 (no-fill). 테스트 광고(Google 공식 test ID)는 항상 100% 뜨므로 코드 검증은 그걸로. 실제 광고 안 나온다고 코드 문제로 오해 X.
- **Development Build 오버레이**: Development Build 체크된 APK는 화면 위에 Debug.Log/Warning/Error 를 오버레이로 보여줌. Release 빌드(체크 해제)는 오버레이 없음. 스토어 업로드 전엔 반드시 Release 빌드로 확인.
- **레벨 생성기**: 랜덤 분포 + solver 검증 방식. reverse-shuffle 불가능한 게임 룰. **최소 빈 튜브 2개 필수** (1개면 대부분 unsolvable).
- **파일명 규칙**: `LevelData_NN.asset` = 0-based (NN=99 → 레벨 100), `level_NN.txt` = 1-based (게임 UI 번호와 일치).
- **UI 종횡비 대응 원칙**: 배경 스프라이트는 `preserveAspect=true` + 뒤에 solid color fill Image로 letterbox 채우기. Canvas Scaler는 항상 `Match=Width(0)` 유지.
- **UMP SDK 필요성 = 배포 지역에 따라**: 한국만 배포하면 UMP(GDPR 동의 UI) 불필요. EU/미국 포함 글로벌 배포 시 필수 (AdMob 계정 정지 리스크 + GDPR 벌금 리스크). Play Console → 앱 콘텐츠 → 대상 국가에서 결정. 현재 한국 only 전제로 UMP 스킵.
- **DOTween SpriteRenderer 확장 미포함**: 이 프로젝트 DOTween 은 `DOFade`/`DOColor` SpriteRenderer 확장이 빠져 있음 (Camera·Material 확장만 있음). SpriteRenderer 색상 tween 은 `DOTween.To(() => sr.color, c => sr.color = c, endColor, dur)` 로 직접 tween. `TutorialManager.cs` 참고.
- **튜토리얼 개발자용 리셋 스위치**: `GameManager` Inspector `Reset Tutorial On Play` 체크박스 → Play 시 `tutorial_done` PlayerPref 삭제. `#if UNITY_EDITOR` 로 감싸 릴리즈 빌드 미포함. 사장님/QA 시연용.
- **AdMob 이벤트 실기 불안정성**: `OnAdFullScreenContentClosed` 등 fullscreen 광고 이벤트가 갤럭시 등 실기에서 발화 안 되는 케이스 확인됨. 폴백 이벤트(예: `OnApplicationFocus`) 도 마찬가지로 안 잡히는 경우 있음. **원칙**: 이벤트 폴백 계층 쌓지 말고 순서 뒤집기·의존 제거로 접근. 리셋 광고가 대표 케이스 (`AdManager.ShowRewarded` 코멘트 참조).
- **아이콘 파일 있음 ≠ 세팅됨**: `Assets/Media/NO2Icon.png` 파일 있어도 `Player Settings → Player → Android → Icons` 슬롯이 비어있으면 유니티 기본 아이콘이 나감. Editor 툴바 대신 `PlayerSettings.SetPlatformIcons` 로 프로그래매틱 세팅 가능. 3종(Adaptive/Round/Legacy) + Application 슬롯 전부 채워야 모든 Android 버전 대응. 확인은 `PlayerSettings.GetPlatformIcons` 로 각 슬롯 tex 배열 검사.
- **Keystore 위치 & 재사용 원칙**: 배포용 keystore 는 프로젝트 밖 `/Users/choiseungjin/letme/unity_/Keystore/NANA_user.keystore` 에 별도 보관 (git 노출 방지). Alias `nana_key`. NO.1(NANA_puzzle)과 **동일 keystore 공유** — 같은 nanabox 법인 identity 통일 목적. 새 keystore 만들지 말 것. 비밀번호는 Unity Editor 세션에만 저장되고 재시작 시 재입력 필요 (`Player Settings → Publishing Settings`).
- **Play Console 법인 계정 = 폐쇄 테스트 요건 면제**: 2024 개인 개발자 정책 (12명 테스터 14일) 은 법인 계정에 적용 안 됨. NO.2 는 프로덕션 트랙 직접 배포 가능. 다만 첫 앱 심사는 며칠~수 주 걸릴 수 있음 (계정 신뢰도 검증 병행).
- **Play Console AAB 업로드 경고 2건은 무시 OK**: `R8/proguard mapping file 없음`, `네이티브 디버그 심볼 없음` — 심사 통과에 지장 없고 개발자 크래시 디버깅 편의만 관련. 개선하려면 `Player Settings → Publishing Settings → Minify` + `Symbols.zip` export 켜기 (재빌드 필요).
- **Play Console 스토어 태블릿 스크린샷 대응**: 우리 앱은 portrait 고정, 태블릿 최적화 안 함. 폰 스샷(1080x2340=9:19.5) 은 태블릿 필수 스샷 aspect ratio(9:16 이내) 초과라 그대로 못 씀. **좌우 핑크 패딩으로 1316x2340=9:16 만들어 7"·10" 슬롯 둘 다 동일 이미지 업로드**. sips 로 처리: `sips --padToHeightWidth 2340 1316 --padColor FFC2C2 input.jpg`.
- **Firebase Unity SDK iOS 네이티브 .a 5종 필수**: `Assets/Plugins/iOS/Firebase/` 에 `libFirebaseCppApp.a` + 사용하는 각 모듈별 (`Analytics/Auth/Crashlytics/Firestore`) 총 5개 있어야 함. `.meta` 도 각각. Editor 는 `Assets/Firebase/Plugins/*.dll` (managed) 만 있으면 컴파일 통과하지만 iOS Archive 링크에서 `Undefined symbol: _Firebase_XXX_CSharp_*` 대량 발생. 새 게임 프로젝트 이식 시 반드시 확인. 상세: [[project-firebase-ios-native-libs]].
- **iOS ATT (App Tracking Transparency) 팝업**: 첫 앱 실행 시 "다른 회사의 앱 및 웹사이트 활동 추적 허용" 다이얼로그. `AdManager.cs` iOS 초기화 로직에서 ATT 먼저 요청 후 콜백에서 `MobileAds.Initialize`. `Info.plist` 에 `NSUserTrackingUsageDescription` 문자열 필수 (`iOSPostBuildProcessor.cs` 가 자동 삽입). 유저가 거부해도 광고는 정상 표시 (개인화 광고만 disable).
- **iOS ITSAppUsesNonExemptEncryption**: App Store Connect 빌드 편집 팝업. 실제로 우린 HTTPS(TLS) 사용하니 정답은 "**표준 암호화만 사용**" (Apple 자동 면제). "둘 다 사용하지 않음" 눌러도 심사 통과되긴 하지만 엄밀히 부정확. TestFlight Internal 은 무관, 정식 심사 제출 전 편집 (재빌드 불필요).

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
- [x] 튜토리얼 (Level 1 손 아이콘 안내 — `TutorialManager.cs`, DOTween pulse/move/fade, PlayerPrefs 완료 저장, Editor 리셋 체크박스)

#### 리워드
- [x] Firebase 프로젝트 `nana-no2` 생성 + Firestore(asia-northeast3) 활성화
- [x] Firebase Unity SDK 13.13.0 이식 (NO.1에서 복사, 버전 일치)
- [x] `RewardManager.cs` — XXXX-XXXX 코드 발급, deviceId 중복 방지, `rewards`/`code_index` 컬렉션 (NO.1과 동일 스키마)
- [x] `GameManager` 훅 — 마지막 레벨 클리어 시 `IssueCode()` 자동 호출
- [x] Editor에서 발급/저장 검증 완료
- [x] uGUI 리워드 팝업 (`RewardPopup.cs`) — ClearPopup 대신 마지막 레벨에서 표시. 코드 박스(딥 플럼 #3D1E5E on 라이트 핑크 #FFEBEB) + 복사 버튼 + 닫기(→레벨선택) 버튼
- [x] 실기(갤럭시) end-to-end 검증 — 실기 device ID로 nana-no2 Firestore 문서 생성 ✓
- [x] 홈페이지 `reward-claim.html` NO.2 지원 — 두 프로젝트 병렬 조회, activeDb 로 claims 저장, `game` 필드로 어느 게임 코드인지 구분 (NO.1 리포에 있음)
- 결정된 사항: **Firebase 프로젝트는 게임별 분리** (`nana-no2` ≠ `nana-53f2e`). 어느 게임에서 클리어했는지 구분해야 함.

#### 광고 / 수익화
- [x] Google AdMob 연동 (SDK v11.2.0)
- [x] 전면 광고 — 레벨 클리어 후 "다음 레벨" 버튼 시 노출 (**3라운드마다 게이팅**, `AdManager.ShowInterstitialGated`)
- [x] 보상형 광고 — 리셋 버튼 (**"리셋 → 광고" 순서로 뒤집어 이벤트 의존 제거**, UX 우선. AdMob close 이벤트 실기 불안정 우회)
- [x] AdManager 스레드 안전성 수정 — `Debug.isDebugBuild`를 Awake에서 필드 캐싱 (MobileAds.Initialize 콜백은 non-main thread라 UnityException 났었음)
- [x] 테스트 광고 실기 확인 완료 (Development Build)
- [ ] 실제 광고 실기 확인 (AdMob 새 앱 검증 24-72시간 대기 중)
- [ ] UMP SDK (GDPR 동의 UI) — **한국만 타겟으로 초기 출시 → 스킵**. 글로벌 확장 시 필수 (EU 유저 접근 시 AdMob 계정 정지 리스크 + GDPR 벌금).

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
- [x] Target API Level 35 (Android 15) 명시 — Play Store 2025년 8월 요구사항 선반영
- [x] 앱 재설치 감지 (`LevelProgressManager.ClearPrefsIfReinstalled`) — Android auto-backup 복원 무효화, 삭제 = 진행도 초기화
- [x] Firebase Crashlytics + Analytics — 프로덕션 크래시/유저 행동 트래킹 (`AppBootstrap.cs`, `level_start`/`level_clear`/`reward_issued` 이벤트). 실기 Analytics 이벤트 도착 확인 완료.
- [x] Android 앱 아이콘 (NO2Icon) — Player Settings Adaptive/Round/Legacy 슬롯 전부 세팅 완료 (2026-07-16). 실기 홈화면 확인.

### 레벨 생성기 (Editor 툴)

- `Assets/Editor/LevelGeneratorWindow.cs` — 랜덤 분포 + solver 검증 방식
  - reverse-shuffle 불가능 (게임 룰상 완성 상태에서 forward 이동은 permutation만)
  - **최소 빈 튜브 2개 필수** (1개면 random 분포가 대부분 unsolvable)
- `Assets/Editor/WaterSortSolver.cs` — BFS + canonical hashing, 최단 풀이 반환
- 생성 시 각 레벨의 풀이 시퀀스가 `LevelSolutions/level_NN.txt` 로 저장됨 (.gitignore)
- **파일명 규칙**: `LevelData_NN.asset` 은 0-based (NN=99 → 레벨 100), `level_NN.txt` 는 1-based (매칭됨: 게임 UI 번호)