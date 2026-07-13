# Unity 소스 카탈로그

검증일: 2026-07-12

## 판정 기준

- **A**: 명시적 MIT, Apache-2.0, BSD, CC0 등으로 코드 재사용 조건이 분명함
- **B**: 구조 연구에 유용하나 Unity Companion License, 별도 약관, 소스/에셋 권리 분리 등 추가 확인이 필요함
- **C**: 공개 저장소이지만 라이선스가 없거나 재사용 범위가 불명확함. 아이디어와 문서만 참고
- **Legacy**: API와 패키지가 오래되어 현재 구현 예제로 복사하면 안 됨

등급은 품질 순위가 아니라 **재사용 위험도**다. 저장소 루트 라이선스와 개별 에셋의 권리는 다를 수 있다.

## 우선 조사 대상

| 우선 | 프로젝트 | 장르/역할 | 핵심 조사 항목 | 상태 | 재사용 판정 |
|---:|---|---|---|---|---|
| 1 | [Game Programming Patterns Demo](https://github.com/Unity-Technologies/game-programming-patterns-demo) | 패턴 최소 예제 | Factory, Pool, Command, State, Observer, MVP | Unity 공식 e-book 보조 예제. README상 패턴은 정답이 아닌 출발점 | C: GitHub 메타데이터에서 명시적 라이선스를 확인하지 못함 |
| 2 | [Chop Chop](https://github.com/UnityTechnologies/open-project-1) | 액션 어드벤처 | SO Event Channel, Runtime Set, FSM, Scene Loader, Audio Cue, Save | Unity 2020.3 LTS. 2021-12 개발 종료 공지 | A: Apache-2.0. 에셋별 고지는 별도 확인 |
| 3 | [Boss Room](https://github.com/Unity-Technologies/com.unity.multiplayer.samples.coop) | 8인 협동 액션 RPG | 서버 권위, Client/Server 분리, Action, RPC, 세션 흐름, Pool | README 기준 Unity 6000.0 LTS 호환. 지속 갱신되는 교육 샘플 | B: 저장소 LICENSE와 포함 에셋 조건을 사용 전에 직접 확인 |
| 4 | [Entity Component System Samples](https://github.com/Unity-Technologies/EntityComponentSystemSamples) | DOTS/ECS 표준 샘플 | Authoring/Baking, System Group, ECB, Burst/Jobs, Netcode | README 기준 Unity 6.2와 Entities 계열 1.4 | B: Unity 샘플 라이선스 확인 필요 |
| 5 | [ECS Network Racing](https://github.com/Unity-Technologies/ECS-Network-Racing-Sample) | 네트워크 레이싱 | Ghost, Command Data, Prediction, Physics, 차량 데이터 | 현재 ECS 샘플에서도 연결되는 소규모 DOTS 게임 | B: Unity 샘플 라이선스 확인 필요 |
| 6 | [Game Lobby Sample](https://github.com/Unity-Technologies/com.unity.services.samples.game-lobby) | 온라인 로비 | 서비스 Adapter/Facade, 비동기 상태, UI Observer, Heartbeat | Lobby/Relay 교육용 버티컬 슬라이스 | B: 교육 구조이며 프로덕션 완성품으로 간주하지 않음 |
| 7 | [Boat Attack](https://github.com/Unity-Technologies/BoatAttack) | 보트 레이싱/렌더링 데모 | 입력-물리 분리, AI 경주, Jobs 부력, Addressables, 카메라 | URP 데모. 대형 에셋과 Git LFS에 주의 | B + Legacy 가능성: 브랜치와 Unity 버전을 고정해 분석 |
| 8 | [Unity Royale](https://github.com/ciro-unity/UnityRoyale-Public) | 카드/타워 디펜스 | 카드 정의-인스턴스 분리, Addressables, Factory, 타깃 선택 | Unity 2020.3 LTS. 마지막 소스 갱신이 오래됨 | C + Legacy: 루트 라이선스 부재, 포함 에셋 조건 별도 |
| 9 | [FPS Sample](https://github.com/Unity-Technologies/FPSSample) | 멀티플레이 FPS | Fixed Tick, 서버 권위, 예측, Snapshot/보간, Ability | Unity 2018.3 시대 기술. 유지보수 중단 안내 | Legacy: 경계와 개념만 참고 |
| 10 | [Addressables Sample](https://github.com/Unity-Technologies/Addressables-Sample) | 콘텐츠 생명주기 | Load/Release 대칭, 참조 계수, 카탈로그, 중복 의존성 | Addressables 기능별 예제 모음 | B: 사용하는 샘플 폴더의 조건까지 확인 |
| 11 | [unity-design-patterns](https://github.com/Naphier/unity-design-patterns) | 커뮤니티 패턴 예제 | Command, Factory, Flyweight, Builder, Decorator, Strategy | 공식 샘플의 대조군으로만 사용 | A: CC0-1.0 |

GitHub의 `pushed_at`은 자동화나 메타데이터 변경으로도 갱신될 수 있으므로 유지보수 판단에 단독으로 사용하지 않는다. README, 릴리스, 이슈, 사용 Unity 버전을 함께 본다.

## 장르별 추출 지도

### 액션·어드벤처·싱글플레이 RPG

**주 소스:** Chop Chop

- `StateMachine` 실행기와 상태/동작/조건/전환 정의의 분리
- Inspector에서 편집하는 ScriptableObject 정의와 플레이 중 변하는 런타임 인스턴스의 분리
- Event Channel과 Runtime Set을 통한 씬 간 결합도 완화
- 다중 씬 로딩, 오디오 큐, 퀘스트·인벤토리·저장의 연결 방식

주의: 모든 호출을 SO Event Channel로 바꾸면 의존성이 Inspector에 숨고 실행 흐름 추적이 어려워진다. 도메인 내부의 직접 호출은 유지하고, 수명이나 씬 경계를 넘는 통신에 제한적으로 사용한다.

### 온라인 액션 RPG

**주 소스:** Boss Room

- 서버 권위와 호스트의 client/server 컴포넌트 조합
- 입력 요청, 서버 검증, 결과 복제의 책임 경계
- 캐릭터 Action/Ability와 장시간 동작 오브젝트
- 로비 → 캐릭터 선택 → 게임 → 종료의 네트워크 상태 머신
- 예측을 쓰지 않는 구간에서 애니메이션으로 지연을 감추는 방식

싱글플레이 프로젝트에는 과한 구조다. 서버가 판정해야 하는 온라인 게임에서만 우선 분석한다.

### FPS·슈터

**주 소스:** FPS Sample, Boss Room의 최신 Netcode 비교 사례

- 고정 Tick과 렌더 프레임의 분리
- 입력 Command, 권위 시뮬레이션, Snapshot, 보간
- 시뮬레이션 상태와 캐릭터 프레젠테이션 분리
- 무기/능력 모듈 및 봇과 플레이어의 공통 경계

FPS Sample의 구형 ECS/네트워크 API를 최신 API 예제로 옮겨 쓰지 않는다.

### 레이싱

**주 소스:** Boat Attack, ECS Network Racing

- 입력, 차량 시뮬레이션, AI 조종자의 분리
- Waypoint와 경주 진행 상태
- 카메라 상태 및 로컬 프레젠테이션
- 다수 차량이나 네트워크 예측이 핵심일 때 ECS 적용

Jobs/ECS는 차량 수와 성능 요구가 이를 정당화할 때만 선택한다.

### 카드·타워 디펜스

**주 소스:** Unity Royale, Addressables Sample

- 불변 `CardDefinition`과 전투 중 변하는 `CardInstance` 분리
- 덱/손패, 유닛 생성 Factory, Tower/Unit State
- Targeting Strategy, Projectile Pool, 조합식 Effect/Modifier
- Addressables의 load/release 소유권과 콘텐츠 카탈로그

Unity Royale은 구조 연구용이다. 라이선스가 명확해지기 전 코드를 복사하지 않는다.

### 대규모 전략·탄막·군중

**주 소스:** Entity Component System Samples

- Authoring/Baker와 런타임 컴포넌트 분리
- System Group 실행 순서와 EntityCommandBuffer
- 데이터 레이아웃, Burst/Jobs, Spatial Query
- 이벤트성 데이터를 Entity 또는 버퍼로 표현하는 방법
- SubScene과 콘텐츠 스트리밍

동종 객체가 적은 일반 게임에는 ECS를 기본값으로 두지 않는다.

### 2D 플랫폼·횡스크롤

현재 1차 목록에는 라이선스가 명확하고, 아키텍처 문서까지 갖춘 대표 GitHub 게임이 부족하다. Dragon Crashers는 Unity 학습/에셋 샘플로 분류하고 코드 재사용 후보에서 보류한다. 다음 패턴을 중심으로 별도 소스를 추가 조사한다.

- 입력 버퍼와 Coyote Time
- 지상/공중/벽 접촉 센서와 이동 상태 머신
- Kinematic Motor와 일방향 플랫폼
- Checkpoint/Respawn, Camera Zone, Level Streaming

## 보류 또는 정정

- **PaddleBallSO/PaddleGameSO:** Unity의 ScriptableObject 아키텍처 교육 자료에서 언급되는 사례는 있으나, 이 조사에서 동일 이름의 공식 공개 GitHub 저장소를 확인하지 못했다. URL과 라이선스가 확인될 때까지 카탈로그에 넣지 않는다.
- **Dragon Crashers:** 공식 학습 샘플로는 가치가 있지만 일반적인 GitHub 오픈소스 프로젝트로 단정하지 않는다.
- **Unity Architecture Patterns:** 이름이 유사한 커뮤니티 저장소가 여럿이므로 특정 URL·저자·라이선스가 확정되기 전에는 하나의 공인 프로젝트처럼 다루지 않는다.

## 다음 분석 순서

1. Patterns Demo에서 패턴별 최소 형태와 안티패턴 기록
2. Chop Chop에서 FSM, Event Channel, Runtime Set, Scene Loader의 실제 연결 추적
3. Boss Room에서 게임 흐름과 server/client 책임 표 작성
4. 장르별로 한 프로젝트씩 추가하고 동일한 패턴을 비교
5. 재사용할 가치가 확인된 패턴만 독립 Unity 예제로 재작성
