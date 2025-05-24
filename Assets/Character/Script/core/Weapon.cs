using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int damage = 10;
    public CapsuleCollider meleeArea;
    public float activationTime = 1.0f;

    public void use()
    {
        StopCoroutine("Swing");
        StartCoroutine("Swing");
    }

    IEnumerator Swing()
    {
        yield return new WaitForSeconds(0.1f);
        meleeArea.enabled = true;

        yield return new WaitForSeconds(activationTime);
        meleeArea.enabled = false;
    }
}
