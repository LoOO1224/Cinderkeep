// 에디터에서 씬 세팅, Check 리포트, 팀 작업 인수인계를 빠르게 처리하는 개발 도구입니다.
// 런타임 빌드에는 포함되지 않으며, 반복되는 수동 연결과 Check 작업을 줄이는 데 사용합니다.
// Unity 배치모드에서 NavMesh Bake 도구를 호출하기 위한 얇은 진입점입니다.
// 실제 Bake 로직은 CinderkeepNavMeshBakeTool이 담당합니다.
public static class CinderkeepNavMeshBakeCommand
{
    public static void Bake()
    {
        Cinderkeep.EditorTools.CinderkeepNavMeshBakeTool.BakeMapNavMeshesFromCommandLine();
    }
}
