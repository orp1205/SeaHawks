using UnityEngine;

public class ScoreTrigger : MonoBehaviour
{
    public HoopController hoop;
    public RimHitDetector rimHitDetector;

    private void OnTriggerEnter(Collider other)
    {
        var ball = other.GetComponent<BasketBall>();
        if (ball != null)
        {
            hoop.OnBallScored(ball.gameObject);
            if (rimHitDetector != null)
                rimHitDetector.RemoveBallFromCheck(ball.gameObject);
        }
    }
}
