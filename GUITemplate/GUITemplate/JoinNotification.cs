using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using Utils;
using UnityEngine;

[HarmonyPatch(typeof(MonoBehaviourPunCallbacks), nameof(MonoBehaviourPunCallbacks.OnPlayerEnteredRoom), new System.Type[] { typeof(Player) })]
class JoinNotification
{
    [HarmonyPrefix()]
    internal static void Prefix(Player newPlayer)
    {
        string message = $"PLAYER: {newPlayer.NickName} JOINED THE ROOM";
        Notification.AddNotification(NotificationType.Info, message, 2f, new Color(1f, 1f, 1f));
    }
}