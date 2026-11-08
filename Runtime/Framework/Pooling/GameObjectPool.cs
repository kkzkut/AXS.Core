using System.Collections.Generic;
using UnityEngine;

namespace AXS.Core.Pooling
{
    /// <summary>
    /// 汎用GameObjectプール。エフェクト・弾・UI等の頻繁な生成破棄によるGCアロケーションを防ぎます。
    /// </summary>
    public class GameObjectPool : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int initialSize = 10;
        [SerializeField] private int maxSize = 50;

        private readonly Queue<GameObject> _pool = new Queue<GameObject>();

        private void Awake()
        {
            if (prefab != null)
            {
                Initialize(prefab, initialSize, maxSize);
            }
        }

        public void Initialize(GameObject targetPrefab, int initial = 10, int max = 50)
        {
            prefab = targetPrefab;
            initialSize = initial;
            maxSize = max;

            for (int i = 0; i < initialSize; i++)
            {
                var instance = CreateNewInstance();
                instance.SetActive(false);
                _pool.Enqueue(instance);
            }
        }

        private GameObject CreateNewInstance()
        {
            var instance = Instantiate(prefab, transform);
            return instance;
        }

        /// <summary>
        /// プールからオブジェクトを取り出します。
        /// </summary>
        public GameObject Rent(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            GameObject instance;
            if (_pool.Count > 0)
            {
                instance = _pool.Dequeue();
            }
            else
            {
                instance = CreateNewInstance();
            }

            instance.transform.SetParent(parent);
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.SetActive(true);

            return instance;
        }

        /// <summary>
        /// オブジェクトをプールに返却します。
        /// </summary>
        public void Return(GameObject instance)
        {
            if (instance == null) return;

            if (_pool.Count >= maxSize)
            {
                Destroy(instance);
                return;
            }

            instance.SetActive(false);
            instance.transform.SetParent(transform);
            _pool.Enqueue(instance);
        }

        public void Clear()
        {
            while (_pool.Count > 0)
            {
                var item = _pool.Dequeue();
                if (item != null)
                {
                    Destroy(item);
                }
            }
        }
    }
}
