using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GorillaLocomotion;
using easyInputs;
using Photon.Pun;
using Util;

namespace HandHeldTemplate.Mods
{
    class ModManager
    {
        private static GameObject activePointer = null;
        private static LineRenderer sharedLineRenderer = null;
        private static float holdDuration = 0.2f;
        private static float holdTimer = 0f;
        private static float flyActivationDelay = 0.2f;
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
            if (PhotonNetwork.InRoom)
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

        public static void TagGunMaster()
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

            // Dont force master as it will be detected
            if (!PhotonNetwork.IsMasterClient && EasyInputs.GetTriggerButtonDown(EasyHand.RightHand))
            {
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
                        gorillaTagManager.AddInfectedPlayer(owner);
                    }
                }
            }
        }

        public static void TagGunNoMaster()
        {
            RaycastHit raycastHit;
            Physics.Raycast(GorillaLocomotion.Player.Instance.rightHandTransform.position, -GorillaLocomotion.Player.Instance.rightHandTransform.transform.up, out raycastHit);

            InteractivePointer.Initialize();

            InteractivePointer.UpdatePointerPosition(raycastHit.point);

            InteractivePointer.SetLineRendererPositions(
                GorillaLocomotion.Player.Instance.rightHandTransform.transform.position,
                raycastHit.point
            );

            if (EasyInputs.GetTriggerButtonDown(EasyHand.RightHand) && raycastHit.collider != null)
            {
                Player.Instance.rightHandTransform.position = raycastHit.point;
            }
        }
    }
}
