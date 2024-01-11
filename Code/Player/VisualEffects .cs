using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class VisualEffects
{
    [SerializeField] private GameObject m_TransistionShapeEffect;
    [SerializeField] private GameObject m_PlayDeathEffect;
    [SerializeField] private float Duration;
    [SerializeField] private float Strength;
    [SerializeField] private float Randomness;
    [SerializeField] private int Vibrato;

    private Outline m_PreviousOutline;
    public void ShakeObject(GameObject shapeObject)
    {
        if (shapeObject != null)
            shapeObject.transform.DOShakeScale(Duration, Strength, Vibrato, Randomness);
    }
    public void PlayTransistionShapeEffect(bool param = true)
    {
        if (m_TransistionShapeEffect != null)
            m_TransistionShapeEffect.SetActive(param);
    }
    public IEnumerator PlayDeathEffectCoroutine(float duration = 1f,bool isActive = true)
    {
        WaitForSeconds wait = new WaitForSeconds(duration);
        yield return wait;

        if (m_PlayDeathEffect != null)
            m_PlayDeathEffect.SetActive(isActive);
    }
    public void VisibleOutline(GameObject target)
    {
        Outline currentOutline = target.GetComponent<Outline>();

        HiddenOutline();

        if (currentOutline != m_PreviousOutline)
        {
            if (currentOutline != null)
            {
                currentOutline.enabled = true;
            }
            m_PreviousOutline = currentOutline;
        }
    }
    public void HiddenOutline()
    {
        if (m_PreviousOutline != null)
        {
            m_PreviousOutline.enabled = false;
            m_PreviousOutline = null;
        }
    }
}