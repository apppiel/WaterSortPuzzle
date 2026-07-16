# Water Sort Puzzle — 출시 체크리스트

Android(Play Store) 우선. iOS는 별도 섹션.
빼먹으면 심사 반려/승인 지연/스토어 등록 자체 불가능한 항목 위주로 작성.

**현재 상태 (2026-07-16 기준)**: Play Console 프로덕션 트랙 초안 완성. **"검토를 위해 저장" 클릭 하나만 남음** — NO.1 심사 결과 나오면 유저 판단으로 진행.

---

## 📋 Phase 0: 계정·정책 (한 번만)

### Google Play Console 계정
- [x] Google 계정으로 개발자 등록 ($25 일회성 결제) — **법인 계정**
- [x] 개발자 프로필: 이름/이메일/웹사이트 등록
- [x] 지불 프로필 (결제 정보) — 광고만이라 필수 아님 (NO.1 준비 시 완료됨)
- [x] 세금 정보

### AdMob 계정
- [x] AdMob 계정 생성 (Google 계정으로)
- [x] 결제 정보 등록 (수익 $100+ 되면 지급)
- [x] 세금 정보 (미국 세금 양식 등)
- [x] 앱 등록 (Package Name 등록) — `com.nanabox.watersortpuzzle`
- [x] 광고 단위 생성 (전면·보상형 각각)
  - 전면 Android: `ca-app-pub-3079888946602647/4715758711`
  - 보상형 Android: `ca-app-pub-3079888946602647/2117515532`
  - ⏳ **AdMob 새 앱 검증 대기 중** (24-72h) — 검증 완료 전엔 실제 광고 no-fill

### 법적 문서
- [x] **개인정보 처리방침 URL** — `https://nanabox.co.kr/privacy.html`
  - 2026-07-16 갱신: Firebase 3종 (Crashlytics/Analytics/Firestore) 위탁 명시 + 리워드 코드 조항 신설
  - 수집 데이터 (기기 식별자, 광고 ID, 크래시 로그, Analytics 이벤트) 명시
- [ ] (선택) 이용약관 URL — 크리티컬 X, 있으면 좋음
- [ ] (N/A) COPPA — 13세 이상 타겟으로 아동 배제. 처리방침에도 아동 데이터 수집 X 명시.

---

## 📋 Phase 1: 앱 자체 준비 (코드/에셋)

### 앱 아이덴티티
- [x] Company Name: NaNaBox
- [x] Product Name: Water Sort Puzzle
- [x] Package Name: `com.nanabox.watersortpuzzle`
- [x] Version: 1.0 / Bundle Version Code: 1
- [x] **앱 아이콘** — `Assets/Media/NO2Icon.png` (512×512)
  - `PlayerSettings.SetPlatformIcons` 로 3종 슬롯 전부 세팅 (Adaptive 6×2 + Round 6 + Legacy 6 + Application 6)
  - Adaptive icon launcher 마스킹 시 좌우 텍스트 크롭 있으나 사장님 승인

### 스토어 리스팅 자료 (Play Console 업로드용)
- [x] **앱 이름** (스토어 표시) — `나나박스 워터소트`
- [x] **간단한 설명** (80자) — "물을 부어 색을 정리하는 중독성 100단계 퍼즐. 한 번 시작하면 멈출 수 없어요!"
- [x] **자세한 설명** (4000자 이내, 실제 ~850자) — 캐주얼·중독성 톤, 100단계 강조
- [x] **스크린샷** — 폰 3장 업로드 (레벨 선택, 3레벨, 11레벨). `Assets/Media/SS{1,2,3}.jpeg`
  - Development Build 워터마크 있으나 자잘함
- [x] **Feature Graphic** (1024×500) — SVG 로 프로그래매틱 생성 (파스텔 팔레트, 튜브 4개, 나나박스 워터소트 텍스트)
- [x] **아이콘** (512×512) — NO2Icon.png
- [ ] (선택) 프로모션 비디오 (YouTube URL)
- [x] **태블릿 스크린샷** — 폰 스샷 1080×2340 을 좌우 핑크 패딩으로 1316×2340=9:16 만들어 7"·10" 슬롯 동일 이미지 업로드 (`sips --padToHeightWidth 2340 1316 --padColor FFC2C2`)
- [ ] (N/A) TV 배너, Wear/Chromebook 스크린샷

### 로컬라이제이션 (한국 우선)
- [x] 앱 이름 (ko)
- [x] 간단한 설명 (ko)
- [x] 자세한 설명 (ko)
- [ ] (N/A) 영어 버전 — 한국 only 배포 확정, 향후 글로벌 확장 시 추가

---

## 📋 Phase 2: 게임 기능 / 정책 준수

