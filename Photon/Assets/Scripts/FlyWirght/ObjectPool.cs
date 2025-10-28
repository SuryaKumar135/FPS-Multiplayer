using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ObjectPool : MonoBehaviourPunCallbacks
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size = 10;
        public bool isNetworked;
    }

    public static ObjectPool Instance;

    [SerializeField] private List<Pool> pools = new();
    private Dictionary<string, Queue<GameObject>> poolDictionary = new();

    private bool localPoolsInitialized = false;
    private bool networkPoolsInitialized = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    #region === Initialization ===

    public void InitializeLocalPools()
    {
        if (localPoolsInitialized)
            return;

        foreach (Pool pool in pools)
        {
            if (pool.isNetworked)
                continue; // skip networked ones here

            CreatePool(pool, false);
        }

        localPoolsInitialized = true;
        Debug.Log("✅ Local object pools initialized");
    }

    public void InitializeNetworkPools()
    {
        if (networkPoolsInitialized)
            return;

        foreach (Pool pool in pools)
        {
            if (!pool.isNetworked)
                continue; // skip local ones

            CreatePool(pool, true);
        }

        networkPoolsInitialized = true;
        Debug.Log(" Network object pools initialized");
    }

    private void CreatePool(Pool pool, bool isNetworked)
    {
        if (poolDictionary.ContainsKey(pool.tag))
        {
            Debug.LogWarning($"Pool with tag '{pool.tag}' already exists. Skipping.");
            return;
        }

        Queue<GameObject> objectPool = new();

        for (int i = 0; i < pool.size; i++)
        {
            GameObject obj;

            if (isNetworked)
                obj = PhotonNetwork.Instantiate(pool.prefab.name, Vector3.zero, Quaternion.identity);
            else
                obj = Instantiate(pool.prefab);

            obj.SetActive(false);
            obj.transform.SetParent(transform);
            objectPool.Enqueue(obj);
        }

        poolDictionary.Add(pool.tag, objectPool);
    }

    #endregion

    #region === Photon Callbacks ===

    public override void OnJoinedRoom()
    {
        InitializeNetworkPools(); // initialize networked pools when we join
    }

    #endregion

    #region === Pool Usage ===

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"⚠️ Pool with tag '{tag}' doesn't exist.");
            return null;
        }

        Queue<GameObject> objectQueue = poolDictionary[tag];
        GameObject objectToSpawn = null;

        // Find an inactive object to reuse
        foreach (var obj in objectQueue)
        {
            if (!obj.activeSelf)
            {
                objectToSpawn = obj;
                break;
            }
        }

        // If all are active, reuse the first in queue (not ideal but prevents null)
        if (objectToSpawn == null)
            objectToSpawn = objectQueue.Peek();

        objectToSpawn.transform.SetPositionAndRotation(position, rotation);
        objectToSpawn.SetActive(true);

        return objectToSpawn;
    }

    public void ReturnToPool(GameObject obj)
    {
        if (obj == null)
            return;

        obj.SetActive(false);
        obj.transform.SetParent(transform);
    }

    #endregion
}
