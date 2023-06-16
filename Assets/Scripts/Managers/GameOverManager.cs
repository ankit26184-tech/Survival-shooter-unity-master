using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] private GameStateManager gameStateManager;
    public PlayerHealth playerHealth;


    Animator anim;


    void Awake()
    {
        anim = GetComponent<Animator>();
    }


    void Update()
    {
        if (playerHealth.CurrentHealth <= 0)
        {
            gameStateManager.ClearGameData();
            anim.SetTrigger("GameOver");
        }
    }
}
