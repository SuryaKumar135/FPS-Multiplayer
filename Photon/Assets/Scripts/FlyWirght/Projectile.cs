using UnityEngine;
using System.Collections;
public class Projectile : MonoBehaviour
{
    [SerializeField] private float lifeTime = 3f;
    private float timer;

    private void OnEnable() => StartCoroutine(nameof(PoolBack));

    IEnumerator PoolBack()
    {
        yield return new WaitForSeconds(lifeTime);
        ObjectPool.Instance.ReturnToPool(gameObject);
    }
}