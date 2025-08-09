using UnityEngine;

public class HoopController : MonoBehaviour
{
    public int normalScore = 2;
    public int perfectScore = 3;
    public float rimHitWindow = 0.3f;

    public ParticleSystem fireParticle;
    public ParticleSystem perfectParticle;

    private float lastRimHitTime = -10f;
    private int perfectStreak = 0;

    public void NotifyRimHit()
    {
        lastRimHitTime = Time.time;
    }

    public void OnBallScored(GameObject ball)
    {
        bool rimRecentlyHit = (Time.time - lastRimHitTime) <= rimHitWindow;
        int points;

        if (!rimRecentlyHit)
        {
            points = perfectScore;
            perfectStreak++;

            if (perfectParticle != null)
                perfectParticle.Play();

            if (perfectStreak >= 3 && fireParticle != null && !fireParticle.isPlaying)
                fireParticle.Play();
        }
        else
        {
            points = normalScore;

            if (perfectStreak >= 3 && fireParticle != null && fireParticle.isPlaying)
                fireParticle.Stop();

            perfectStreak = 0;
        }
        Debug.Log($"[HoopController] Ball scored! Points: {points} | Perfect Streak: {perfectStreak}");
        GameManager.Instance.AddScore(points);
    }

    public void OnMiss()
    {
        if (perfectStreak >= 3 && fireParticle != null && fireParticle.isPlaying)
            fireParticle.Stop();

        perfectStreak = 0;
    }
}
