using UnityEngine;
using System.Collections.Generic;

public class RimHitDetector : MonoBehaviour
{
    public HoopController hoop;
    public float missCheckDelay = 0.3f;

    private Dictionary<GameObject, float> rimHitBalls = new Dictionary<GameObject, float>();

    private void OnCollisionEnter(Collision collision)
    {
        var ball = collision.collider.GetComponent<BasketBall>();
        if (ball != null)
        {
            hoop.NotifyRimHit();
            rimHitBalls[ball.gameObject] = Time.time;
        }
    }

    private void Update()
    {
        var toRemove = new List<GameObject>();

        foreach (var kv in rimHitBalls)
        {
            if (Time.time - kv.Value > missCheckDelay)
            {
                hoop.OnMiss();
                toRemove.Add(kv.Key);
            }
        }

        foreach (var b in toRemove)
            rimHitBalls.Remove(b);
    }

    public void RemoveBallFromCheck(GameObject ball)
    {
        if (rimHitBalls.ContainsKey(ball))
            rimHitBalls.Remove(ball);
    }
}
