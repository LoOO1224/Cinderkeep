// 런타임 시스템이 공유하는 최소 계약입니다.
// 인터페이스는 작게 유지해 구현체 책임이 섞이지 않게 합니다.
namespace Cinderkeep.Gameplay
{
    // 모든 매니저가 같은 이름의 초기화 함수를 갖도록 맞추는 약속입니다.
    // GameManager는 이 인터페이스만 보고 정해진 순서대로 Initialize를 호출합니다.
    public interface IGameInitializable
    {
        bool IsInitialized { get; }

        void Initialize();
    }
}
