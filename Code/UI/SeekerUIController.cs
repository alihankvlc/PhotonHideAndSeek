using System.Collections;
using UnityEngine;

public class SeekerUIController : MonoBehaviour
{
    [SerializeField] private GameObject m_Effect;
    [SerializeField] private Transform[] m_SpawnPositionsArray;

    private Outline m_Outline;
    private Animator m_Animator;

    private bool m_IsSelection;
    private readonly int HIGHLIGHTED_VAL_HASH = Animator.StringToHash("Highlighted");
    private void Start()
    {
        m_Outline = GetComponent<Outline>();
        m_Animator = GetComponent<Animator>();
    }
    private void OnMouseEnter()
    {
        OnHighlighted();
    }
    private void OnMouseExit()
    {
        OnHighlighted(false);
    }
    private void OnMouseDown()
    {
        TeamManager.OnTeamSelection?.Invoke(PlayerTeam.Seeker,ref m_IsSelection);
    }
    private void OnHighlighted(bool param = true)
    {
        m_Outline.enabled = param;
        m_Animator.SetBool(HIGHLIGHTED_VAL_HASH, param);
        m_Effect.SetActive(param);
    }
}