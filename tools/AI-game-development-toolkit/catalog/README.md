# 게임 개발 AI 자료 분류

원본 `skills/`와 `agents/`는 자동 탐색 호환성을 위해 이동하지 않는다. 이 카탈로그는 Unity와 Unreal 중심으로 설치 대상을 고르기 위한 분류표다.

## 요약

| 종류 | 공용 | Unity | Unreal | 미사용 엔진 | 합계 |
|---|---:|---:|---:|---:|---:|
| 스킬 (`SKILL.md`) | 100 | 8 | 6 | 26 | 140 |
| 에이전트 | 34 | 5 | 5 | 5 | 49 |

분류 원칙은 다음과 같다.

- [Unity](unity.md)와 [Unreal](unreal.md)에 명시된 항목은 해당 엔진 전용이다.
- [미사용 엔진](미사용-엔진.md)은 현재 설치 대상에서 제외한다.
- 위 목록에 없는 원본 스킬과 에이전트는 모두 [공용](공용.md)이다.
- `references/` 문서는 부모 스킬과 같은 분류를 따른다.
- `skills/README.md`는 설명 문서이므로 140개 스킬 수에 포함하지 않는다.

## 개인 규칙

- `my_skills/personal-code-style`: 공용 코드 일관성 및 디버깅 가독성
- `my_skills/unity-project-conventions`: Unity C#·수명·직렬화·UI 규칙
- `my_skills/unreal-project-conventions`: Unreal C++·UObject·Blueprint·복제 규칙
- `my_agents/code-consistency-reviewer.md`: 위 규칙을 이용한 변경 범위 검토

`_archive/`는 출처 보존용이다. 개인 규칙은 보관본을 그대로 복제하지 않고 현재 Unity와 Unreal 관례에 맞게 정제했다.
