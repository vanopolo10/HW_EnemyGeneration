using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class SpawnSystem : MonoBehaviour
{
    [SerializeField] private float _spawnRate = 2f;
    [SerializeField] private Enemy _enemyPrefab;
    [SerializeField] private Hero _hero;
    [SerializeField] private bool _autoExpand = false;
    [SerializeField] private int _poolSize = 10;
    [SerializeField] private List<MeshRenderer> _spawnersMeshes;

    private ObjectPool<Enemy> _enemyPool;
    private int _spawnerMeshCount;

    private void Awake()
    {
        _enemyPool = new ObjectPool<Enemy>(
            OnCreatePooledItem,
            OnGetFromPool,
            OnReleaseToPool,
            OnDestroyPoolObject,
            _autoExpand,
            _poolSize
        );

        _spawnerMeshCount = _spawnersMeshes.Count;
    }

    private void Start()
    {
        StartCoroutine(Spawn(_spawnRate));
    }

    private Enemy OnCreatePooledItem()
    {
        Enemy enemy = Instantiate(_enemyPrefab);
        enemy.TouchedHero += OnEnemyTouchedHero;
        return enemy;
    }

    private void OnEnemyTouchedHero(Enemy enemy)
    {
        if (_enemyPool.CountActive > 0)
        {
            _enemyPool.Release(enemy);
        }
        else
        {
            Debug.LogWarning("Попытка возврата объекта в пул, но активные объекты отсутствуют!");
        }
    }

    private void OnGetFromPool(Enemy enemy)
    {
        enemy.gameObject.SetActive(true);
    }

    private void OnReleaseToPool(Enemy enemy)
    {
        enemy.gameObject.SetActive(false);
    }

    private void OnDestroyPoolObject(Enemy enemy)
    {
        enemy.TouchedHero -= OnEnemyTouchedHero;
        Destroy(enemy.gameObject);
    }

    private IEnumerator Spawn(float time)
    {
        WaitForSeconds spawnDelay = new WaitForSeconds(time);
        int spawnHeightCoefficient = 2;

        while (true)
        {
            if (_enemyPool.CountActive < _poolSize || _autoExpand)
            {
                Enemy enemy = _enemyPool.Get();

                Vector3 spawnPosition = GetHighestSurfaceCenter() + new Vector3(0, enemy.transform.localScale.y / spawnHeightCoefficient, 0);
                enemy.transform.position = spawnPosition;

                Vector3 heroPosition = _hero.transform.position;
                Vector3 enemyPosition = enemy.transform.position;

                Vector3 direction = (heroPosition - enemyPosition).normalized;
                enemy.StartMoving(direction);

                yield return spawnDelay;
            }
            else
            {
                yield return null;
            }
        }
    }

    private MeshRenderer GetRandomSpawnerMesh()
    {
        return _spawnersMeshes[Random.Range(0, _spawnerMeshCount)];
    }

    private Vector3 GetHighestSurfaceCenter()
    {
        MeshRenderer meshRenderer = GetRandomSpawnerMesh();

        Bounds bounds = meshRenderer.bounds;
        return new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
    }
}
