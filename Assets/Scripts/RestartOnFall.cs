using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartOnFall : MonoBehaviour
{
    public float fallThreshold = -10f;
    public Vector2 crushCheckSize = new Vector2(0.8f, 0.8f); // size of the box to check around the player

    void Update()
    {
        if (transform.position.y < fallThreshold)
        {
            RestartGame();
        }

        CheckForCrush();
    }

    void CheckForCrush()
    {
        Vector2 center = transform.position;
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, crushCheckSize, 0f);

        int crushCount = 0;
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Crush"))
            {
                crushCount++;
            }
        }

        // If crushed between two or more "Crush" objects
        if (crushCount >= 2)
        {
            RestartGame();
        }
    }

    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, crushCheckSize);
    }
}