### 필수 기능 추가
- [ ] **UMP (User Messaging Platform) SDK** — ❌ **한국만 타겟으로 초기 출시 → 스킵**. 글로벌 확장 시 필수 (EU 유저 접근 시 AdMob 계정 정지 리스크 + GDPR 벌금).
- [x] **광고 없이 리셋 대체 옵션** — `HandleReset` 은 광고 로드 실패해도 무조건 리셋 실행. 순서도 "리셋 → 광고" 로 뒤집어 이벤트 의존 제거.
- [x] **튜토리얼** — Level 1 첫 이동 손 아이콘 안내 (`TutorialManager.cs`)
  - DOTween pulse/move/fade + PlayerPrefs 완료 저장 + 재설치 시 자동 리셋
  - Editor 전용 `Reset Tutorial On Play` 체크박스 (사장님/QA 시연용, 릴리즈 미포함)
- [x] BGM — AudioManager 슬롯 채워짐

### 광고 정책 준수 (AdMob 승인 필수)
- [x] 광고 클릭 유도 문구 없음
- [x] 배너와 콘텐츠 명확히 구분
- [x] 광고가 UI 요소와 겹치지 않음
- [x] 실수 클릭 유도 안 함
- [x] 전면 광고 노출 빈도 적정 — **3라운드마다 게이팅** (`AdManager.ShowInterstitialGated`, 레벨 3/6/9)
- [x] 보상형 광고 보상 명확 표시 — 리셋 버튼 = 광고 시청 (또는 스킵) 후 리셋
- [x] 광고 없이도 앱 기본 기능 사용 가능 — 광고 실패 시에도 리셋·다음 레벨 모두 동작

### 개인정보/데이터
- [ ] 앱 내 개인정보 처리방침 링크 노출 (설정 메뉴 등) — **미구현**. 현재 처리방침 URL 은 Play Console 스토어 페이지에만 노출됨. 앱 내 설정 메뉴 링크 있으면 좋으나 필수 X.
- [x] 광고 ID 사용 명시 (처리방침 제5조 위탁 표에 AdMob 명시)
- [x] 수집 데이터 목록 정확히 파악 (Data safety form 작성용)

---

## 📋 Phase 3: 빌드 & 서명

### Keystore 생성 (⚠️ 절대 잊으면 안 되는 것)
- [x] **Custom Keystore** — NO.1 공유: `/Users/choiseungjin/letme/unity_/Keystore/NANA_user.keystore`, alias `nana_key`
  - `PlayerSettings.Android.keystoreName`/`keyaliasName` 세팅, `useCustomKeystore=true`
- [x] **Keystore 파일 백업** — 프로젝트 밖 별도 폴더 보관 (git 노출 방지)
- [x] Play App Signing 활성화 — Play Console 에서 자동. 우리 keystore = "업로드 키" 로만 사용, Google 이 재서명.

### 빌드 세팅 (Release)
- [x] `Development Build` **해제**
- [x] `Build App Bundle (Google Play)` **체크** (`buildAppBundle=true`)
- [ ] `Minify → Release` — 안 켬. Play Console 에 "R8/proguard mapping 없음" 경고 뜨지만 무시 OK (개발자 디버깅 편의 관련, 심사 통과 지장 X)
- [x] Custom Keystore 선택 및 비밀번호 입력 (Unity Editor 세션에만 저장, 재시작 시 재입력 필요)
- [x] Bundle Version Code 1 (첫 릴리즈)

### 기술 요구사항 (2026년 기준 Play Store 요구)
- [x] Target SDK 35 (Android 15) — 명시 설정
- [x] Min SDK 25 (Android 7.1)
- [x] ARM64 지원
- [x] AAB 형식 업로드
- [x] AAB 크기 150MB 이하

### AndroidManifest 확인
- [x] `INTERNET` permission (AdMob 필수) — Unity/AdMob SDK 자동
- [x] `AD_ID` permission (Android 13+, AdMob 필수) — AdMob SDK 자동
- [x] `ACCESS_NETWORK_STATE` (AdMob 권장) — AdMob SDK 자동
- [x] `com.google.android.gms.permission.AD_ID` — AdMob SDK 자동
- [x] AdMob App ID meta-data 등록 — `GoogleMobileAdsSettings.asset` 의 `adMobAndroidAppId` 필드 (Custom Main Manifest 활성화 금지)
- [ ] (N/A) Deep link intent-filter

---

## 📋 Phase 4: 테스트 (Play Console 트랙)

**법인 계정이라 12명 테스터 14일 폐쇄 테스트 요건 면제됨.** 프로덕션 트랙 직접 배포 가능.

### Internal Testing (즉시 배포, 최대 100명)
- [ ] (스킵) 프로덕션 직접 진행. 필요시 향후 업데이트에서 활용.

