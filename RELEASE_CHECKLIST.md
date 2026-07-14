# Water Sort Puzzle — 출시 체크리스트

Android(Play Store) 우선. iOS는 별도 섹션.
빼먹으면 심사 반려/승인 지연/스토어 등록 자체 불가능한 항목 위주로 작성.

---

## 📋 Phase 0: 계정·정책 (한 번만)

### Google Play Console 계정
- [ ] Google 계정으로 개발자 등록 ($25 일회성 결제)
- [ ] 개발자 프로필: 이름/이메일/웹사이트 등록
- [ ] 지불 프로필 (결제 정보) — 유료 앱/IAP 있으면 필수, 광고만 있으면 불필요
- [ ] 세금 정보 (2단계 인증 필요)

### AdMob 계정
- [ ] AdMob 계정 생성 (Google 계정으로)
- [ ] 결제 정보 등록 (수익 $100+ 되면 지급)
- [ ] 세금 정보 (미국 세금 양식 등)
- [ ] 앱 등록 (Package Name 등록)
- [ ] 광고 단위 생성 (배너/전면/보상형 각각)

### 법적 문서
- [ ] **개인정보 처리방침 URL** ⚠️ 필수 (스토어 + AdMob 둘 다 요구)
  - 무료 생성: `privacypolicygenerator.info`, `app-privacy-policy-generator.firebaseapp.com`
  - GitHub Pages / Notion 공개 페이지 등에 호스팅
  - 반드시 수집 데이터 목록 명시 (광고 ID, 기기 정보 등)
- [ ] (선택) 이용약관 URL — 크리티컬 X, 있으면 좋음
- [ ] (선택) COPPA 대응 — 13세 미만 타겟이면 필수

---

## 📋 Phase 1: 앱 자체 준비 (코드/에셋)

### 앱 아이덴티티
- [x] Company Name: NaNaBox
- [x] Product Name: Water Sort Puzzle
- [x] Package Name: `com.nanabox.watersortpuzzle`
- [x] Version: 1.0 / Bundle Version Code: 1
- [ ] **앱 아이콘** (현재 임시)
  - Adaptive icon: 108x108dp (foreground + background 각각)
  - Legacy icon: 512x512 PNG
  - Round icon
  - Unity: `Player Settings → Icon` 섹션에서 각 크기별 설정

### 스토어 리스팅 자료 (Play Console 업로드용)
- [ ] **앱 이름** (스토어 표시용, 최대 30자, 예: "물병 정렬 퍼즐 - 워터 소트")
- [ ] **간단한 설명** (80자, 리스트 노출용)
- [ ] **자세한 설명** (4000자, 앱 상세 페이지)
  - 게임 소개, 특징, 100레벨, 랜덤 상품 코드 등
  - 키워드 자연스럽게 포함 (물병, 정렬, 퍼즐, 색깔, 힐링)
- [ ] **스크린샷** (최소 2장, 권장 6~8장)
  - Phone: 16:9 또는 9:16 (Portrait), 최소 320px, 최대 3840px
  - 게임 진행 화면, 클리어 팝업, 레벨 선택 화면 등 다양하게
  - 첫 2-3장이 리스트에 노출되므로 가장 매력적인 것 배치
- [ ] **Feature Graphic** (1024x500 PNG/JPG) — 필수. 스토어 상단 배너
- [ ] **아이콘** (512x512 PNG, 32bit with alpha) — 필수
- [ ] (선택) 프로모션 비디오 (YouTube URL, 30초 이내 권장)
- [ ] (선택) TV 배너, 태블릿 스크린샷

### 로컬라이제이션 (한국 우선)
- [ ] 앱 이름 (ko)
- [ ] 간단한 설명 (ko)
- [ ] 자세한 설명 (ko)
- [ ] (선택) 영어 버전 추가 시 글로벌 노출↑

---

## 📋 Phase 2: 게임 기능 / 정책 준수

### 필수 기능 추가
- [ ] **UMP (User Messaging Platform) SDK** — EU 유저 GDPR 동의 UI
  - AdMob SDK와 함께 통합
  - 첫 실행 시 동의 팝업
  - Firebase Analytics 쓰면 그것도 UMP 적용됨
- [ ] **광고 없이 리셋 대체 옵션** — 광고 미준비 시 유저 갇힘 방지
- [ ] (선택) **튜토리얼** — 첫 레벨에서 손가락 안내
  - 이거 없으면 유저 첫 5분 이탈률↑ (강력 추천)
- [ ] (선택) BGM 1곡 — AudioManager 슬롯 이미 있음

