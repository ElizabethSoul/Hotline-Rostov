using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
	[Header("Настройки спавна")]
	[Tooltip("Трансформ, в котором будут появляться враги. Если не задан, используется позиция этого объекта.")]
	public Transform spawnPoint;

	[Tooltip("Массив префабов врагов; один будет выбран случайно для спавна.")]
	public GameObject[] enemyPrefabs;

	[Header("Тайминги")]
	[Tooltip("Задержка между спавном отдельных врагов внутри одной волны (в секундах).")]
	public float timeBetweenSpawns = 0.5f;

	[Tooltip("Задержка между волнами после очистки локации (в секундах).")]
	public float timeBetweenWaves = 3f;

	[Header("Параметры волн")]
	[Tooltip("Количество врагов в первой волне.")]
	public int startingEnemiesPerWave = 3;

	[Tooltip("Прирост количества врагов к каждой следующей волне.")]
	public int enemiesPerWaveIncrement = 1;

	[Tooltip("Всего волн. Установите 0 или меньше для бесконечных волн.")]
	public int totalWaves = 5;

	[Header("Поиск существующих врагов")]
	[Tooltip("Тег, по которому определяется наличие врагов на сцене. У префабов врагов должен быть этот тег.")]
	public string enemyTag = "Enemy";

	[Header("Смещение спавна (опционально)")]
	[Tooltip("Радиус случайного смещения позиции спавна вокруг `spawnPoint`.")]
	public float spawnRadius = 0f;

	private int currentWave = 0;
	private int enemiesThisWave;

	private void Start()
	{
		enemiesThisWave = Mathf.Max(1, startingEnemiesPerWave);
		StartCoroutine(WaveManager());
	}

	private IEnumerator WaveManager()
	{
		bool firstWave = true;

		while (totalWaves <= 0 || currentWave < totalWaves)
		{
			// Wait until there are no enemies in the scene
			yield return new WaitUntil(() => CountEnemiesWithTag() == 0);

			if (!firstWave)
			{
				yield return new WaitForSeconds(timeBetweenWaves);
			}

			firstWave = false;
			currentWave++;

			// Спавним врагов по одному с задержкой между ними
			for (int i = 0; i < enemiesThisWave; i++)
			{
				SpawnRandomEnemy();
				if (timeBetweenSpawns > 0f)
					yield return new WaitForSeconds(timeBetweenSpawns);
				else
					yield return null; // хотя бы один кадр, чтобы не заблокировать
			}

			enemiesThisWave += enemiesPerWaveIncrement;
		}
	}

	private void SpawnRandomEnemy()
	{
		if (enemyPrefabs == null || enemyPrefabs.Length == 0)
		{
			Debug.LogWarning("Спавнер: не назначены префабы врагов в инспекторе.");
			return;
		}

		Transform target = spawnPoint != null ? spawnPoint : transform;
		Vector3 pos = target.position;

		if (spawnRadius > 0f)
		{
			pos += (Vector3)(Random.insideUnitCircle) * spawnRadius;
		}

		GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
		Instantiate(prefab, pos, target.rotation);
	}

	private int CountEnemiesWithTag()
	{
		if (string.IsNullOrEmpty(enemyTag)) return 0;
		return GameObject.FindGameObjectsWithTag(enemyTag).Length;
	}

	private void OnDrawGizmosSelected()
	{
		Transform t = spawnPoint != null ? spawnPoint : transform;
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(t.position, 0.2f);
		if (spawnRadius > 0f)
		{
			Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
			Gizmos.DrawSphere(t.position, spawnRadius);
		}
	}
}