### Closed Testing (알파)
- [ ] (N/A) 법인 계정 면제

### Open Testing (베타)
- [ ] (스킵) 필요 없음

### Production 승격
- [x] Play Console 프로덕션 트랙 국가 = 한국 only 설정
- [x] AAB 업로드 완료 (`WaterSortPuzzle-1.0.aab`)
- [x] 출시명 `1 (1.0)`, 출시 노트 작성
- [ ] **"검토를 위해 저장" 클릭** — NO.1 심사 결과 확인 후 유저 판단
- [ ] Rollout percentage — 첫 릴리즈는 100% 로 갈 계획 (단계 배포 안 함)

### 실기 QA 라운드 (갤럭시 S22+ 검증)
- [x] 로딩 → 메인메뉴 → 레벨선택 → 게임 → 클리어 → 다음 레벨
- [x] Undo, Reset, 메뉴 팝업
- [x] 전면 광고 게이팅 (레벨 3/6/9)
- [x] 보상형 광고 (리셋)
- [x] Level 100 클리어 → 리워드 코드 발급 + nana-no2 Firestore 저장 (이전 세션 검증)
- [x] Safe Area (갤럭시 S22+ 펀치홀 대응)
- [x] 튜토리얼 (Level 1 손 아이콘)
- [x] Firebase Analytics 실기 이벤트 도착
- [ ] Firebase Crashlytics — 활성 유저 임계치 대기
- [ ] 실제 광고 (AdMob) — 24-72h 검증 대기

---

## 📋 Phase 5: Play Console 스토어 등록

### 앱 대시보드 세팅
- [x] 앱 만들기 (앱 이름 `나나박스 워터소트`, 언어 ko-KR, 게임, 무료)
- [x] 카테고리: Games → **퍼즐**
- [x] 태그
- [x] 이메일 (`nanabox79@gmail.com`), 웹사이트 (`https://nanabox.co.kr`), 개인정보 처리방침 URL 등록

### 필수 선언 (Google이 심사에 반영)
- [x] **콘텐츠 등급 (IARC)** — 전 지역 전체이용가/3세 이상 (폭력/성적/도박 등 다 아니오)
- [x] **타겟 연령 & 콘텐츠 설정** — 13-15 + 16-17 + 18+ 세 개 (아동 배제로 COPPA/GDPR-K 회피, 개인화 광고 정책 준수)
- [x] **광고 포함 여부**: "예" 선언
- [x] **데이터 안전 (Data Safety)** — 4 카테고리 선언
  - 앱 활동 (앱 상호작용) — Analytics 이벤트
  - 앱 정보 및 성능 (비정상 종료 로그, 진단)
  - 기기 또는 기타 ID (deviceUniqueIdentifier + GAID)
  - 각각 수집·공유·필수·처리방침과 일치. 목적: 애널리틱스·앱 기능·광고. 삭제 요청 = 처리방침 URL 안내.
- [x] **정부 요구 대상**: N/A (정부 앱 아님)

### 국가/지역
- [x] 배포 국가 = **대한민국** only (UMP 스킵의 전제)

### 접근성
- [ ] (스킵) 접근성 기능 선언 — 특별한 접근성 대응 없음

---

## 📋 Phase 6: iOS 별도 (나중에)

**Android 안정화 후 별도 세션에서 진행.**

### Apple Developer 계정
- [ ] Apple Developer Program 가입 ($99/년)
- [ ] Team ID 확인 → `PlayerSettings.iOS.appleDeveloperTeamID`
- [ ] Bundle ID 등록 (Identifiers) → `com.nanabox.watersortpuzzle` (Android와 통일)

### Firebase iOS 세팅
- [ ] Firebase Console에서 nana-no2 프로젝트에 iOS 앱 추가
- [ ] `GoogleService-Info.plist` 다운로드 → `Assets/` 루트에 배치
- [x] `app-ads.txt` 업로드 완료 — `https://nanabox.co.kr/app-ads.txt` (내용: `google.com, pub-3079888946602647, DIRECT, f08c47fec0942fa0`). 도메인 하나로 NO.1·NO.2·Android·iOS 모두 커버. ⏳ AdMob 크롤링·승인 대기 중 (NO.1 iOS 도 이 승인 완료 시점에 정상화됨).

### App Store Connect
- [ ] 앱 생성
- [ ] iOS 앱 아이콘 세팅 (`PlayerSettings.SetPlatformIcons(iOS, ...)`)
- [ ] 스크린샷 (iPhone 6.5", 5.5" 각 3-10장)
- [ ] 앱 리뷰용 데모 계정 (있으면)

