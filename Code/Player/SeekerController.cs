using ExitGames.Client.Photon;
using Photon.Pun;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SeekerController : PlayerController
{
    [SerializeField] private Transform m_MeleeTransform;
    [SerializeField] private float m_WeaponLength;
    [SerializeField] private float m_WeaponDamage;
    [SerializeField] private LayerMask m_DamageableLayerMask;
    [SerializeField] private LayerMask m_RigLayerMask;
    [SerializeField] private Transform m_TargetRigObject;
    [SerializeField] private float m_TargetRigObjectDistance;
    private bool m_CanDealDamage;
    private List<GameObject> m_HasDealDamage = new List<GameObject>();

    [Header("Others")]
    [SerializeField] private TextMeshProUGUI m_TMP_SeekerUsername;


    private Hashtable m_ScoreProperties = new Hashtable();
    private readonly int ATTACK_HASH_ID = Animator.StringToHash("Attack");

    private Ray m_Ray;
    private RaycastHit m_HitInfo;

    public static Action<bool> OnShowSeekerPlayerName;
    protected override void Start()
    {
        base.Start();
        m_CanDealDamage = false;
        OnShowSeekerPlayerName += UI_ShowPlayerName;

        if (!m_PhotonView.IsMine)
        {
            m_TMP_SeekerUsername.SetText(m_PhotonView.Owner.NickName);
            m_TMP_SeekerUsername.transform.gameObject.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        OnShowSeekerPlayerName -= UI_ShowPlayerName;
    }
    protected override void Update()
    {
        base.Update();


        if (m_PhotonView.IsMine)
        {
            if (Input.GetMouseButtonDown(0))
            {
                m_PhotonView.RPC(nameof(SyncAttacakAnimation), RpcTarget.All);
            }

            CastRay();
        }
    }
    private void CastRay()
    {
        if (m_CanDealDamage)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(m_MeleeTransform.position, -m_MeleeTransform.up, out hitInfo, m_WeaponLength, m_DamageableLayerMask))
            {
                if (!m_HasDealDamage.Contains(hitInfo.transform.gameObject))
                {
                    PhotonView photonView = hitInfo.collider.GetComponentInChildren<PhotonView>();
                    if (photonView != null)
                    {
                        ApplyAttack(photonView.ViewID);
                    }
                    m_HasDealDamage.Add(hitInfo.transform.gameObject);
                }
            }
        }

        m_Ray.origin = Camera.main.transform.position;
        m_Ray.direction = Camera.main.transform.forward;
        Physics.Raycast(m_Ray, out m_HitInfo, Mathf.Infinity, m_RigLayerMask);

        if (m_HitInfo.collider != null)
            m_TargetRigObject.transform.position = m_HitInfo.point;
    }
    public void StartDealDamage()
    {
        m_CanDealDamage = true;
        m_HasDealDamage.Clear();
        AudioSource.PlayClipAtPoint(SoundManager.Instance.MeleeAttack, m_MeleeTransform.position, 0.5f);
    }
    private void UI_ShowPlayerName(bool param)
    {
        m_TMP_SeekerUsername.transform.gameObject.SetActive(param);
    }
    public void EndDealDamage()
    {
        m_CanDealDamage = false;
    }
    private void ApplyAttack(int id)
    {
        if (m_PhotonView.IsMine)
            m_PhotonView.RPC(nameof(SyncDealDamageRPC), RpcTarget.All, id);
    }
    [PunRPC]
    private void SyncDealDamageRPC(int id)
    {
        PhotonView targetPhotonView = PhotonView.Find(id);
        if (targetPhotonView != null)
        {
            IDamageable damageable = targetPhotonView.GetComponent<IDamageable>();
            damageable?.TakeDamage(25);
            m_PhotonView.RPC(nameof(SyncScore), RpcTarget.AllBuffered);

        }
    }
    [PunRPC]
    private void SyncScore()
    {
        m_PlayerInfo.PlayerScore += GameManager.OnSetPlayerScore(ScoreEventType.Kill);
        m_PlayerBoard.SetUpdateScore(m_PlayerInfo.PlayerScore);
    }
    [PunRPC]
    public void SyncAttacakAnimation()
    {
        if (m_Animator != null)
            m_Animator.Play(ATTACK_HASH_ID);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(m_MeleeTransform.position, m_MeleeTransform.position - m_MeleeTransform.up * m_WeaponLength);
    }
}
