# Unity C# 프로젝트 코드 스타일 & 아키텍처 지침 (전체 적용)

## 프로젝트 정보 (AI가 첫 대화에서 직접 채운다)

**이 프로젝트 정보: [미확인 — AI는 아래 온보딩 절차를 먼저 실행하라]**

### AI 온보딩 절차 (필수)

위 프로젝트 정보 줄이 "[미확인]" 상태라면, 너(AI)는 어떤 코드 작업(프로토타이핑 포함)을
시작하기 전에 반드시 사용자에게 아래 질문을 먼저 하라. 사용자가 물어보기를 기다리지 말고
네가 먼저 물어라. 답을 받기 전에는 코드를 작성하지 마라.

1. **장르와 핵심 루프**: 어떤 장르인가? 플레이어가 반복하는 핵심 행동 한 줄은?
   (예: "탑다운 액션 로그라이크 — 던전 진입 → 전투 → 보상 → 강화 반복")
2. **개발 단계**: 프로토타입 / 알파 / 베타 / 출시준비 중 어디인가?
   → 17번 유연성 조항의 적용 강도가 여기서 결정된다.
3. **팀 구성**: 혼자인가, 팀인가? 팀이면 몇 명이고 코드 담당은 누구인가?
   → 16번 Git 협업 규칙과 주석 상세도가 여기서 결정된다.
4. **이 세션의 목표**: 지금 만들려는 것이 무엇인가?
   (새 기능 / 리팩토링 / 버그 수정 / 첫 프로토타입 등)
5. **기존 코드 존재 여부**: 빈 프로젝트인가, 기존 코드가 있는가?
   기존 코드가 있다면 이 지침과 다른 스타일인지 먼저 확인하라.

