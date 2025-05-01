using System.Collections.Generic;
using MelonLoader;
using Utils;
using UnityEngine;
using easyInputs;
using Photon.Pun;
using GorillaNetworking;
using PlayFab;
using System.IO;
using GorillaLocomotion;
using PlayFab.ClientModels;

namespace GUITemplate
{
    public class Main : MelonMod
    {
        public static bool delay = false;
        public static float delayTime = 0.2f;
        private float smoothSpeed = 0.8f;
        private Vector2 currentPos = new Vector2(0, 0);
        public override void OnInitializeMelon()
        {
          
            base.OnInitializeMelon();

            PageInfo navigationPageInfo = new PageInfo("Harmony.lol", 0, PageType.Navigation, new string[]
            { "Fly", "No Name", "SpeedBoost", "Grab Player Info Gun", "Connect to Europe", "Connect to USW", "Connect to US", "Connect To Region <color=yellow>[PLAYER NAME]</color>", "Disconnect", "Join Random Public", "Set Master <b><color=red>[D?]</color></b>" });
            PageInfo navigation2PageInfo = new PageInfo("Harmony.lol", 0, PageType.Navigation, new string[]
           { "" });
            List<PageInfo> pageInfoList = new List<PageInfo>
            {
                navigationPageInfo,
                navigation2PageInfo,
            };

            ModUtils.InitializePages(pageInfoList);

            Notification.InitializeSharedCanvas();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            float currentTime = Time.time;
            float deltaTime = Time.deltaTime;
            Notification.UpdateNotifications(currentTime, deltaTime);
            ManageMods();
            if (delay)
            {
                delayTime -= Time.deltaTime;
                if (delayTime <= 0)
                {
                    delay = false;
                }
            }
            else
            {
                if (ModUtils.IsGuiEnabled)
                {
                    HandleUserInput();
                }
                bool menuButtonDown = EasyInputs.GetMenuButtonDown(EasyHand.LeftHand);
                if (menuButtonDown)
                {
                    ToggleGUIState();
                }
            }
        }

        private void HandleUserInput()
        {
            currentPos = Vector2.Lerp(currentPos, new Vector2(0, EasyInputs.GetThumbStick2DAxis(EasyHand.RightHand).y), smoothSpeed);
            bool changeModDown = currentPos.y >= -0.5;
            bool changeModUp = currentPos.y <= 0.5;
            bool primaryLeftButton = EasyInputs.GetPrimaryButtonDown(EasyHand.LeftHand);

            if (changeModUp)
            {
                NavigateRight();
            }
            if (changeModDown)
            {
                NavigateLeft();
            }
            if (primaryLeftButton)
            {
                ToggleModState();
            }
        }

        private void NavigateRight()
        {
            Delay(0.2f);
            ModUtils.NextMod();
        }

        private void NavigateLeft()
        {
            Delay(0.2f);
            ModUtils.PreviousMod();
        }

        private void ToggleModState()
        {
            Delay(0.2f);
            ModUtils.ToggleActiveMod();
        }

        private void ToggleGUIState()
        {
            Delay(0.2f);
            ModUtils.ToggleGUI();
        }

        private void ManageMods()
        {
            if (ModUtils.IsModEnabled("Fly"))
            {
                Mods.ModManager.Fly(25);
            }
            if (ModUtils.IsModEnabled("No Name"))
            {
                Mods.ModManager.SetName("  ");
            }
            if (ModUtils.IsModEnabled("SpeedBoost"))
            {
                Mods.ModManager.SpeedBoost();
            }
            if (ModUtils.IsModEnabled("Grab Player Info Gun"))
            {
                Mods.ModManager.PlayerInfo();
            }
            if (ModUtils.IsModEnabled("Connect to Europe"))
            {
                string USW = "EU";
                PhotonNetwork.ConnectToBestCloudServer();
                PhotonNetwork.ConnectToRegion(USW);
                PhotonNetworkController.instance.InitiateConnection();
            }
            if (ModUtils.IsModEnabled("Connect to US"))
            {
                string USW = "US";
                PhotonNetwork.ConnectToBestCloudServer();
                PhotonNetwork.ConnectToRegion(USW);
                PhotonNetworkController.instance.InitiateConnection();
            }
            if (ModUtils.IsModEnabled("Connect to USW"))
            {
                string USW = "USW";
                PhotonNetwork.ConnectToBestCloudServer();
                PhotonNetwork.ConnectToRegion(USW);
                PhotonNetworkController.instance.InitiateConnection();
            }
            if (ModUtils.IsModEnabled("Connect To Region <color=yellow>[PLAYER NAME]</color>"))
            {
                PhotonNetworkController.instance.ConnectToRegion(PhotonNetwork.LocalPlayer.nickName);
                PhotonNetworkController.instance.InitiateConnection();
            }

            if (ModUtils.IsModEnabled("Disconnect"))
            {
                    PhotonNetwork.Disconnect();
                }
                if (ModUtils.IsModEnabled("Join Random Public"))
                {
                    PhotonNetwork.JoinRandomRoom();
                }
            
            if (ModUtils.IsModEnabled("Set Master <b><color=red>[D?]</color></b>"))
            {
                    PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
                }
            }
        
      
      
        private void Delay(float delayAmount)
        {
            delay = true;
            delayTime = delayAmount;
        }
    }
}
