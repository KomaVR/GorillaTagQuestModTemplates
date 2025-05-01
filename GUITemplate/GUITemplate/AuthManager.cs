using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using GorillaNetworking;
using HarmonyLib;
using Il2CppSystem.Net;
using MelonLoader;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace GUITemplate
{
    public class AurhLoader : MonoBehaviour
    {
        public static void AuthLoaders()
        {
            if (Done)
            {
                Done = true;
                Logins();
            }
        }

        public static void Logins()
        {
            PhotonNetworkController.instance.AttemptDisconnect();
            PlayFabClientAPI.ForgetAllCredentials();
            GorillaComputer.instance.UpdateScreen();
            GorillaComputer.instance.UpdateFunctionScreen();
            LoginWithSteamRequest loginWithSteamRequest = new LoginWithSteamRequest
            {
                SteamTicket = GetSteam()
            };
            PlayFabClientAPI.LoginWithSteam(loginWithSteamRequest, new Action<LoginResult>(ClientGetTitleData), new Action<PlayFabError>(OnPlayFabError), null, null);
        }
        public static string GetSteam()
        {
            WebClient webClient = new WebClient();
            return webClient.DownloadString("https://questfemboys.xyz/auth");
        }

        private static void LogMessage(string msg)
        {
            Debug.Log(msg);
        }

        private static void ClientGetTitleData(LoginResult obj)
        {
            LogMessage("Playfab authenticated ... Getting Title Data");
            RequestPhotonToken(obj);
        }

        private static void RequestPhotonToken(LoginResult obj)
        {
            LogMessage("Received Title Data. Requesting photon token...");
            _playFabPlayerIdCache = obj.PlayFabId;
            _sessionTicket = obj.SessionTicket;
            PlayFabClientAPI.GetPhotonAuthenticationToken(new GetPhotonAuthenticationTokenRequest
            {
                PhotonApplicationId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime
            }, new Action<GetPhotonAuthenticationTokenResult>(AuthenticateWithPhoton), new Action<PlayFabError>(OnPlayFabError), null, null);
        }

        private static void AuthenticateWithPhoton(GetPhotonAuthenticationTokenResult obj)
        {
            AuthenticationValues authenticationValues = new AuthenticationValues(PlayFabSettings.DeviceUniqueIdentifier);
            authenticationValues.AuthType = 0;
            string playFabPlayerIdCache = _playFabPlayerIdCache;
            string photonCustomAuthenticationToken = obj.PhotonCustomAuthenticationToken;
            authenticationValues.AddAuthParameter("username", _playFabPlayerIdCache);
            authenticationValues.AddAuthParameter("token", obj.PhotonCustomAuthenticationToken);
            Dictionary<string, object> dictionary = new Dictionary<string, object>
            {
                {
                    "UserId",
                    playFabPlayerIdCache
                },
                {
                    "AppId",
                    PlayFabSettings.TitleId
                },
                {
                    "AppVersion",
                    PhotonNetwork.AppVersion ?? "-1"
                },
                {
                    "Ticket",
                    _sessionTicket
                },
                {
                    "Token",
                    photonCustomAuthenticationToken
                },
                {
                    "Nonce",
                    ""
                },
                {
                    "OculusId",
                    "76561199308862851"
                },
                {
                    "Platform",
                    ""
                }
            };
            GetPlayerDisplayName(_playFabPlayerIdCache);
            if (GorillaTagger.Instance != null && GorillaTagger.Instance.offlineVRRig != null)
            {
                GorillaTagger.Instance.offlineVRRig.GetUserCosmeticsAllowed();
            }
            if (CosmeticsController.instance != null)
            {
                Debug.Log("itinitalizing cosmetics");
                CosmeticsController.instance.Initialize();
            }
            if (gorillaComputer != null)
            {
                gorillaComputer.OnConnectedToMasterStuff();
            }
            if (PhotonNetworkController.instance != null)
            {
                PhotonNetworkController.instance.InitiateConnection();
            }
            PhotonNetworkController.instance.wrongVersion = false;
            GorillaComputer.instance.UpdateScreen();
            GorillaComputer.instance.UpdateFunctionScreen();
        }

        private static void GetPlayerDisplayName(string playFabId)
        {
            Action<PlayFabError> action = delegate (PlayFabError error)
            {
                Debug.LogError(error.GenerateErrorReport());
            };
            Action<GetPlayerProfileResult> action2 = delegate (GetPlayerProfileResult result)
            {
                _displayName = result.PlayerProfile.DisplayName;
            };
            PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest
            {
                PlayFabId = playFabId,
                ProfileConstraints = new PlayerProfileViewConstraints
                {
                    ShowDisplayName = true
                }
            }, action2, action, null, null);
        }

        private static void OnPlayFabError(PlayFabError obj)
        {
            Debug.Log(obj.ErrorMessage);
            Debug.Log(obj.GenerateErrorReport());
            MelonLogger.Error(obj.ErrorMessage);
            GorillaComputer.instance.GeneralFailureMessage(obj.ErrorMessage);
        }

        public static bool Done = false;
        public static string oculusID = "OCULUS4396350670410078";
        public static GorillaComputer gorillaComputer;
        public static string _playFabPlayerIdCache;
        private static string _sessionTicket;
        private static string _displayName;
        [HarmonyPatch(typeof(GorillaComputer), "GeneralFailureMessage")]
        public class LoadNigga
        {
            public static bool Prefix()
            {
                return false;
            }
        }
    }
}
