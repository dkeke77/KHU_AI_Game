//
// ���� �ϼ��� �⺻ ��� ���� ������Ʈ�� ���߾� ������ ���� �����ؼ� ����ؾ� ��!!
// GameManager.cs ��ũ��Ʈ�� GameManager ������Ʈ�� Add Component �ϴ� �ͺ��� �����ϸ� ��.
// -> Scene �ȿ� �ϳ����� GameManager�� �����ǰ�, ���� ������ ��� UI�� �� ������Ʈ �ϳ��� ����.
//

// GameManager.cs
using UnityEngine;
using TMPro;
using System.Collections;  // �ڷ�ƾ ������ �ʿ�

public class GameManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI winCountPlayer;
    public TextMeshProUGUI winCountEnemy;

    [Header("Result Panel")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;

    [Header("Agents")]
    public CharacterCore attackAgent;
    public CharacterCore defenseAgent;

    [Header("Episode Controller")]
    public EpisodeController episodeCtrl;

    int winsPlayer = 0, winsEnemy = 0;
    bool hasResultShown = false;

    void Start()
    {
        // �ʱ�ȭ
        winsPlayer = winsEnemy = 0;
        UpdateWinUI();
        // ��� �г��� �ݵ�� ��Ȱ��ȭ ���¿��� ����
        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    void Update()
    {
        /*
       // �� ������Ʈ�� ��� ��� ������ ���� �ο��� ���� �÷��� ����
       if (attackAgent.cur_hp > 0 && defenseAgent.cur_hp > 0)
       {
            hasResultShown = false;
       }

        if (hasResultShown) return;   // �̹� �� �� ����� ������� �� �̻� ó�� �� ��

        bool atkDead = attackAgent != null && attackAgent.cur_hp <= 0;
        bool defDead = defenseAgent != null && defenseAgent.cur_hp <= 0;

        if (atkDead || defDead)
        {
            // ����� �����Ǹ�
            hasResultShown = true;
            //ShowAndCount(atkDead, defDead);
            // ��� ShowAndCount�� �ƴ϶�, ���� �ڷ�ƾ�� ����
            StartCoroutine(OnDeathDelay(atkDead, defDead));
        }*/
    }
    IEnumerator OnDeathDelay(bool atkDead, bool defDead)
    {
        // 2.5�� ���� ��� (�״� ��� ��� �ð� ���)
        yield return new WaitForSeconds(2.5f);

        // ���� ���� �Ŀ� ��� �г� ����
        ShowAndCount(atkDead, defDead);
    }

    void ShowAndCount(bool atkDead, bool defDead)
    {
        // 1) Draw
        if (atkDead && defDead)
        {
            resultText.text = "Draw!";
        }
        // 2) ������ �¸� (���� �׾��� ��)
        else if (!atkDead && defDead)
        {
            winsPlayer++;
            UpdateWinUI();
            resultText.text = "AT Agent Win!";
        }
        // 3) ������ �¸� (���ݸ� �׾��� ��)
        else if (atkDead && !defDead)
        {
            winsEnemy++;
            UpdateWinUI();
            resultText.text = "DF Agent Win!";
        }

        // 4) �г� �ѱ�
        if (resultPanel != null) resultPanel.SetActive(true);

        // 5) ���Ǽҵ� �ø���
        episodeCtrl?.NextEpisode();

        // 6) 5�� �� �г� ����� & ���� ���Ǽҵ� �غ�
        Invoke(nameof(HideResultPanel), 5.0f);
    }

    void UpdateWinUI()
    {
        if (winCountPlayer != null) 
            winCountPlayer.text = winsPlayer.ToString();
        if (winCountEnemy != null) 
            winCountEnemy.text = winsEnemy.ToString();
    }

    void HideResultPanel()
    {
        if (resultPanel != null) 
            resultPanel.SetActive(false);
    }
}


