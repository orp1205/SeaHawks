using UnityEngine;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int totalScore = 0;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    public void AddScore(int s)
    {
        totalScore += s;
        Debug.Log("[GameManager] Score added: " + s + " | Total: " + totalScore);
    }
}
