# 출처 및 라이선스 표기 (Attribution)

이 저장소는 아래 오픈소스 프로젝트의 자료를 포함한다.
서드파티 자료의 라이선스 원문은 `licenses/` 폴더에 있다.

| 저장소 내 위치 | 출처 | 라이선스 |
|---|---|---|
| `skills/unity-*`, `unreal-*`, `godot-*`, phaser/pixijs/threejs/bevy/love2d/pygame/roblox-*, 분야(disciplines)·장르(genres)·워크플로우 스킬, `skills/gamedev-router` | [gamedev-skills/awesome-gamedev-agent-skills](https://github.com/gamedev-skills/awesome-gamedev-agent-skills) — Copyright 2026 Abhishek Barali and contributors | Apache-2.0 ([원문](licenses/awesome-gamedev-agent-skills-LICENSE.txt), [NOTICE](licenses/awesome-gamedev-agent-skills-NOTICE.txt)) |
| `skills/ccgs-*` (제작 프로세스 스킬 73종), `agents/` (직군별 에이전트 49종) | [Donchitos/Claude-Code-Game-Studios](https://github.com/Donchitos/Claude-Code-Game-Studios) — Copyright (c) 2026 Donchitos | MIT ([원문](licenses/Claude-Code-Game-Studios-LICENSE.txt)) |
| `Karpathy MD/` 원본 3종, 루트 CLAUDE.md/AGENTS.md의 카파시 4원칙 본문, `.cursor/rules/karpathy-guidelines.mdc` | [forrestchang/andrej-karpathy-skills](https://github.com/forrestchang/andrej-karpathy-skills) — Andrej Karpathy의 관찰을 Forrest Chang이 정리 | 리포 루트에 LICENSE 파일 없음. `skills/karpathy-guidelines/SKILL.md` frontmatter에 MIT 선언 (`license: MIT`) |

## 변경 사항 고지 (Apache-2.0 요구)

awesome-gamedev-agent-skills에서 가져온 스킬은 엔진별 폴더 구조(unity/, unreal/, ...)를
평면 구조로 재배치했다. 라우터는 폴더명과 맞도록 `name`을 `gamedev-router`로 조정했다.
Claude-Code-Game-Studios의 스킬은 이름 충돌 방지를 위해 폴더명, frontmatter `name`,
내부 Skill 호출에 `ccgs-` 접두사를 붙였다. 그 외 본문은 원문을 유지한다.

## 이 저장소의 원저작물

루트 지침 파일(GAMEDEV.md 등), `GameDev/`, `catalog/`, `my_skills/`, `my_agents/`, `scripts/`는
이 저장소의 원저작물이며 루트 LICENSE(MIT)를 따른다.
