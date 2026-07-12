// 씬에 배치되지 않은 HUD를 코드로 즉석 생성할지 결정하는 전역 정책입니다.
// 기본값 false: 씬(또는 씬 빌더)에 배치된 HUD만 사용하고, 코드가 임의로 HUD를 만들지 않습니다.
// 프로토타이핑처럼 임시 HUD가 급하게 필요할 때만 true로 바꿔 사용합니다.
public static class HudRuntimeCreationPolicy
{
    public static bool IsRuntimeCreationEnabled { get; set; } = false;
}