### 광고 정책 준수 (AdMob 승인 필수)
- [ ] 광고 클릭 유도 문구 없음 ("여기 눌러주세요" X)
- [ ] 배너와 콘텐츠 명확히 구분 (경계 확실)
- [ ] 광고가 UI 요소와 겹치지 않음
- [ ] 실수 클릭 유도 안 함 (X 버튼 광고 위 배치 X)
- [ ] 전면 광고 노출 빈도 적정 (매 레벨마다는 X, 3-5레벨마다 정도)
- [ ] 보상형 광고 보상 명확 표시 ("리셋을 위해 광고 시청" 등)
- [ ] 광고 없이도 앱 기본 기능 사용 가능해야 함

### 개인정보/데이터
- [ ] 앱 내 개인정보 처리방침 링크 노출 (설정 메뉴 등)
- [ ] 광고 ID 사용 명시
- [ ] 수집 데이터 목록 정확히 파악 (Data safety form 작성용)

---

## 📋 Phase 3: 빌드 & 서명

### Keystore 생성 (⚠️ 절대 잊으면 안 되는 것)
- [ ] **Custom Keystore 생성** (한 번 만들면 이 앱 평생 사용)
  - Unity: `Player Settings → Publishing Settings → Keystore Manager`
  - Alias, 비밀번호 안전한 곳에 저장
- [ ] **Keystore 파일 백업** (⚠️ 잃으면 앱 업데이트 영영 못 함)
  - Google Drive / iCloud / 물리 백업
  - 비밀번호 별도 저장
- [ ] Play App Signing 활성화 (Play Console에서 자동 관리)

### 빌드 세팅 (Release)
- [ ] `Development Build` **해제** (매우 중요 — 켜져 있으면 테스트 광고 나감)
- [ ] `Build App Bundle (Google Play)` **체크** (AAB 필수, APK 안 됨)
- [ ] `Minify → Release` **체크** (APK 크기 감소, ProGuard 최적화)
- [ ] Custom Keystore 선택 및 비밀번호 입력
- [ ] Bundle Version Code 이전 버전보다 크게 증가 (첫 릴리즈는 1)

### 기술 요구사항 (2026년 기준 Play Store 요구)
- [x] Target SDK 34+ (Android 14) — Automatic 설정이면 자동 최신
- [x] Min SDK 25 (Android 7.1) — 21 이상이면 OK
- [x] ARM64 지원 (64bit 필수)
- [ ] AAB 형식 업로드 (APK 안 받음)
- [ ] APK/AAB 크기 150MB 이하 (초과 시 Play Asset Delivery)

### AndroidManifest 확인
- [ ] `INTERNET` permission (AdMob 필수)
- [ ] `AD_ID` permission (Android 13+, AdMob 필수)
- [ ] `ACCESS_NETWORK_STATE` (AdMob 권장)
- [ ] `com.google.android.gms.permission.AD_ID` 추가
- [ ] AdMob App ID meta-data 등록
- [ ] Deep link 필요하면 intent-filter

---

## 📋 Phase 4: 테스트 (Play Console 트랙)

### Internal Testing (즉시 배포, 최대 100명)
- [ ] Internal testing 트랙에 AAB 업로드
- [ ] 테스터 이메일 리스트 등록 (본인 이메일 포함)
- [ ] 앱 링크 생성 → 브라우저에서 열어 Play Store로 설치
- [ ] 실기기(갤럭시)에서 전체 플로우 테스트
  - 로딩 → 메인메뉴 → 레벨선택 → 게임 → 클리어 → 다음 레벨
  - Undo, Reset, 메뉴 팝업
  - 광고 노출 (테스트 광고여야 함 = Development Build였을 때)
  - 실제 광고 (Release 빌드)
  - Level 100 클리어 → 리워드 코드 팝업 (구현 후)
  - Safe Area (노치 있는 기기에서)

### Closed Testing (알파)
- [ ] Google Group 만들어 테스터 초대
- [ ] 최소 12명 이상 14일 이상 테스트해야 프로덕션 승격 가능
- [ ] 피드백 수집

### Open Testing (베타)
- [ ] 공개 링크 → 누구나 참여
- [ ] 20-100명 유입 모으는 게 이상적

### Production 승격
- [ ] Closed testing 조건 충족 후 승격 가능
- [ ] Rollout percentage 조정 (10% → 50% → 100% 점진적)

---

## 📋 Phase 5: Play Console 스토어 등록

### 앱 대시보드 세팅
- [ ] 앱 만들기 (앱 이름, 언어, 무료/유료 선택)
- [ ] 카테고리: Games → Puzzle → Casual
- [ ] 태그: 최대 5개 선택 (Puzzle, Casual, Family, Brain Games 등)
- [ ] 이메일, 웹사이트, 개인정보 처리방침 URL 등록

