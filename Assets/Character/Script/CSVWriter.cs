using UnityEngine;
using System.IO;

public class CSVWriter : MonoBehaviour
{
    public Transform Agent_ATK;
    public Transform Agent_DEF;

    CharacterCore Agent_ATK_core;
    CharacterCore Agent_DEF_core;

    string Arena_name;
    string Agent_ATK_Type;
    string Agent_DEF_Type;


    string filePath;

    public bool activate = false;
    public bool isRLInArena = false;
    bool isLineWrited = false;

    void Start()
    {
        if (activate)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;

            if (Agent_ATK.GetComponentInChildren<Offensive_BT>() != null)
            {
                Agent_ATK_Type = "BT_ATK";
            }
            else
            {
                Agent_ATK_Type = "RL_ATK";
                isRLInArena = true;
            }

            if (Agent_DEF.GetComponentInChildren<Defensive_BT>() != null)
            {
                Agent_DEF_Type = "BT_DEF";
            }
            else
            {
                Agent_DEF_Type = "RL_DEF";
                isRLInArena = true;
            }

            Agent_ATK_core = Agent_ATK.GetComponent<CharacterCore>();
            Agent_DEF_core = Agent_DEF.GetComponent<CharacterCore>();

            Arena_name = transform.name;
            filePath = $"{projectRoot}/CombatData/{Arena_name}--{Agent_DEF_Type}-{Agent_DEF_Type}.csv";

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, string.Format("Winner,{0}-Total_Attack,{0}-Success_Attack,{0}-Total_Defence,{0}-Success_Defence,{0}-Total_Dodge,{0}-Success_Dodge,{1}-Total_Attack,{1}-Success_Attack,{1}-Total_Defence,{1}-Success_Defence,{1}-Total_Dodge,{1}-Success_Dodge", Agent_ATK_Type, Agent_DEF_Type));
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (activate) 
        { 
            // 둘다 BT인 경우
            if (!isRLInArena && !isLineWrited)
            {
                if (Agent_ATK_core.isDead)
                {
                    WriteRLCombatData(Agent_DEF.name);
                    isLineWrited = true;
                }
                else if (Agent_DEF_core.isDead)
                {
                    WriteRLCombatData(Agent_ATK.name);
                    isLineWrited = true;
                }
            }
        // 둘중 하나가 RL이라면 RL쪽에서 에피소드가 종료될때 호출
        // 둘 다 RL이라면 한쪽만 호출되도록 설정해줘야함
        }
    }

    public void WriteRLCombatData(string Winner)
    {
        if (activate)
        {
            string winner_type = "No Winner";
            if (Winner == Agent_ATK.name)
                winner_type = Agent_ATK_Type;
            else if (Winner == Agent_DEF.name)
                winner_type = Agent_DEF_Type;

            WriteLine(new string[] {winner_type, $"{Agent_ATK_core.attackCounter}", $"{Agent_ATK_core.attackSucCounter}",
                                    $"{Agent_ATK_core.blockCounter}", $"{Agent_ATK_core.blockSucCounter}",
                                    $"{Agent_ATK_core.dodgeCounter}", $"{Agent_ATK_core.dodgeSucCounter}",
                                    $"{Agent_DEF_core.attackCounter}", $"{Agent_DEF_core.attackSucCounter}",
                                    $"{Agent_DEF_core.blockCounter}", $"{Agent_DEF_core.blockSucCounter}",
                                    $"{Agent_DEF_core.dodgeCounter}", $"{Agent_DEF_core.dodgeSucCounter}",
            });
        }
    }

    void WriteLine(string[] data)
    {
        using (StreamWriter writer = new StreamWriter(filePath, true)) // true = append 모드
        {
            string line = string.Join(",", data);
            writer.WriteLine(line);
        }
    }
}
