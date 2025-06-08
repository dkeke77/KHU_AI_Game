using UnityEngine;
using TMPro;

public class EpisodeController : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI episodeText;   // �ϴ� �߾� �ؽ�Ʈ ���� ����

    int currentEpisode = 1;

    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        // Ű �׽�Ʈ��: E Ű�� ������ ���� ���Ǽҵ�
        if (Input.GetKeyDown(KeyCode.E))
            NextEpisode();
    }

    // �ܺο��� ȣ���Ϸ��� public����
    public void NextEpisode() // ���߿� ML-Agents���� �� ���Ǽҵ尡 ���� ������ ȣ���ؼ� �����ָ� ��!
    { // EpisodeController.NextEpisode() ���߿� ȣ���ϵ���
        currentEpisode++;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (episodeText != null)
            episodeText.text = $"EPISODE {currentEpisode}";
    }
}
