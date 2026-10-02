using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameplayKit.AI
{
    /// <summary>Genera enemigos (u otro prefab) cada cierto tiempo hasta un máximo vivos a la vez. Si los generados
    /// tienen AIBrain se les asigna el objetivo (por defecto, el objeto con tag Player).</summary>
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [Tooltip("Puntos de aparición (se alternan). Vacío = la posición del spawner.")]
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private float interval = 3f;
        [SerializeField] private int maxAlive = 3;
        [Tooltip("Total a generar en toda la partida. 0 = sin límite.")]
        [SerializeField] private int totalToSpawn = 0;
        [SerializeField] private bool spawnOnStart = true;
        [Tooltip("Objetivo para los AIBrain generados. Vacío = el objeto con tag Player.")]
        [SerializeField] private Transform target;

        public int AliveCount { get { Prune(); return _alive.Count; } }
        public int SpawnedCount { get; private set; }
        public event Action<GameObject> OnSpawned;

        private readonly List<GameObject> _alive = new List<GameObject>();
        private float _timer;
        private int _nextPoint;

        private void Start()
        {
            _timer = spawnOnStart ? 0f : interval;
        }

        private void Update()
        {
            if (prefab == null) return;
            if (totalToSpawn > 0 && SpawnedCount >= totalToSpawn) return;

            _timer -= Time.deltaTime;
            if (_timer > 0f) return;
            _timer = interval;
            if (AliveCount < maxAlive) Spawn();
        }

        public GameObject Spawn()
        {
            if (prefab == null) return null;
            Transform point = spawnPoints != null && spawnPoints.Length > 0 ? spawnPoints[_nextPoint++ % spawnPoints.Length] : transform;
            var instance = Instantiate(prefab, point != null ? point.position : transform.position, Quaternion.identity);
            _alive.Add(instance);
            SpawnedCount++;

            var brain = instance.GetComponent<AIBrain>();
            if (brain != null && brain.Target == null)
            {
                Transform t = target;
                if (t == null) { var player = GameObject.FindWithTag("Player"); if (player != null) t = player.transform; }
                brain.Target = t;
            }

            OnSpawned?.Invoke(instance);
            return instance;
        }

        // Un enemigo cuenta como vivo mientras exista y esté activo (los muertos se destruyen o desactivan).
        private void Prune() => _alive.RemoveAll(go => go == null || !go.activeInHierarchy);
    }
}
