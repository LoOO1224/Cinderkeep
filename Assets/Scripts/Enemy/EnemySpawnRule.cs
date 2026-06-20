using UnityEngine;

public enum EnemySpawnMode
{
    Day,
    Night,
    Boss
}

// 낮, 밤, 보스 접근 단계별 스폰 숫자를 담는 설정 클래스입니다.
// 팀원이 이 값만 조절해서 스폰 주기와 한 번에 나오는 수를 빠르게 바꿀 수 있습니다.
[System.Serializable]
public sealed class EnemySpawnRule
{
    [Tooltip("다음 웨이브가 나오기까지 기다리는 시간입니다.")]
    [SerializeField] private float _spawnInterval = 45f;
    [Tooltip("한 번에 생성되는 적 수입니다.")]
    [SerializeField] private int _spawnCountPerWave = 1;
    [Tooltip("이 스폰 지점이 유지할 수 있는 최대 생존 적 수입니다.")]
    [SerializeField] private int _maxAliveEnemyCount = 3;
    [Tooltip("낮, 밤, 보스 단계로 전환되는 즉시 한 번 스폰할지 결정합니다.")]
    [SerializeField] private bool _spawnOnModeStart = true;

    public EnemySpawnRule()
    {
    }

    public EnemySpawnRule(float spawnInterval, int spawnCountPerWave, int maxAliveEnemyCount, bool spawnOnModeStart)
    {
        _spawnInterval = spawnInterval;
        _spawnCountPerWave = spawnCountPerWave;
        _maxAliveEnemyCount = maxAliveEnemyCount;
        _spawnOnModeStart = spawnOnModeStart;
        ClampValues();
    }

    public float SpawnInterval
    {
        get
        {
            return _spawnInterval;
        }
    }

    public int SpawnCountPerWave
    {
        get
        {
            return _spawnCountPerWave;
        }
    }

    public int MaxAliveEnemyCount
    {
        get
        {
            return _maxAliveEnemyCount;
        }
    }

    public bool SpawnOnModeStart
    {
        get
        {
            return _spawnOnModeStart;
        }
    }

    public void ClampValues()
    {
        if (_spawnInterval < 1f)
        {
            _spawnInterval = 1f;
        }

        if (_spawnCountPerWave < 0)
        {
            _spawnCountPerWave = 0;
        }

        if (_maxAliveEnemyCount < 0)
        {
            _maxAliveEnemyCount = 0;
        }
    }
}
