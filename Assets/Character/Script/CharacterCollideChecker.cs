using UnityEngine;

public class CharacterCollideChecker : MonoBehaviour
{
    public bool isCollideWithCharacter = false;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Character"))
        {
            isCollideWithCharacter = true;
            Debug.Log("Collide With Character!!");
        }
    }
}
