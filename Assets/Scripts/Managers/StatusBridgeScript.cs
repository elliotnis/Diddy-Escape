using UnityEngine;

public class StatusBridgeScript : MonoBehaviour
{
    public GameObject player;
    public HUDController hudController;
    

    private float maxHealth = 100f;
    private float maxStamina = 100f;
    private PlayerMovement playerMovement;

    void Start()
    {
        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
        }
    }

    void Update()
    {
        if (player != null && hudController != null)
        {
            float normalizedHealth = playerMovement.health/maxHealth;
            float normalizedStamina = playerMovement.stamina / maxStamina;

            hudController.SetHealth(normalizedHealth);
            hudController.SetStamina(normalizedStamina);

        }
    }
}
