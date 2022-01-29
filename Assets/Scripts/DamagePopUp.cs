using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class DamagePopUp : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh;
    public void SetupDamagePopUp(float damageAmount)
    {
        textMesh.SetText(damageAmount.ToString("0"));
        StartCoroutine(AnimPopUpDamage(10f));
    }

    private IEnumerator AnimPopUpDamage(float destroyDuration)
    {
        yield return new WaitForSeconds(destroyDuration);
        Destroy(gameObject);
    }
}