### 필수 코드
- [ ] **ATT (App Tracking Transparency) 팝업** — iOS 14+ 필수
  - `AppTrackingTransparency` framework 사용
  - AdMob SDK와 통합
  - 승인 안 하면 personalized 광고 불가 (여전히 non-personalized는 가능)
- [ ] Info.plist에 사용 이유 문구 추가

### Xcode 빌드
- [ ] Team 서명 설정
- [ ] Provisioning profile
- [ ] Archive → App Store Connect 업로드

---

## 📋 Phase 7: 출시 후 (Post-Launch)

**아직 심사 요청 전. 심사 통과 & 스토어 공개 후 시작.**

### 모니터링
- [x] **Firebase Crashlytics** 세팅 완료 (`AppBootstrap.cs`)
- [x] **Firebase Analytics** 세팅 완료 (`level_start`, `level_clear`, `reward_issued` 커스텀 이벤트)
- [ ] AdMob 대시보드 — 광고 노출/수익 확인 (출시 후)
- [ ] Play Console 리뷰 답변 (48시간 내 응답 권장)

### 업데이트 프로세스
- [ ] Version Name (사용자용): 1.0.1, 1.1.0 등 semantic
- [ ] Version Code (내부): 반드시 증가 (2, 3, 4...)
- [ ] 변경사항 (What's new) 작성 (500자 이내)
- [ ] 새 AAB 빌드 → Production 직접 (Internal/Closed 트랙 스킵)

### 리텐션 개선 (데이터 기반)
- [ ] D1/D7/D30 리텐션 확인
- [ ] 특정 레벨 이탈률 높으면 난이도 조정 or 힌트 기능 추가
- [ ] 광고 시청률 낮으면 UX 개선

---

## 📋 Water Sort Puzzle 프로젝트별 잔여 항목

- [ ] 튜브 스프라이트 정식 아트로 교체 (현재 임시)
- [ ] 로딩 씬 배경 이미지/로고 추가
- [x] 튜토리얼 — Level 1 손 아이콘 안내 완성 (2026-07-16)
- [x] BGM (AudioManager 슬롯 채워짐)
- [x] **리워드 코드 시스템** — Level 100 클리어 → nana-no2 Firestore 저장 → RewardPopup 표시 (이전 세션 완료)
- [ ] 아이콘 압축 경고 처리 — `NO2Icon.png` 텍스처 압축을 None 으로 (선택, APK 500KB 증가하나 아이콘 품질 최선)
- [ ] 이메일 포워딩 — Cafe24 도메인 서비스에서 `developer@nanabox.co.kr → nanabox79@gmail.com` (처리방침 연락처와 실 수신 통일)

---

## ⚠️ 자주 반려되는 이유 (미리 방지)

1. **개인정보처리방침 없음/링크 깨짐** → 스토어 즉시 반려 ✅ 대응됨
2. **광고 클릭 유도** → AdMob 승인 거부 ✅ 대응됨
3. **Target SDK 낮음** → 신규 앱 등록 거부 (2024년 8월부터 API 34+ 필수) ✅ 35 로 설정
4. **AAB 아님** → 신규 앱 APK 업로드 불가 ✅ AAB
5. **Keystore 유실** → 업데이트 영영 불가 ✅ NO.1과 공유, 별도 폴더 백업, Play App Signing 활성화
6. **Data Safety 미기재** → 스토어 페이지 표시 안 됨 ✅ 4카테고리 선언
7. **Content Rating 없음** → 등록 불가 ✅ IARC 완료 (전체이용가)
8. **광고 없이 아무것도 못 하게 만듦** → AdMob 정책 위반 ✅ 광고 실패 시에도 리셋·진행 가능
9. **크래시가 심함** → 승인 거부 or 순위 하락 ✅ Crashlytics 모니터링
10. **UMP 미구현으로 EU 유저 데이터 무단 수집** → 정책 위반 ✅ 한국 only 배포로 EU 유저 접근 불가

---

## 📌 우선순위 요약 (현재)

**즉시 (심사 요청 하나만)**:
1. NO.1 심사 결과 확인
2. NO.2 "검토를 위해 저장" 클릭

**대기 중** (시간 지나면 저절로 or 알림):
- AdMob 실제 광고 검증 (24-72h)
- Firebase Crashlytics 활성 유저 도달

**심사 통과 후**:
- Play Console 리뷰 답변
- Crashlytics/Analytics 실기 데이터 확인
- 리텐션 지표 관찰

**향후 (별도 세션)**:
- iOS 이식
- 튜브 정식 아트
- 아이콘 압축 최적화
- 글로벌 확장 시 UMP SDK + 다국어 + 이용약관

---

**이 체크리스트는 프로젝트 진행하며 업데이트해 나가세요.**
완료 항목 `[x]` 처리하면 진척도 한눈에 파악됩니다.
