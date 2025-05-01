using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
    public enum NotificationType
    {
        System,
        Debug,
        ModStatusChange,
        Info,
        GUIChange
    }

    public class Notification
    {
        public GameObject TextObject { get; private set; }
        public GameObject CanvasObject { get; private set; }
        public NotificationType Type { get; set; }
        public string Message { get; set; }
        public float Duration { get; private set; }
        public float StartTime { get; private set; }
        public bool IsDisappearing { get; private set; } = false;
        public static int MaxAmountOfNotifications = 15;
        private static int nextNotificationId = 0;
        public static Canvas SharedCanvas;
        public static float CooldownBetweenNotifications = 0.3f;
        private static float NextAllowedNotificationTime = 0f;

        public Notification(NotificationType type, string message, float duration)
        {
            Type = type;
            Message = message;
            Duration = duration;
            StartTime = Time.time;
        }

        public static void InitializeSharedCanvas()
        {
            if (SharedCanvas == null)
            {
                SharedCanvas = new GameObject("SharedCanvas").AddComponent<Canvas>();
                SharedCanvas.renderMode = RenderMode.WorldSpace;
                SharedCanvas.transform.SetParent(GameObject.Find("Main Camera").transform, false);
            }
        }

        public static void AddNotification(NotificationType type, string message, float duration, Color color)

        {
            if (Time.time >= NextAllowedNotificationTime)
            {
                Notification newNotification = new Notification(type, message, duration);
                newNotification.CreateNotification(color);
                StackedNotifications.Insert(0, newNotification);
                NextAllowedNotificationTime = Time.time + CooldownBetweenNotifications;
            }
        }

        public GameObject CreateNotification(Color color)
        {
            CanvasObject = SharedCanvas.gameObject;
            TextObject = new GameObject($"NotificationText_{nextNotificationId}");
            TextObject.AddComponent<Text>();
            TextObject.AddComponent<ContentSizeFitter>();
            TextObject.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            string formattedMessage = $"[{Type.ToString().ToUpper()}] {Message}";
            TextObject.GetComponent<Text>().text = formattedMessage;
            TextObject.GetComponent<Text>().fontSize = 12;
            TextObject.GetComponent<Text>().color = color;
            TextObject.GetComponent<Text>().alignment = TextAnchor.UpperLeft;
            TextObject.GetComponent<Text>().font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            RectTransform rectTransform = TextObject.GetComponent<RectTransform>();
            rectTransform.localPosition = new Vector3(0.16f, 0f, 0.4f);
            rectTransform.localScale = new Vector3(0.0008f, 0.0008f, 0.0008f);
            rectTransform.sizeDelta = new Vector2(rectTransform.rect.width * 1.5f, rectTransform.rect.height * 1.5f);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            CanvasObject.transform.SetParent(GameObject.Find("Main Camera").transform, false);
            TextObject.transform.SetParent(CanvasObject.transform, false);
            int index = StackedNotifications.Count;
            float offsetY = -0.05f + (index * 0.028f);
            rectTransform.transform.localPosition = new Vector3(-0.15f, offsetY, 0.4f);

            nextNotificationId++;
            return TextObject;
        }

        private static List<Notification> StackedNotifications = new List<Notification>();

        public static void UpdateNotifications(float currentTime, float deltaTime)
        {
            foreach (var notification in StackedNotifications.ToList())
            {
                if (notification.StartTime <= currentTime && currentTime < notification.StartTime + notification.Duration)
                {
                    notification.ShowNotification(currentTime, deltaTime);
                }
                else if (currentTime >= notification.StartTime + notification.Duration)
                {
                    notification.Destroy();
                    StackedNotifications.Remove(notification);
                }
            }
        }

        public void ShowNotification(float currentTime, float deltaTime)
        {
            if (currentTime >= StartTime)
            {
                float elapsedTimeSinceStart = currentTime - StartTime;
                float fadeDuration = Duration;
                
                float progress = Mathf.Clamp01(elapsedTimeSinceStart / fadeDuration);
                TextObject.GetComponent<Text>().color = new Color(TextObject.GetComponent<Text>().color.r, TextObject.GetComponent<Text>().color.g, TextObject.GetComponent<Text>().color.b, progress);
                
                float startPositionY = 0f;
                float endPositionY = -0.15f;
                float newPositionY = Mathf.Lerp(startPositionY, endPositionY, progress);
                TextObject.GetComponent<RectTransform>().localPosition = new Vector3(TextObject.GetComponent<RectTransform>().localPosition.x, newPositionY, TextObject.GetComponent<RectTransform>().localPosition.z);
            }
        }

        private void Destroy()
        {
            float startPositionY = TextObject.GetComponent<RectTransform>().localPosition.y;
            float endPositionY = startPositionY + -0.2f;
            float duration = 0.5f;
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float newY = Mathf.Lerp(startPositionY, endPositionY, elapsedTime / duration);
                TextObject.GetComponent<RectTransform>().localPosition = new Vector3(TextObject.GetComponent<RectTransform>().localPosition.x, newY, TextObject.GetComponent<RectTransform>().localPosition.z);
                
                if (newY >= endPositionY)
                {
                    UnityEngine.Object.Destroy(TextObject.gameObject);
                    break;
                }
            }
        }
    }
}
