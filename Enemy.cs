using System;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float _speed = 5f;

    public event Action<Enemy> TouchedHero;

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Hero>(out Hero hero))
        {
            TouchedHero?.Invoke(this);
        }
    }
    
    public void StartMoving(Vector3 direction)
    {
        StopAllCoroutines();
        StartCoroutine(MoveToPosition(direction));
    }

    private IEnumerator MoveToPosition(Vector3 direction)
    {
        while (true)
        {
            transform.position += direction * (Time.deltaTime * _speed);
            yield return null;
        }
    }
}