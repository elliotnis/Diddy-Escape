using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("Avatar")]
    public RawImage avatarHUD;

    [Header("Bars (RawImage Fill Objects)")]
    public RawImage healthHUD;
    public RawImage staminaHUD;

    private float originalHealthWidth;
    private float originalStaminaWidth;

    void Awake()
    {
        originalHealthWidth = healthHUD.rectTransform.sizeDelta.x;
        originalStaminaWidth = staminaHUD.rectTransform.sizeDelta.x;
    }

    public void SetAvatar(Texture avatarTexture)
    {
        avatarHUD.texture = avatarTexture;
    }

    public void SetHealth(float normalizedHealth)
    {
        normalizedHealth = Mathf.Clamp01(normalizedHealth);
        healthHUD.rectTransform.sizeDelta = new Vector2(originalHealthWidth * normalizedHealth,
                                                          healthHUD.rectTransform.sizeDelta.y);
    }

    public void SetStamina(float normalizedStamina)
    {
        normalizedStamina = Mathf.Clamp01(normalizedStamina);
        staminaHUD.rectTransform.sizeDelta = new Vector2(originalStaminaWidth * normalizedStamina,
                                                           staminaHUD.rectTransform.sizeDelta.y);
    }
}
