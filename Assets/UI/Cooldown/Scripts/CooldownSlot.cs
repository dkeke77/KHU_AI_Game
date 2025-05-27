using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CooldownSlot : MonoBehaviour
{
    [Header("UI References")]
    public Image backgroundImage;
    public Image fillImage;
    public TextMeshProUGUI timerText;

    [Header("Cooldown Settings")]
    public float cooldownTime = 2.5f;

    private float remainingTime = 0f;
    private Color activeColor, inactiveColor;
    private bool isCooling = false;

    void Start()
    {
        activeColor = fillImage.color;
        inactiveColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        EndCooldown();
    }

    void Update()
    {
        if (!isCooling) return;

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            EndCooldown();
            return;
        }

        // 진행량 비율
        float t = 1f - (remainingTime / cooldownTime);
        fillImage.fillAmount = t;
        fillImage.color = Color.Lerp(inactiveColor, activeColor, t);
        backgroundImage.color = inactiveColor;

        // 0.5초 단위 반올림하여 표시
        float display = Mathf.Ceil(remainingTime * 2f) / 2f;
        timerText.text = display.ToString("0.0");
    }

    public void StartCooldown()
    {
        isCooling = true;
        remainingTime = cooldownTime;
        fillImage.fillAmount = 0f;
        fillImage.color = inactiveColor;
        backgroundImage.color = inactiveColor;
        timerText.text = cooldownTime.ToString("0.0");
    }

    private void EndCooldown()
    {
        isCooling = false;
        fillImage.fillAmount = 1f;
        fillImage.color = activeColor;
        backgroundImage.color = activeColor;
        timerText.text = "";
    }
}
