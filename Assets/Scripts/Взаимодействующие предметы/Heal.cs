using UnityEngine;

public class Heal : MonoBehaviour
{
    public int healthIncreaseAmount = 20;
    private PlayerScript playerController;

    void Start()
    {
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerController.Heal(healthIncreaseAmount);
            Destroy(gameObject);
        }
    }
}