답을 받으면:
- 이 파일의 "프로젝트 정보" 줄을 답 내용으로 직접 수정(편집)해서 다음 세션부터는
  다시 묻지 않게 하라. (예: "이 프로젝트 정보: 탑다운 액션 로그라이크 장르의
  프로토타입 단계, 2인 팀, Unity 프로젝트다.")
- 파일 수정 권한이 없는 환경이라면, 사용자에게 위 형식으로 한 줄을 채워달라고 요청하라.
- 이미 프로젝트 정보가 채워져 있다면 이 온보딩 절차는 건너뛰고 바로 작업하라.
  단, 채워진 정보와 실제 요청이 명백히 어긋나면(예: 정보는 프로토타입인데 출시 빌드 요청)
  확인 질문을 한 번 하라.

---

지금부터 이 프로젝트에서 코드를 작성하거나 수정할 때 아래 기준을 지속적으로 따르라.
이 지침은 한 번 읽고 끝나는 것이 아니라, 모든 코드 작성/수정/리뷰 시 항상 적용해야 하는 기준이다.
기존 코드를 수정할 때는 파일 전체를 이 스타일로 갈아엎지 말고, 수정하는 부분부터 점진적으로 맞춘다.

---

## 0. 이 지침의 목적 (가장 중요)

모든 규칙의 존재 이유는 하나다:
"팀원(또는 미래의 나)이 자기 담당 부분만 수정해도 다른 부분이 깨지지 않게 하고,
수정할 위치를 코드를 다 읽지 않아도 예측할 수 있게 만드는 것."

그리고 하나 더:
"코드는 AI가 아니라 사람이 읽고, 사람이 중단점을 찍고, 사람이 따라 치며 배운다.
사람이 디버깅하기 좋은 코드가 좋은 코드다." (8번 참조)

규칙과 이 목적이 충돌하면 목적을 우선하고, 왜 규칙에서 벗어났는지 주석으로 남겨라.

---

## 1. 아키텍처 레이어 (책임 분리)

프로젝트를 다음 레이어로 나눈다. 각 레이어의 책임을 넘는 코드를 발견하면
그 자리에서 고치지 말고 먼저 지적하고, 옮길 위치를 제안하라.

| 레이어 | 책임 | 하지 말아야 할 것 |
|---|---|---|
| Managers | 초기화 순서 보장, 런/씬 전환, 외부 접근 창구 | 실제 게임 규칙 실행 |
| Models | 런타임 상태 저장, 명시적 메서드로만 상태 변경 | 계산, UI 접근, MonoBehaviour 상속 |
| Systems | 실제 게임 규칙 실행 (전투, 제작, 보상, 흐름 등) | UI 직접 조작 |
| Runtime Domains | Player / Enemy / Building 등 도메인별 컴포넌트 | 다른 도메인 내부 직접 수정 |
| UI | 표시와 입력 전달만 | 게임 규칙 계산, 상태 소유 |
| Data | JSON(또는 SO) 수치·ID 기준 | 로직 포함 |
| Editor | 개발 확인 도구, 씬 빌더, 검증 러너 | 런타임 코드에 섞이는 것 (#if UNITY_EDITOR 또는 Editor 폴더로 분리) |

핵심 원칙:
- Manager는 허브다. Manager에 로직이 쌓이기 시작하면 즉시 전용 System으로 분리를 제안하라.
- 단, System을 무한정 늘리지도 마라. "이 규칙은 누가 소유하는가"를 먼저 정하고 그 소유자에게 넣는다.
- 새 기능을 추가할 때 기존 허브 클래스에 참조를 계속 늘리지 말고, 전용 Connector/System을 먼저 검토하라.

---

## 2. 매니저 표준 구성

프로젝트의 매니저는 아래 역할 분담을 기본으로 한다.
매니저 싱글턴은 `public static Xxx Inst { get; set; }` 형태로 선언하고 Awake에서 `Inst = this;`로 등록한다.

| 매니저 | 역할 |
|---|---|
| GameManager | 게임 전체 흐름과 진행 로직의 중심. 세이브/로드 진입점. 인스턴스 데이터(Model)를 소유 |
| UIManager | UI 오픈/클로즈 핵심 로직. 신규 콘텐츠 UI는 UIManagerExtension(확장 메서드 클래스)에만 추가 |
| SoundManager | SFX(겹침 재생)/BGM(교체 재생) 분리 재생 관리 |
| GameDataManager | static 데이터를 JSON에서 불러와 Data 클래스에 매핑. Dictionary<string, TData>로 보관 |
| ResourceManager | 에셋 로딩. Addressables 비동기 로딩이 기본, 동기 처리가 꼭 필요한 경우만 Resources.Load. 로드 핸들을 캐싱하고 Release로 해제 |
| NetworkManager | 저장/로드의 실제 입출력(파일 또는 서버) 담당 |
| GameObjectManager | 씬에서 동적 생성되는 거의 모든 게임 오브젝트의 생성/제거 담당 (12번 참조) |
| GameUtil | static class. 데이터를 가공해 순수 결과 값만 반환하는 공용 static 메서드 모음. 상태를 갖지 않는다 |

- 장르에 GameManager와 독립적인 별도 룰이 있다면 BattleManager, DeckManager, TurnManager처럼
  전용 매니저를 추가한다. GameManager에 욱여넣지 마라.
- 게임 진행 중 저장되어야 하는 인스턴스 데이터는 모두 `XxxModel` 클래스로 만들어
  GameManager가 소유한다. (예: PlayerModel)
- Model이 private이므로 외부가 내부 리스트를 써야 하면 `GetPlayerItemList()` 같은
  public Get 함수를 GameManager에 제공한다.

---

## 3. 초기화 순서

Data → Model → Runtime Object → Sound/UI → GameFlow 순서를 보장하라.

이유: UI가 Model보다 먼저 초기화되면 null 또는 초기화 전 값을 읽는다.
초기화 순서가 필요한 곳은 Awake/Start에 흩어놓지 말고,
하나의 진입점(예: GameManager의 InitializeRoutine)에서 명시적 순서로 호출한다.
씬에 배치된 컴포넌트 간 연결은 전용 Connector 클래스가 담당하고,
Connector는 "무엇을 먼저 연결할지"만 지휘한다.

---

## 4. 클래스 규칙

- 클래스는 기본적으로 sealed로 선언한다.
  (예외: UIBase처럼 상속 계층이 설계상 필요한 경우 — 이때는 추상 클래스/인터페이스를 명시적으로 설계하고 이유를 주석으로 남긴다)
- 클래스 하나 = 책임 하나. 클래스가 두 가지 이상을 하고 있으면 분리를 제안하라.
- 파일 상단에 // 주석 1~3줄로 "이 클래스가 무엇을 담당하고, 무엇은 담당하지 않는지" 명시한다.
  예:
  // 3일차 보스 등장 알림과 보스 HP를 보여주는 HUD입니다.
  // 보스 체력 계산과 클리어 판정은 BossStatus와 GameFlowController가 담당합니다.
- 상속보다 컴포넌트 조합을 우선한다.
- 클래스 범위 내에서만 사용하는 메서드는 private, 다른 객체가 부르는 경우만 public.

---

## 5. 네이밍

- private 멤버 변수: _camelCase (소문자 시작)
- Unity 인스펙터에서 참조하는 객체: 대문자 시작 + 역할 접두어
  (Text_Title, Button_Close, Image_Icon, Prefab_Enemy, Root_Enemy, Canvas_PopupRoot, Layout_CharacterName)
- public 메서드: 동사형 PascalCase (TakeDamage, SetNightTime, TryCraft)
- 프로퍼티 개방: public 필드보다 `public int Xxx { get; private set; }` 프로퍼티를 우선.
  기본 private 원칙이되, 필요한 경우 public Get/Set 함수를 추가로 제공한다.
- bool 반환 시도 메서드: Try 접두사 (TryAttack, TryAddItem) — 실패해도 예외를 던지지 않고 false 반환
- 상태 확인 메서드/프로퍼티: Is/Has/Can 접두사 (IsDestroyed, HasDetectedPlayer, CanInteract)
- **Get vs Find를 구별하라**: Get은 이미 보관된 것을 꺼낼 때(실패가 이상 상황),
  Find는 탐색해서 없을 수도 있을 때(실패가 정상 흐름).
- null을 반환할 수 있는 Get 메서드는 이름에 명시한다: GetEntityObjectCanBeNull(instanceId)
- 다른 시스템에 작업을 요청하는 메서드: Request 접두사 (RequestSpawnEnemy, RequestDestroyEntityObject, RequestSaveData)
- UI 버튼 콜백 메서드: OnClick_ 접두사 (OnClick_ClosePopup, OnClick_SelectItem)
- 전역 이벤트: XxxGlobal 접미사 (CinderHeartDamagedGlobal, BossDiedGlobal)
- 상수: PascalCase (const string PlayerTag = "Player")
- 태그/씬 이름/오브젝트 이름 문자열은 반드시 const로 선언하고 하드코딩 금지
- 소스 코드의 한글 문자열은 유니코드 이스케이프(\uXXXX)가 아니라 한글 그대로 적는다.
  이스케이프는 사람이 읽을 수 없다.

---

## 6. 필드 선언 (Inspector)

- [SerializeField] private 조합을 기본으로 한다. public 필드 금지.
- [Header("그룹명")]으로 Inspector를 그룹화한다.
- [Tooltip("설명")]으로 모든 SerializeField의 역할과 단위를 명시한다.
  수치 필드는 "180이면 3분입니다"처럼 체감 단위를 함께 적는다.
- OnValidate에서 Inspector 값 범위를 Clamp하는 메서드를 두어 잘못된 값 입력을 방어한다.
- Inspector 등록이 누락될 수 있는 참조는 Awake에서 GetComponent로 자동 연결하는
  fallback 로직을 넣는다. (예: 버튼 래퍼가 자식에서 Button을 자동 탐색)

---

## 7. 프로퍼티와 조건문

- 프로퍼티는 화살표(=>) 대신 확장 표기를 사용한다:

```csharp
public float MaxHealth
{
    get
    {
        return _maxHealth;
    }
}
```

- if와 괄호 사이 공백: if (condition)
- bool 비교는 == true / == false로 명시한다. ! 연산자는 사용하지 않는다.
- 조기 반환(early return)을 적극 사용해 중첩을 2단계 이하로 유지한다.
- 가드 절(null 체크, 상태 체크)을 메서드 최상단에 모은다.

---

## 8. 사람 가독성 & 디버깅 편의 (AI 밀도 코드 금지)

AI가 만든 코드는 "돌아가는 코드"를 한 줄에 욱여넣는 경향이 있다.
그 코드는 사람이 읽기 어렵고, 중단점을 찍기 어렵고, 따라 치며 배우기 어렵다.
아래 규칙은 전부 "사람이 디버거로 한 단계씩 밟아갈 수 있는가"를 기준으로 한다.

### 8-1. 다중 조건은 눈에 보이게 나눈다

조건이 3개 이상이면 한 줄에 늘어놓지 마라. 두 가지 방법 중 하나를 쓴다.

방법 A — 연산자 기준 줄바꿈 (조건이 짧고 서로 대칭일 때):

```csharp
// 나쁨: AI 밀도 코드. 어느 조건에서 걸렸는지 한눈에 안 보인다.
return x >= -1.0f && x <= 1.0f && y >= -1.0f && y <= 1.0f;

// 좋음: 축별로 눈에 들어오고, 디버깅 시 조건 단위로 읽힌다.
return x >= -1.0f && x <= 1.0f
    && y >= -1.0f && y <= 1.0f;
```

방법 B — 이름 있는 변수/메서드로 추출 (조건에 의미가 있을 때, 더 권장):

```csharp
// 좋음: 조건에 이름이 생기고, 중단점에서 각 값이 보인다.
bool isInsideX = x >= -1.0f && x <= 1.0f;
bool isInsideY = y >= -1.0f && y <= 1.0f;
return isInsideX && isInsideY;
```

### 8-2. 같은 조건 조합이 2번 이상 반복되면 반드시 메서드로 추출한다

```csharp
// 나쁨: 이 3중 조건이 파일 안에 4번 반복된다.
if (_navMeshAgent != null && _navMeshAgent.enabled && _navMeshAgent.isOnNavMesh)

// 좋음: 이름이 곧 문서가 되고, 수정할 곳이 한 곳이 된다.
if (IsAgentOnNavMesh())

private bool IsAgentOnNavMesh()
{
    if (_navMeshAgent == null)
    {
        return false;
    }

    if (_navMeshAgent.enabled == false)
    {
        return false;
    }

    return _navMeshAgent.isOnNavMesh;
}
```

헬퍼 내부를 가드 절로 나누면 중단점에서 "어느 조건이 false였는지"를 바로 알 수 있다.
&&로 이어진 한 줄에서는 이것이 불가능하다.

### 8-3. 중단점을 찍을 수 있는 코드를 쓴다

- 한 줄 = 한 문장. 세미콜론 두 개를 한 줄에 넣지 마라.
- 조건식 안에서 부수효과(할당, 메서드 호출로 상태 변경)를 일으키지 마라.
- 메서드 체이닝을 길게 잇지 마라. 중간 결과를 이름 있는 지역 변수로 받아라.
  디버거에서 마우스만 올려도 값이 보여야 한다.

```csharp
// 나쁨: 중간 값이 어디서 잘못됐는지 볼 수 없다.
target.GetComponent<EnemyStatus>().ApplyDamage(CalculateDamage(GetWeapon().Data, GetBuffs()));

// 좋음: 각 줄에 중단점을 찍고 값을 확인할 수 있다.
WeaponData weaponData = GetWeapon().Data;
float damage = CalculateDamage(weaponData, GetBuffs());
EnemyStatus enemyStatus = target.GetComponent<EnemyStatus>();
enemyStatus.ApplyDamage(damage);
```

### 8-4. 삼항 연산자는 짧을 때만, 중첩은 금지

- 한 줄에 들어오고 양쪽 결과가 자명할 때만 삼항을 쓴다. (예: `value ? "Yes" : "No"`)
- 삼항 안에 삼항을 넣지 마라. if/else로 풀어라.

### 8-5. 줄 길이와 인자 줄바꿈

- 한 줄은 120자 이내를 기준으로 한다.
- 인자가 4개 이상인 메서드 호출/선언은 인자별로 줄바꿈한다:

```csharp
TMP_Text titleText = CreateText(
    rootObject.transform,
    "Text_Title",
    TitleText,
    18f,
    new Vector2(14f, -10f),
    new Vector2(292f, 24f));
```

### 8-6. 과잉 분해 금지 (반대 방향의 실수)

읽기 좋게 나누는 것과 잘게 부수는 것은 다르다. 아래는 하지 마라:

- 자명한 2개 조건 if를 굳이 변수로 추출 (`if (item == null || amount <= 0)`은 그대로 두는 게 낫다)
- 한 번만 쓰이고 이름이 식 자체와 동어반복인 변수 (`bool isNull = obj == null` 같은 것)
- 3줄짜리 메서드를 다시 3개의 1줄 메서드로 쪼개는 것
- 로그 메시지 문자열은 한 줄이 길어져도 허용한다 (문자열 연결을 줄바꿈하면 오히려 읽기 어렵다)

기준: "나누면 디버깅이 쉬워지는가?" 쉬워지면 나누고, 이름 붙일 의미가 없으면 그대로 둬라.

---

## 9. 이벤트 패턴 (시스템 간 연결)

시스템 간 결합은 직접 참조보다 static event를 우선한다:

```csharp
public static event Action<float> CinderHeartDamagedGlobal;

private void NotifyCinderHeartDamaged(float damage)
{
    if (CinderHeartDamagedGlobal == null)
    {
        return;
    }

    CinderHeartDamagedGlobal(damage);
}
```

- 구독은 OnEnable, 해제는 OnDisable에서 반드시 쌍으로 한다.
- 이벤트 발행은 null 체크를 포함한 전용 private Notify 메서드로 분리한다.
- 어떤 시스템 A가 다른 시스템 B의 static 메서드를 직접 호출하고 있다면,
  이벤트 구독 방식으로의 통일을 제안하라.
- 부모-자식(1:n) UI 관계에서는 자식이 `private event Action<int> OnSelectEvent;`를 갖고
  부모가 `BindSlotSelectEvent(콜백)`으로 등록하는 콜백 바인딩 패턴을 쓴다.
  자식은 OnDisable에서 이벤트를 null로 정리한다.
- 주의: 오브젝트 풀링이나 멀티씬 구조에서는 static 이벤트 구독 해제 누락이
  치명적이므로, 풀링 대상 오브젝트는 구독/해제 시점을 특히 검증하라.

---

## 10. 데이터 규칙 (static Data vs 인스턴스 Model)

데이터는 두 종류로 엄격히 구분하고, 이름으로 구별한다.

| 구분 | 명명 | 정의 | 예시 |
|---|---|---|---|
| static 데이터 | XxxData | 기획자가 미리 작성하고 게임 중 변하지 않음. 엑셀 → JSON → Data 클래스 | ItemData의 Name, Description, Grade |
| 인스턴스 데이터 | XxxModel | 게임 중 실시간으로 변하고 저장 대상 | PlayerModel의 경험치, 아이템의 강화 Level |

**AI 의무**: 사용자가 새 콘텐츠를 구현하려 할 때, 그 콘텐츠의 데이터 중 무엇이 static 데이터이고
무엇이 인스턴스 데이터인지 먼저 구별해서 설명하라.
(예: Card의 Name/Description은 static Data, Card의 강화 Level이나 현재 버프는 인스턴스 Model)

세부 규칙:
- static 데이터 스키마는 Unity 기본 자료형과 일치시킨다. List<T>는 엑셀에서 string[]/int[]
  방식으로 구성하고 컨버팅 후 Unity에서 List<T>로 받는다.
- Data 클래스는 GameDataBase(공통 Id 필드)를 상속하고 [System.Serializable]을 반드시 붙인다.
  (없으면 JsonUtility가 데이터를 무시한다)
- Model 클래스는 JsonUtility 직렬화를 위해 MonoBehaviour를 상속하지 않는다.
- JsonUtility는 루트 배열을 못 읽으므로 `{"items":[...]}` Wrapper 트릭을 쓴다.
- GameDataManager의 데이터 접근은 TryGetValue 기반 Get 메서드로 제공하고,
  null/빈 ID 가드를 최상단에 둔다.
- 밸런스 수치(체력, 피해량, 시간, 비용, 스폰 규칙)는 코드에 하드코딩하지 않는다.
- 데이터 ID와 PrefabKey는 데이터 파일과 코드에서 항상 일치해야 한다.
- 구현되지 않은 데이터 항목(executor가 처리 못 하는 타입)은 UI에 노출하지 마라.
  "데이터에는 있는데 클릭하면 아무 일도 안 일어나는" 상태가 최악이다.
- 데이터 로더가 커지면 하나의 거대 Manager가 아니라 도메인별 Catalog로 분리한다.
- 단, 프로토타이핑 초기 단계라면 데이터 드리븐을 강제하지 마라.
  루프가 재미있는지 먼저 확인하고, 굳어진 수치부터 데이터로 뺀다.

---

## 11. 리소스 로딩

- 기본은 Addressables 비동기 로딩. 프리팹/게임 오브젝트처럼 동기 처리가 꼭 필요한 경우만
  Resources.Load를 쓴다.
- 로드된 에셋의 핸들은 `Dictionary<string, AsyncOperationHandle>`로 캐싱한다.
  같은 주소를 두 번 로드하지 않고, 메모리 해제 시 Release(address)로 핸들을 해제한다.
- 로드 실패 시 LogError로 주소를 남기고, 실패한 핸들도 IsValid 확인 후 Release한다.
- 비동기 로드 완료 전까지 대상 UI 오브젝트를 잠시 비활성화하는 패턴을 권장한다
  (로드 전 깨진 이미지 노출 방지).
- 파일 쓰기(저장)는 비용이 큰 작업이다. 매 프레임 저장 금지, 명시적 저장 시점을 정하라.

---

## 12. 동적 오브젝트 관리 (GameObjectManager)

씬에서 동적 생성되는 거의 모든 게임 오브젝트는 GameObjectManager를 통해 생성/제거한다.

핵심 패턴:
- int 범위의 인스턴스 ID 발급기(`_objectInstanceKeyGenerator++`)로 생성마다 고유 키를 부여한다.
- 생성된 오브젝트는 `Dictionary<int, GameObject>` 컨테이너에 보관한다.
- Dictionary에 추가하기 전에 ContainsKey로 중복 키를 검사한다.
- 생성 직후 해당 엔티티 컴포넌트에 인스턴스 ID를 주입한다(InitEnemyInfo(generatedId)).
  이후 그 객체가 자신의 정보를 수정할 때는 GameObjectManager를 통한다.
- 조회는 GetEntityObjectCanBeNull(instanceId) — 없으면 LogWarning 후 null 반환.
- 제거는 RequestDestroyEntityObject(instanceId) — 컨테이너에서 Remove 후 Destroy.
- 프리팹 미등록, 생성 실패 등 각 단계마다 가드 절 + LogWarning을 넣는다.
- 저장용 고유값이 필요하면 GameUtil의 GenerateUniqueId()(스레드 안전한 long ID)를 쓴다.
  게임오브젝트 인스턴스 ID(int, 휘발성)와 저장용 고유 ID(long, 영속)를 혼동하지 마라.

---

## 13. UI 구조

- 모든 UI는 UIBase를 상속받는다. UGUI 기반.
- UI의 종류는 UIType enum, 배치 계층은 UIRootType enum으로 관리한다.
  Canvas 루트는 Background / Main / Content / Popup / VeryFront 5계층.
- UIManager는 두 컨테이너를 구분해서 관리한다:
  - `Dictionary<UIType, UIBase> _createdUIDic` — 생성/제거 (Instancing, GC 연관)
  - `HashSet<UIType> _openedUIDic` — 활성/비활성 (SetActive)
- UI 프리팹 경로는 `Prefabs/UI/{UIRootType}/{UIType}` 규칙으로 통일한다.
  (폴더명과 프리팹 이름이 enum 이름과 동일해야 한다)
- 신규 UI 추가 절차: ① UIType enum 추가 → ② 경로 규칙에 맞게 프리팹 배치 →
  ③ UIManagerExtension에 OpenXxxPopup(파라미터) 확장 메서드 추가 →
  ④ `uiBase is XxxPopup popup` 패턴 매칭으로 캐스팅 후 SetUI/Refresh 호출.
  UIManager 본체는 수정하지 않는다.
- 버튼은 UIButton 래퍼 클래스로 감싼다:
  - 이벤트는 Unity 인스펙터 등록이 아니라 코드에서
    `BindOnClickButtonEvent(Action onClickCallback)`으로 연결한다.
  - OnDisable에서 RemoveAllListeners로 정리한다.
  - 인스펙터 등록 누락 시 GetComponentInChildren로 자동 탐색하는 fallback을 넣는다.
- 부모-자식 슬롯 구조(인벤토리 등):
  - 부모가 `Dictionary<int, SlotUI>`로 자식 슬롯을 보관하고 키를 발급한다.
  - 자식 슬롯은 InitSlot(instanceId, dataId, count)으로 초기화한다.
  - 선택 이벤트는 자식 → 부모 콜백 바인딩(BindSlotSelectEvent)으로 전달한다.
  - 오픈 시점에 기존 슬롯을 정리(Clear + Destroy)하고 다시 생성한다.

---

## 14. 성능 기본기

- Update에서 FindObjectsByType, GetComponent, 문자열 연결, 할당(new)을 반복하지 마라.
  탐색이 필요하면 캐싱하거나 최소 0.3~0.5초 인터벌로 제한한다.
- Physics.OverlapSphere 대신 NonAlloc 버전 + 재사용 배열을 사용한다.
- AI 판단 로직은 매 프레임이 아니라 DecisionInterval(예: 0.2초) 주기로 돌린다.
  이동/공격 실행은 매 프레임, "판단"만 주기적으로.
- 태그 비교는 CompareTag를 사용한다 (== 문자열 비교 금지).
- 컴포넌트 참조는 Awake에서 한 번 캐싱한다.

---

## 15. 로그와 방어 코드

- 프로젝트 전용 로그 래퍼(예: ProjectLog.Verbose)를 두고 Debug.Log 직접 호출을 지양한다.
  릴리즈 빌드에서 로그를 일괄 차단할 수 있어야 한다.
- 경고는 Debug.LogWarning("[클래스명] 무엇이 왜 잘못됐는지 + 관련 오브젝트명") 형태로.
- null이 가능한 참조는 가드 절로 방어하되, "조용히 실패"하면 안 되는 곳은
  LogWarning으로 원인을 남긴다.
- Inspector 참조가 끊겨도 최소한 동작하도록 fallback(GetComponent 자동 연결,
  EnsureSceneXxx 패턴)을 두되, fallback으로 동작 중임을 로그로 알린다.
- 코드가 맞아도 Prefab/Inspector 연결이 끊기면 동작하지 않는다.
  참조 연결을 검증하는 Editor 체크 도구를 만드는 것을 권장한다.

---

## 16. Git / 협업 (팀 프로젝트인 경우)

- main은 항상 플레이 가능한 상태를 유지한다. 이것이 머지 기준이다.
- 팀원 브랜치는 통짜 머지보다 작동하는 변경만 선별 반영하고,
  커밋 메시지에 출처 브랜치/커밋을 남긴다.
- 개인 ProjectSettings, 오래된 Scene, 대용량 테스트 에셋은 머지에서 제외한다.
- Unity의 prefab/meta/GUID 충돌은 텍스트 충돌보다 위험하다.
  같은 씬/프리팹을 두 명이 동시에 만지지 않도록 조율하라.
- 커밋은 작게, 하나의 커밋 = 하나의 의도. 버전 태그([5.64] 같은)를 쓰면 이력 추적이 쉽다.

---

## 17. 유연성 조항 (규칙보다 판단)

다음 상황에서는 규칙을 완화하고, 대신 이유를 주석이나 커밋 메시지로 남겨라:

1. 프로토타입/게임잼: 데이터 드리븐, 레이어 분리를 느슨하게. 속도 우선.
2. 상속이 자연스러운 도메인(스킬 트리, 캐릭터 클래스, UIBase 등): sealed 강제하지 말 것.
3. 서드파티/에셋스토어 코드: 이 스타일로 갈아엎지 말고 격리(전용 폴더, 어댑터)하라.
4. 성능 크리티컬 구간: 가독성 규칙보다 프로파일링 결과를 우선하라.
5. 기존 프로젝트에 도입할 때: 새로 작성하는 코드부터 적용하고,
   기존 코드는 수정할 일이 생길 때 그 파일만 맞춘다. 일괄 리포맷 커밋은 diff를 오염시킨다.

단, 8번(사람 가독성 & 디버깅 편의)은 프로토타입 단계에서도 완화하지 마라.
빨리 짠 코드일수록 빨리 디버깅하게 되고, 그때 밀도 코드가 발목을 잡는다.

---

## 18. 너(AI)의 행동 규칙

- 코드를 작성/수정할 때마다 이 지침과의 일치 여부를 스스로 검토하라.
  특히 8번: 네가 방금 쓴 코드에 3개 이상 && 체인, 긴 메서드 체이닝,
  140자 넘는 줄이 있으면 제출 전에 스스로 고쳐라.
- 사용자가 새 콘텐츠를 구현하려 하면, 그 콘텐츠의 static 데이터(Data)와
  인스턴스 데이터(Model)를 먼저 구별해서 설명하라. (10번 참조)
- 내가 요청한 기능이 이 아키텍처 원칙과 충돌하면, 구현 전에 충돌 지점과 대안을 먼저 말하라.
- 기존 코드에서 이 지침 위반을 발견하면: 내 요청 범위 안이면 고치고,
  범위 밖이면 고치지 말고 위치와 함께 보고만 하라.
- 리팩토링을 제안할 때는 "무엇이 나쁜가"가 아니라
  "이대로 두면 어떤 수정 상황에서 무엇이 깨지는가"로 설명하라.
- 이 지침을 요약해달라고 하면 규칙 목록이 아니라 0번(목적)부터 설명하라.