### 필수 선언 (Google이 심사에 반영)
- [ ] **콘텐츠 등급 (IARC)** — 설문지 응답 → 자동 등급 부여
  - Water Sort는 폭력/성적/공포 없음 → 전연령
- [ ] **타겟 연령 & 콘텐츠 설정**
  - 주 타겟 연령 그룹 선택 (13-17, 18+ 등)
  - 아이들에게 매력적인 요소 없음 (Kids app 아님)
- [ ] **광고 포함 여부**: "예" 선택
- [ ] **데이터 안전 (Data Safety)** — 매우 중요
  - 수집 데이터: 광고 ID, 기기 정보 (필요 최소)
  - 사용 목적: 광고
  - 제3자 공유: Google (AdMob), Firebase
  - 보안 관행: HTTPS 전송, 유저 삭제 요청 가능
- [ ] **정부 요구 대상** (한국, 브라질 등 특정 국가 요구사항)

### 국가/지역
- [ ] 배포 국가 선택 (한국만 or 전 세계)
- [ ] 로컬라이제이션 없는 국가는 영어로 자동 표시

### 접근성
- [ ] (선택) 접근성 기능 선언

---

## 📋 Phase 6: iOS 별도 (나중에)

### Apple Developer 계정
- [ ] Apple Developer Program 가입 ($99/년)
- [ ] Team ID 확인
- [ ] Bundle ID 등록 (Identifiers)

### App Store Connect
- [ ] 앱 생성
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

### 모니터링
- [ ] **Firebase Crashlytics** — 크래시 모니터링
- [ ] **Firebase Analytics** — 유저 행동 (이탈 지점 파악)
- [ ] **AdMob 대시보드** — 광고 노출/수익 확인
- [ ] Play Console 리뷰 답변 (48시간 내 응답 권장)

### 업데이트 프로세스
- [ ] Version Name (사용자용): 1.0.1, 1.1.0 등 semantic
- [ ] Version Code (내부): 반드시 증가 (2, 3, 4...)
- [ ] 변경사항 (What's new) 작성 (500자 이내)
- [ ] 새 AAB 빌드 → Internal → Closed → Production 재순환

### 리텐션 개선 (데이터 기반)
- [ ] D1/D7/D30 리텐션 확인
- [ ] 특정 레벨 이탈률 높으면 난이도 조정 or 힌트 기능 추가
- [ ] 광고 시청률 낮으면 UX 개선

---

## 📋 Water Sort Puzzle 프로젝트별 잔여 항목

이미 CLAUDE.md TODO에서 확인한 것:
- [ ] 튜브 스프라이트 정식 아트로 교체 (현재 임시)
- [ ] 로딩 씬 배경 이미지/로고 추가
- [ ] 튜토리얼 (강력 추천 — 리텐션 결정적)
- [ ] BGM 1곡 (AudioManager 슬롯 있음)
- [ ] **리워드 코드 시스템** (Level 100 클리어 → Firebase 저장 → 유저에게 표시)

---

## ⚠️ 자주 반려되는 이유 (미리 방지)

1. **개인정보처리방침 없음/링크 깨짐** → 스토어 즉시 반려
2. **광고 클릭 유도** → AdMob 승인 거부
3. **Target SDK 낮음** → 신규 앱 등록 거부 (2024년 8월부터 API 34+ 필수)
4. **AAB 아님** → 신규 앱 APK 업로드 불가
5. **Keystore 유실** → 업데이트 영영 불가 (⚠️⚠️⚠️)
6. **Data Safety 미기재** → 스토어 페이지 표시 안 됨
7. **Content Rating 없음** → 등록 불가
8. **광고 없이 아무것도 못 하게 만듦** → AdMob 정책 위반
9. **크래시가 심함** → 승인 거부 or 순위 하락
10. **UMP 미구현으로 EU 유저 데이터 무단 수집** → 정책 위반

---

## 📌 우선순위 요약

**당장 (실기 테스트 성공 후)**:
1. 개인정보 처리방침 URL 생성
2. 앱 아이콘 정식 제작
3. 스크린샷 촬영
4. UMP SDK 통합 (GDPR)
5. Custom Keystore 생성 + 백업

**중간 (스토어 등록 전)**:
6. Data safety 설문 작성
7. Content rating 설문
8. 스토어 설명 작성 (한/영)
9. Feature graphic 디자인
10. 튜토리얼 구현

**추가 개선 (출시 후 or 병행)**:
11. Firebase Crashlytics
12. 리워드 코드 시스템 (Level 100)
13. BGM 추가
14. 정식 튜브 아트

---

**이 체크리스트는 프로젝트 진행하며 업데이트해 나가세요.**
완료 항목 `[x]` 처리하면 진척도 한눈에 파악됩니다.
