using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    public BoxCollider sheildArea;
    public float activationTime =1.5f;

    public void use()
    {
        StopCoroutine("Block");
        StartCoroutine("Block");
    }

    IEnumerator Block()
    {
        yield return new WaitForSeconds(0.1f);
        sheildArea.enabled = true;

        yield return new WaitForSeconds(activationTime);
        sheildArea.enabled = false;
    }
}
