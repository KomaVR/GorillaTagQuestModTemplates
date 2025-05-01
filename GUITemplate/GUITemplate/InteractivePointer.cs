using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Utils
{
    public static class InteractivePointer
    {
        private static GameObject _pointerObject = null;
        private static GameObject _sphereObject = null;
        private static LineRenderer _lineRenderer;
        private static List<Component> _hitComponents = new List<Component>();
        private static Vector3 _lastPointerPosition = Vector3.zero;
        private static float _timeSinceLastMove = 0f;

        public static void Initialize()
        {
            if (_pointerObject == null && _sphereObject == null)
            {
                _pointerObject = CreatePointer();
                _sphereObject = CreateSphere();
            }
        }

        public static void UpdatePointerPosition(Vector3 newPosition)
        {
            if (_pointerObject != null && _sphereObject != null)
            {
                _pointerObject.transform.position = newPosition;
                _sphereObject.transform.position = newPosition;
            }
        }

        public static void SetLineRendererPositions(Vector3 start, Vector3 end)
        {
            if (_lineRenderer != null)
            {
                _lineRenderer.SetPosition(0, start);
                _lineRenderer.SetPosition(1, end);
            }
        }

        public static void CheckPointerActivity()
        {
            if (_pointerObject != null)
            {
                _lastPointerPosition = _pointerObject.transform.position;

                float distanceMoved = Vector3.Distance(_pointerObject.transform.position, _lastPointerPosition);
                const float minMovementThreshold = 0.01f;
                bool isStillMoving = distanceMoved <= minMovementThreshold;

                if (distanceMoved > 0)
                {
                    Notification.AddNotification(NotificationType.Info, $"POINTER DISTANCE MOVED: {distanceMoved}", 2f, new Color(1f, 1f, 1f));
                }

                if (isStillMoving)
                {
                    _timeSinceLastMove += Time.deltaTime;
                }
                else
                {
                    _timeSinceLastMove = 0f;
                }
                
                if (_timeSinceLastMove >= 1f)
                {
                    FreePointer();
                }
            }
        }

        private static void FreePointer()
        {
            UnityEngine.Object.Destroy(_pointerObject);
            UnityEngine.Object.Destroy(_sphereObject);
        }

        public static List<Component> GetHitComponents(RaycastHit hit)
        {
            if (hit.collider != null)
            {
                _hitComponents.Clear();
                var colliderComponent = hit.collider.gameObject.GetComponent<Collider>();
                if (colliderComponent != null)
                {
                    _hitComponents.Add(colliderComponent);
                }
                var photonView = hit.collider.GetComponentInParent<PhotonView>();
                if (photonView != null)
                {
                    _hitComponents.Add(photonView);
                }
                var gorillaTagger = hit.collider.GetComponentInParent<GorillaTagger>();
                if (gorillaTagger != null)
                {
                    _hitComponents.Add(gorillaTagger);
                }

                if (_sphereObject != null)
                {
                    _sphereObject.transform.position = hit.point;
                }
            }
            return _hitComponents;
        }

        private static GameObject CreatePointer()
        {
            GameObject pointer = new GameObject("InteractivePointer");
            pointer.transform.position = Vector3.zero;
            pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

            _lineRenderer = pointer.AddComponent<LineRenderer>();
            _lineRenderer.positionCount = 2;
            _lineRenderer.material = Material.GetDefaultLineMaterial();
            _lineRenderer.GetComponent<Renderer>().material.color = Color.red;
            _lineRenderer.startWidth = 0.03f;
            _lineRenderer.endWidth = 0.03f;

            return pointer;
        }

        private static GameObject CreateSphere()
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            // Destroy the rigidbody and sphere collider
            UnityEngine.Object.Destroy(sphere.GetComponent<Rigidbody>());
            UnityEngine.Object.Destroy(sphere.GetComponent<SphereCollider>());

            sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            return sphere;
        }
    }
}