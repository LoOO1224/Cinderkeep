# AI Game Development Toolkit

AI 코딩 도구(Claude Code, Codex, Cursor)가 게임 개발 작업을 더 정확하게 수행하도록 만드는
지침·스킬·에이전트 모음. 개인용으로 관리하며, 새 프로젝트를 시작할 때 필요한 파일을 복사해 쓴다.

> 이 저장소는 딥러닝 학습 프로젝트가 아니다. 프롬프트/컨텍스트 엔지니어링과
> 스킬 기반 작업 라우팅으로 AI 코딩 에이전트의 행동을 개선하는 자료 모음이다.

## 핵심 구조

```
AI-game-development-toolkit/
├─ CLAUDE.md          # 카파시 4원칙 + 확장 호출 규칙 (Claude Code용, 어느 프로젝트에나 사용)
├─ AGENTS.md          # 위와 동일 (Codex용)
├─ GAMEDEV.md         # 게임 개발 확장 지침 — 자립형 단일 파일 (엔진 판별 + 스킬 라우팅)
├─ .cursor/rules/     # Cursor용 카파시 규칙 (.mdc)
├─ GameDev/           # 게임 지침 원본 모듈 (UNITY.md, UNREAL.md, SKILL_GUIDE.md)
├─ skills/            # 게임 개발 스킬 140종 (SKILL.md 표준, 평면 구조)
├─ agents/            # 게임 직군별 서브에이전트 49종
├─ my_skills/         # 개인 제작 스킬 (코딩 스타일, Unity/Unreal 컨벤션)
├─ my_agents/         # 개인 제작 에이전트
├─ catalog/           # 스킬/지침 분류 자료
├─ Karpathy MD/       # 카파시 원본 보관 (수정 금지)
├─ licenses/          # 서드파티 라이선스 원문
└─ _archive/          # 이전 버전 보관
```

## 작동 원리 (두 가지 메커니즘)

1. **지침(md)**: `CLAUDE.md`/`AGENTS.md`는 도구가 세션 시작 시 항상 읽는다.
   맨 아래 "확장 호출 규칙"이 루트에 `GAMEDEV.md`가 있으면 읽고, 없으면 무시한다 —
   그래서 게임이 아닌 프로젝트에 붙여도 안전하다.
2. **스킬(SKILL.md)**: 지침에 import되지 않는다. 정해진 폴더에 넣으면 도구가 자동 발견하고,
   작업이 스킬 설명과 맞을 때만 전체를 로드한다(on-demand).

## 새 프로젝트에 적용하기

### 1) 일반 프로젝트 (게임 아님)

```
프로젝트루트/
├─ CLAUDE.md     ← 이 저장소의 CLAUDE.md 복사
└─ AGENTS.md     ← 이 저장소의 AGENTS.md 복사
```

### 2) 게임 프로젝트

```
프로젝트루트/
├─ CLAUDE.md, AGENTS.md      ← 위와 동일
├─ GAMEDEV.md                ← 추가 (엔진 자동 판별: Unity/Unreal/Godot/웹)
├─ .claude/skills/           ← skills/ 에서 필요한 것 복사 (Claude Code)
├─ .codex/skills/            ← 동일 내용 복사 (Codex)
└─ .claude/agents/           ← agents/ 에서 필요한 것 복사 (선택)
```

스킬은 전부 복사해도 되고 엔진/장르에 맞는 것만 골라도 된다.
접두사로 구분: `unity-` `unreal-` `godot-` `ccgs-`(제작 프로세스) 등.

### 3) Cursor 사용 시

`.cursor/` 폴더를 프로젝트 루트에 복사한다. (Cursor는 CLAUDE.md를 읽지 않는다)

### 4) 전역 설치 (모든 프로젝트 기본 적용)

- Claude Code: `~/.claude/CLAUDE.md`, 전역 스킬은 `~/.claude/skills/`
- Codex: `~/.codex/AGENTS.md`, 전역 스킬은 `~/.codex/skills/`

## 유지보수 규칙

- **원본 기준**: 카파시 원문이 의심되면 `Karpathy MD/KARPATHY_original.md`
  (또는 로컬 포크 `C:\GitHub\andrej-karpathy-skills`)가 기준이다.
- **GAMEDEV.md ↔ GameDev/**: `GameDev/` 폴더가 원본 모듈이고 `GAMEDEV.md`는
  배포용 통합본이다. 한쪽을 수정하면 반드시 다른 쪽에도 반영한다.
- **업스트림 동기화**: `scripts/sync-upstream.ps1` 실행 → 로컬 포크 3개를 pull 하고
  skills/agents/카파시 사본을 갱신한 뒤 변경 목록을 보여준다.
- **자동 검증**: `python scripts/validate-toolkit.py`로 Skill 이름·링크·UTF-8·카탈로그
  개수를 검사한다. 같은 검사는 GitHub Actions에서도 실행된다.
- **새 스킬 추가 전**: `GameDev/SKILL_GUIDE.md`의 검증·보안 체크리스트를 통과시킨다.
- **한글 인코딩**: 파일은 전부 정상 UTF-8이다. PowerShell 출력이 깨져 보여도 파일을
  변환하지 마라. 확인은 `Get-Content -Encoding UTF8 <파일>`.

## 출처

서드파티 자료의 출처와 라이선스는 [ATTRIBUTION.md](ATTRIBUTION.md) 참고.
