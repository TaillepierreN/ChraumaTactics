using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

public class AutoRelease : MonoBehaviour
{
    private ObjectPool<GameObject> _pool;
    private float _t;
    private bool _armed;
    private bool _releasing;
    private Coroutine _co;

    private void OnDisable()
    {
        if (_armed && !_releasing)
            Release();
    }

    /// <summary>
    /// set impact to be disabled after a time
    /// </summary>
    /// <param name="pool"></param>
    /// <param name="seconds"></param>
    public void Arm(ObjectPool<GameObject> pool, float seconds)
    {
        _pool = pool;
        _t = seconds;
        _armed = true;
        _releasing = false;

        if (_co != null)
            StopCoroutine(_co);

        _co = StartCoroutine(WaitAndRelease());
    }

    /// <summary>
    /// Wait a bit and release the impact
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitAndRelease()
    {
        yield return new WaitForSeconds(_t);
        Release();
    }

    /// <summary>
    /// release the impact into the pool
    /// </summary>
    private void Release()
    {
        if (_releasing)
            return;

        _releasing = true;
        _armed = false;

        if (_co != null)
        {
            StopCoroutine(_co);
            _co = null;
        }

        var pool = _pool;
        _pool = null;

        if (pool != null)
            pool.Release(gameObject);
    }
}
