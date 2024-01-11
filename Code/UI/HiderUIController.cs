using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HiderUIController : MonoBehaviour
{
    [SerializeField] private GameObject m_ShakeObject;
    [SerializeField] private GameObject[] m_PlayerPrefab;
    [SerializeField] private VisualEffects m_VisualEffects;
    [SerializeField] private Transform[] m_SpawnPositionsArray;

    private AudioSource m_AudioSource;
    private bool m_IsSelection;
    private void OnMouseEnter()
    {
        m_VisualEffects.ShakeObject(m_ShakeObject);
        m_VisualEffects.PlayTransistionShapeEffect();
        SetActiveHiderObjects();
    }
    private void OnMouseExit()
    {
        SetActiveHiderObjects(true);
        m_VisualEffects.PlayTransistionShapeEffect(false);
    }
    private void SetActiveHiderObjects(bool param = false)
    {
        for (int i = 0; i < m_PlayerPrefab.Length; i++)
            m_PlayerPrefab[i].SetActive(param);

        m_ShakeObject.SetActive(!param);
    }
    private void OnMouseDown()
    {
        TeamManager.OnTeamSelection?.Invoke(PlayerTeam.Hider, ref m_IsSelection);
    }
}