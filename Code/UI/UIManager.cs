using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [Header("UI Elements")]
    [SerializeField] private GameObject m_Crosshair;
    [SerializeField] private GameObject m_InteractionPopupParentObject;
    [SerializeField] private GameObject m_TransistionShapeParentObject;
    [SerializeField] private GameObject m_ShapeKeyInfoPopup;
    [SerializeField] private GameObject[] m_UITeamSelectionElements;
    [SerializeField] private GameObject m_FadeOutEffect;
    [SerializeField] private GameObject m_VeritfyPlayerName;
    [SerializeField] private GameObject m_GameStatusParentObject;
    [SerializeField] private GameObject m_PlayerHealthParentObject;
    [SerializeField] private GameObject m_PlayerSpawnableObjectUI;
    [SerializeField] private GameObject m_GhostScreenVignette;
    [SerializeField] private GameObject m_PlayerBoardParentObject;
    [SerializeField] private Image m_WhistleKeyImage;
    [SerializeField] private Image m_HealthBackgroundImage;
    [SerializeField] private Button m_RandomNameButton;

    [Header("Text Elements")]
    [SerializeField] private TextMeshProUGUI m_TMP_InteractionInfo;
    [SerializeField] private TextMeshProUGUI m_TMP_SystemDialogs;
    [SerializeField] private TextMeshProUGUI m_TMP_GameStatusInfo;
    [SerializeField] private TextMeshProUGUI m_TMP_PlayerNameSystemMessage;
    [SerializeField] private TextMeshProUGUI m_TMP_HiderPlayerCount;
    [SerializeField] private TextMeshProUGUI m_TMP_SeekerPlayerCount;
    [SerializeField] private TextMeshProUGUI m_TMP_HealthValue;
    [SerializeField] private List<TextMeshProUGUI> m_TMP_PlayerRoomStatus = new List<TextMeshProUGUI>();

    [Header("Other Settings")]
    [SerializeField] private string[] m_RandomPlayerNameArrayList;

    [SerializeField] private Transform m_PlayerBoardContentHolder;

    [Header("Color Settings")]
    [SerializeField] private Color m_AliveHealthBarColor;
    [SerializeField] private Color m_DeathHealthBarColor;

    public TMP_InputField m_TMP_Input_UserName;

    #region Events
    public static Action<bool> OnShowTeamMenu;
    public static Action<bool> OnPlayerFormShapeKeyInfo;
    public static Action<float> OnGameStartingInfo;
    public static Action<string, PlayerStatus> OnPlayerStatusInfo;

    public delegate string SetPlayerInfoDelegate();
    public static SetPlayerInfoDelegate OnPlayerSetName;
    public static Action<PlayerTeam, bool> OnShowPlayerStat;

    public static Action<int> OnUpdateHiderCount;
    public static Action<int> OnUpdateSeekerPlayerCount;
    public static Action<GameStatusEventType, float> OnUpdateGameStatus;
    public static Action<float> OnUpdatePlayerHealth;
    public static Action<bool> OnShowGhostFormScreen;
    public static Action<bool> OnShowGameStatusWindow;
    public static Action<bool> OnShowTransistionShapePopup;
    public static Action<bool> OnShowInteractPopup;
    public static Action<GameObject> OnSetPlayerBoardContentObjectTransform; 
    #endregion
    private void Update()
    {
        m_VeritfyPlayerName.SetActive(m_TMP_Input_UserName.text.Length > 3 &&
            !string.IsNullOrEmpty(m_TMP_Input_UserName.text));

        m_PlayerBoardParentObject.SetActive(Input.GetKey(KeyCode.Tab));
    }
    private void Start()
    {
        #region Subscribe
        OnShowTeamMenu += Window_TeamMenu;
        OnPlayerStatusInfo += TMP_OnPlayerRoomStatusInfo;
        OnPlayerSetName += SetPlayerName;
        OnShowInteractPopup += Window_Interaction;
        OnPlayerFormShapeKeyInfo += ShowPlayerFormShapeKey;
        OnShowPlayerStat += UI_PlayerHealth;

        OnUpdateHiderCount += TMP_UpdateHiderCount;
        OnUpdateSeekerPlayerCount += TMP_UpdateSeekerCount;
        OnUpdateGameStatus += UI_UpdateGameStatus;
        OnUpdatePlayerHealth += UI_PlayerHealth;
        OnShowGhostFormScreen += UI_ShowGhostFormScreen;
        OnShowGameStatusWindow += Window_GameStatus;
        OnShowTransistionShapePopup += Window_TransistionShape;
        OnSetPlayerBoardContentObjectTransform += Window_PlayerBoardConent; 
        #endregion
        m_RandomNameButton.onClick.AddListener(SelectRandomPlayerName);
    }
    private void OnDestroy()
    {
        #region Unsubscribe
        OnShowTeamMenu -= Window_TeamMenu;
        OnPlayerStatusInfo -= TMP_OnPlayerRoomStatusInfo;
        OnPlayerSetName -= SetPlayerName;
        OnShowInteractPopup -= Window_Interaction;
        OnPlayerFormShapeKeyInfo -= ShowPlayerFormShapeKey;
        OnShowPlayerStat -= UI_PlayerHealth;

        OnUpdateHiderCount -= TMP_UpdateHiderCount;
        OnUpdateSeekerPlayerCount -= TMP_UpdateSeekerCount;
        OnUpdateGameStatus -= UI_UpdateGameStatus;
        OnUpdatePlayerHealth -= UI_PlayerHealth;
        OnShowGhostFormScreen -= UI_ShowGhostFormScreen;
        OnShowGameStatusWindow -= Window_GameStatus;
        OnShowTransistionShapePopup -= Window_TransistionShape;
        OnSetPlayerBoardContentObjectTransform -= Window_PlayerBoardConent; 
        #endregion
    }
    #region UI
    private void Window_PlayerBoardConent(GameObject content)
    {
        content.transform.SetParent(m_PlayerBoardContentHolder, false);
    }
    private void UI_UpdateGameStatus(GameStatusEventType status, float matchStartingTimer)
    {
        string description = GetEnumDescription(status);
        m_GameStatusParentObject.SetActive(true);

        m_TMP_GameStatusInfo.SetText(description);

        if (status != GameStatusEventType.MatchStarting) return;
        m_TMP_GameStatusInfo.SetText($"{GetEnumDescription(GameStatusEventType.MatchStarting)}: {matchStartingTimer.ToString("F0")}");
    }
    private void Window_GameStatus(bool param)
    {
        m_GameStatusParentObject.SetActive(param);
    }
    private void Window_TeamMenu(bool param)
    {
        for (int i = 0; i < m_UITeamSelectionElements.Length; i++)
            m_UITeamSelectionElements[i].SetActive(param);

        m_Crosshair.SetActive(true);
        m_FadeOutEffect.SetActive(true);
    }
    public void Window_Interaction(bool param)
    {
        m_InteractionPopupParentObject.SetActive(param);
    }
    private void Window_TransistionShape(bool param)
    {
        m_TransistionShapeParentObject.SetActive(param);
    }
    private void ShowPlayerFormShapeKey(bool param)
    {
        m_ShapeKeyInfoPopup.SetActive(param);
    }
    private void UI_ShowGhostFormScreen(bool param)
    {
        m_PlayerHealthParentObject.SetActive(!param);
        m_GhostScreenVignette.SetActive(param);
        m_Crosshair.SetActive(!param);

        m_HealthBackgroundImage.color = param ? m_DeathHealthBarColor : m_AliveHealthBarColor;
    }
    private void UI_PlayerHealth(float value)
    {
        Slider slider = m_PlayerHealthParentObject.GetComponent<Slider>();
        if (slider != null)
        {
            slider.value = Mathf.RoundToInt(value);
            m_TMP_HealthValue.SetText(slider.value.ToString("F0"));
        }
    }
    #endregion
    #region GameStatusInfo
    public void UI_PlayerHealth(PlayerTeam team, bool param)
    {
        m_PlayerHealthParentObject.SetActive(param);
        m_PlayerSpawnableObjectUI.SetActive(team == PlayerTeam.Hider);
    }
    public void Slider_UpdatePlayerHealth(float value)
    {
        Slider slider = m_PlayerHealthParentObject.GetComponent<Slider>();
        slider.value = value;
        m_TMP_HealthValue.SetText(slider.value.ToString());
    }
    #endregion
    #region PlayerRoomStatusInfo
    private void TMP_OnPlayerRoomStatusInfo(string playerName, PlayerStatus status)
    {
        photonView.RPC(nameof(PlayerRoomStatusInfo), RpcTarget.All, playerName, status);
    }
    [PunRPC]
    private void PlayerRoomStatusInfo(string playerName, PlayerStatus status)
    {

        TextMeshProUGUI informationTextMesh = m_TMP_PlayerRoomStatus.Where(r => !r.gameObject.activeSelf).First();

        informationTextMesh.gameObject.SetActive(true);
        string description = GetEnumDescription(status);

        if (informationTextMesh != null)
            informationTextMesh.SetText($"<color=green>{playerName}</color> {description}");

        GameObjectSetActive(informationTextMesh.gameObject, 2.5f, false);
    }
    private void GameObjectSetActive(GameObject gameObject, float duration, bool param = true)
    {
        StartCoroutine(GameObjectSetActiveCoroutine(gameObject, duration, param));
    }
    private IEnumerator GameObjectSetActiveCoroutine(GameObject gameObject, float duration, bool param)
    {
        WaitForSeconds delay = new WaitForSeconds(duration);

        yield return delay;
        gameObject.SetActive(false);
    }
    private void TMP_UpdateHiderCount(int value)
    {
        m_TMP_HiderPlayerCount.SetText($"HIDER({value})");
    }
    private void TMP_UpdateSeekerCount(int value)
    {
        m_TMP_SeekerPlayerCount.SetText($"SEEKER({value})");
    }
    private string SetPlayerName()
    {
        return m_TMP_Input_UserName.text;
    }
    public bool IsValidPlayerName()
    {
        if (CheckPlayerNameEmpty() && CheckPlayerNameLength())
        {
            return true;
        }
        return false;
    }
    public bool CheckPlayerNameEmpty()
    {
        string emptyErrorMessage = GetEnumDescription(TMP_InputSystemMessage.UsernameLengthMinimum);
        string playerName = m_TMP_Input_UserName.text;

        if (!string.IsNullOrEmpty(playerName))
        {
            return true;
        }
        ShowErrorMessage(emptyErrorMessage, m_TMP_PlayerNameSystemMessage);
        return false;
    }
    public bool CheckPlayerNameLength()
    {
        string lengthErrorMessage = GetEnumDescription(TMP_InputSystemMessage.UsernameCannotBeEmpty);
        string playerName = m_TMP_Input_UserName.text;

        if (playerName.Length > 3)
        {
            return true;
        }

        ShowErrorMessage(lengthErrorMessage, m_TMP_PlayerNameSystemMessage);
        return false;
    }
    private void ShowErrorMessage(string errorMessage, TextMeshProUGUI textMesh)
    {
        textMesh.gameObject.SetActive(true);
        textMesh.SetText(errorMessage);
        GameObjectSetActive(textMesh.gameObject, 2f, false);
    }
    private void SelectRandomPlayerName()
    {
        int randomIndex = UnityEngine.Random.Range(0, m_RandomPlayerNameArrayList.Length);
        m_TMP_Input_UserName.text = m_RandomPlayerNameArrayList[randomIndex];
    }
    #endregion
    private string GetEnumDescription(Enum value)
    {
        FieldInfo field = value.GetType().GetField(value.ToString());
        DescriptionAttribute attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
        return attribute?.Description ?? value.ToString();
    }
}
