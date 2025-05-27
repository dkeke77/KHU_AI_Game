//
// ���� �ϼ��� �⺻ ��� ���� ������Ʈ�� ���߾� ������ ���� �����ؼ� ����ؾ� ��!!
// GameManager.cs ��ũ��Ʈ�� GameManager ������Ʈ�� Add Component �ϴ� �ͺ��� �����ϸ� ��.
// -> Scene �ȿ� �ϳ����� GameManager�� �����ǰ�, ��� ���� ������ ��� UI�� �� ������Ʈ �ϳ��� ����.
//

//using UnityEngine;
//using TMPro;

//public class GameManager : MonoBehaviour
//{
//    public HealthSystem playerHS;
//    public HealthSystem enemyHS;

//    public TextMeshProUGUI winCountPlayer;
//    public TextMeshProUGUI winCountEnemy;

//    public GameObject resultPanel;
//    public TextMeshProUGUI resultText;

//    private int winsPlayer = 0;
//    private int winsEnemy = 0;

//    void Start()
//    {
//        // ü�� 0 �̺�Ʈ ����
//        playerHS.OnDead.AddListener(OnPlayerDead);
//        enemyHS.OnDead.AddListener(OnEnemyDead);

//        // �ʱ� UI
//        resultPanel.SetActive(false);
//        UpdateWinCountUI();
//    }

//    private void OnPlayerDead()
//    {
//        // �� �¸�
//        winsEnemy++;
//        ShowResult("Defense Agent"); // Enemy
//    }

//    private void OnEnemyDead()
//    {
//        // �÷��̾� �¸�
//        winsPlayer++;
//        ShowResult("Attack Agent"); // Player
//    }

//    private void ShowResult(string winner)
//    {
//        // ��� �г� ǥ��
//        resultText.text = $"WINNER: {winner}";
//        resultPanel.SetActive(true);

//        // �¸� Ƚ�� ����
//        UpdateWinCountUI();

//        // �ڵ� ���� Ȥ�� ��ư ���
//        Invoke(nameof(RestartMatch), 2.0f);
//    }

//    private void UpdateWinCountUI()
//    {
//        winCountPlayer.text = winsPlayer.ToString();
//        winCountEnemy.text = winsEnemy.ToString();
//    }

//    private void RestartMatch()
//    {
//        // �г� ���� ü�� ����
//        resultPanel.SetActive(false);
//        playerHS.Heal(playerHS.maxHP);
//        enemyHS.Heal(enemyHS.maxHP);
//    }
//}
