// CharacterHealthUI.cs
using UnityEngine;
using TMPro;

[RequireComponent(typeof(CharacterInfo))]
public class CharacterHealthWidthUI : MonoBehaviour
{
    [Header("Character Data")]
    public CharacterInfo info;     // CharacterInfo ����

    [Header("UI Settings")]
    public RectTransform barRect;  // HealthBar �̹����� ���� RectTransform
    public float maxWidth = 400f;  // full HP�� �� width �� (400)

    [Header("Optional Text")]
    public TextMeshProUGUI hpText; // ü�� ���� ǥ�� (����)

    void Start()
    {
        if (info == null) info = GetComponent<CharacterInfo>();

        if (barRect == null)
            Debug.LogError("barRect ���Կ� RectTransform�� �����ϼ���.", this);
        if (hpText == null)
            Debug.LogWarning("hpText�� ���� �����Դϴ�.", this);
    }

    void Update()
    {
        if (info == null || barRect == null) return;

        // 0~1 ���� ���� ���
        float ratio = Mathf.Clamp01(info.CurrentHP / (float)CharacterCore.MAX_HP);

        // sizeDelta.x �� ������ ���� ����
        var size = barRect.sizeDelta;
        size.x = ratio * maxWidth;
        barRect.sizeDelta = size;

        // ���� ǥ�� (������)
        if (hpText != null)
            hpText.text = $"{info.CurrentHP:0}";
    }
}
