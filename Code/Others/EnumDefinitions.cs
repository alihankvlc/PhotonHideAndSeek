using System.ComponentModel;
using UnityEngine;

public enum PlayerForm
{
    Human,
    Shape,
    Ghost
}
public enum PlayerTeam
{
    Hider,
    Seeker
}
public enum TMP_InputSystemMessage
{
    [Description("Username must be a minimum of 3 characters.")]
    UsernameCannotBeEmpty,


    [Description("Username cannot be left empty.")]
    UsernameLengthMinimum
}
public enum DoorEventType
{
    Locked,
    Closed,
    Open
}
public enum GameStatusEventType
{
    [Description("Preparing for the game")]
    Preparing = 0,

    [Description("Waiting for a player from the <color=red>HIDER</color> team")]
    WaitingForHiderPlayer = 1,

    [Description("Waiting for a player from the <color=red>SEEKER</color> team")]
    WaitingForSeekerPlayer = 2,

    [Description("MATCH STARTING IN")]
    MatchStarting = 3,

    [Description("MATCH STARTED")]
    MatchStarted = 4
}
public enum ScoreEventType
{
    Kill,
    Death,
    Win,
    Lose
}
public enum PlayerStatus
{
    [Description("entered the game.")]
    EnteredGame,

    [Description("left the game.")]
    LeftGame,

    [Description("joined the game as a Hider.")]
    JoinedHiderTeam,

    [Description("joined the game as a Seeker.")]
    JoinedSeekerTeam
}