using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using GorillaLocomotion;
using GorillaNetworking;
using easyInputs;
using Photon.Pun;
using Utils;
using OVR.OpenVR;

namespace GUITemplate.Mods
{
    class ModManager
    {
        private static GameObject activePointer = null;
        private static LineRenderer sharedLineRenderer = null;
        private static float holdDuration = 0.5f;
        private static float holdTimer = 0f;
        private static float flyActivationDelay = 0.5f;
        private static float flyHoldTimer = 0f;
        private static bool buttonDownLastFrame = false;

        public static void Fly(int speed)
        {
            if (EasyInputs.GetPrimaryButtonDown(EasyHand.RightHand))
            {
                flyHoldTimer += Time.deltaTime;
                if (flyHoldTimer >= flyActivationDelay)
                {
                    GorillaLocomotion.Player.Instance.playerRigidBody.velocity = Vector3.zero;
                    GorillaLocomotion.Player.Instance.transform.position += GorillaLocomotion.Player.Instance.headCollider.transform.forward * speed * Time.deltaTime;
                }
            }
            else
            {
                flyHoldTimer = 0f;
            }
        }

        public static void SpeedBoost()
        {
            if (PhotonNetwork.IsConnected)
            {
                GorillaTagManager.instance.slowJumpLimit = 8f;
                GorillaTagManager.instance.slowJumpMultiplier = 1.4f;
            }
        }

        public static void NoClip()
        {
            bool secondaryButtonDownCurrentFrame = EasyInputs.GetSecondaryButtonDown(EasyHand.RightHand);

            if (secondaryButtonDownCurrentFrame)
            {
                holdTimer += Time.deltaTime;
            }
            else
            {
                holdTimer = 0f;
            }

            if (holdTimer >= holdDuration && buttonDownLastFrame)
            {
                var colliders = Resources.FindObjectsOfTypeAll<Collider>();

                foreach (var collider in colliders)
                {
                    collider.enabled = !secondaryButtonDownCurrentFrame || !collider.enabled;
                }
                holdTimer = 0f;
            }

            buttonDownLastFrame = secondaryButtonDownCurrentFrame;
        }
        public static void ConnectToRegion(string Region)
        {
            PhotonNetworkController.instance.AttemptDisconnect();
            PhotonNetworkController.instance.ConnectToRegion(Region);
            PhotonNetworkController.instance.InitiateConnection();
        }
       


        public static void TagGun()
        {
            RaycastHit raycastHit;
            Physics.Raycast(GorillaLocomotion.Player.Instance.rightHandTransform.position, -GorillaLocomotion.Player.Instance.rightHandTransform.transform.up, out raycastHit);

            InteractivePointer.Initialize();

            InteractivePointer.UpdatePointerPosition(raycastHit.point);

            InteractivePointer.SetLineRendererPositions(
                GorillaLocomotion.Player.Instance.rightHandTransform.transform.position,
                raycastHit.point
            );

            List<Component> hitComponents = InteractivePointer.GetHitComponents(raycastHit);

            if (!PhotonNetwork.IsMasterClient && EasyInputs.GetTriggerButtonDown(EasyHand.RightHand))
            {
                Notification.AddNotification(NotificationType.System, "WARNING NOT ACTIVE MASTER CANNOT TAG", 2f, new Color(1f, 0f, 0f));
                return;
            }

            if (EasyInputs.GetTriggerButtonDown(EasyHand.RightHand) && hitComponents.Any(comp => comp is PhotonView))
            {
                PhotonView photonView = hitComponents.FirstOrDefault(comp => comp is PhotonView) as PhotonView;

                if (photonView != null)
                {
                    Photon.Realtime.Player owner = photonView.Owner;
                    foreach (GorillaTagManager gorillaTagManager in UnityEngine.Object.FindObjectsOfType<GorillaTagManager>())
                    {
                        Notification.AddNotification(NotificationType.Info, $"PLAYER TAGGED: {owner.NickName}", 2f, new Color(1f, 1f, 1f));
                        gorillaTagManager.AddInfectedPlayer(owner);
                    }
                }
            }
        }
        public static void SetName(string PlayerName)
        {
            PhotonNetwork.LocalPlayer.NickName = PlayerName;
            PhotonNetwork.NickName = PlayerName;
            PhotonNetwork.NetworkingClient.NickName = PlayerName;
            GorillaComputer.instance.currentName = PlayerName;
            GorillaComputer.instance.savedName = PlayerName;
            GorillaComputer.instance.offlineVRRigNametagText.text = PlayerName;
            Player.Instance.name = PlayerName;
            PlayerPrefs.SetString("playerName", PlayerName);
            PlayerPrefs.Save();
        }
        public static void ToggleLongArms()
        {
            if (PhotonNetwork.IsConnected)
            {
                GameObject.Find("GorillaPlayer").transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
            }
        }


        public static void NoLongarms()
        {
            if (PhotonNetwork.IsConnected)
            {
                GameObject.Find("GorillaPlayer").transform.localScale = new Vector3(1f, 1f, 1f);
            }
        }
        public static void PlayerInfo()
        {
            if (EasyInputs.GetGripButtonDown(EasyHand.RightHand))
            {
                RaycastHit raycastHit;
                Physics.Raycast(GorillaLocomotion.Player.Instance.rightHandTransform.position, -GorillaLocomotion.Player.Instance.rightHandTransform.transform.up, out raycastHit);

                string name = "N/A";
                string photonId = "N/A";

                InteractivePointer.Initialize();
                InteractivePointer.UpdatePointerPosition(raycastHit.point);
                InteractivePointer.SetLineRendererPositions(
                    GorillaLocomotion.Player.Instance.rightHandTransform.transform.position,
                    raycastHit.point
                );

                List<Component> hitComponents = InteractivePointer.GetHitComponents(raycastHit);

                if (EasyInputs.GetTriggerButtonDown(EasyHand.RightHand) && hitComponents.Any(comp => comp is PhotonView))
                {
                    PhotonView photonView = hitComponents.FirstOrDefault(comp => comp is PhotonView) as PhotonView;

                    if (photonView != null)
                    {
                        Photon.Realtime.Player owner = photonView.Owner;
                        name = owner.NickName;
                        photonId = owner.UserId;
                    }
                    string message = $"PLAYER NAME: {name}\nPHOTON ID: {photonId}";
                    Notification.AddNotification(NotificationType.Info, message, 2f, new Color(1f, 1f, 0f));
                }
            }
        }
    }
}