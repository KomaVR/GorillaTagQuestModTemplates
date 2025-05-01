using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using GorillaLocomotion;
using MelonLoader;

namespace Util
{
    class MenuHandler
    {
        public static Page currentPage = null;
        public static List<Page> Pages = new List<Page>();
        public static List<string> modNames = new List<string>();
        public static List<bool> modStates = new List<bool>();

        public static void InitializePages(List<PageInfo> pageInfoList)
        {
            Pages.Clear();
            modStates.Clear();

            foreach (var pageInfo in pageInfoList)
            {
                var page = new Page(pageInfo.Name, pageInfo.Mods);
                Pages.Add(page);
                foreach (string modName in pageInfo.Mods)
                {
                    modNames.Add(modName);
                }
            }
            
            if (Pages.Count > 1)
            {
                for (int i = 1; i < Pages.Count; i++)
                {
                    Pages[i].DisableCurrentPage();
                }
            }

            if (Pages.Count > 0)
            {
                SetCurrentPage(Pages[0]);
            }
        }

        public static void DisableMenu()
        {
            currentPage.DisableCurrentPage();
        }

        public static void EnableMenu()
        {
            currentPage.EnableCurrentPage();
        }

        public static void UpdateMenuPosition()
        {
            Transform hand = Player.Instance.leftHandTransform;
            currentPage.UpdateMenuPosition(hand);
        }

        public static void SetCurrentPage(Page page)
        {
            currentPage = page;
        }

        public static bool IsModEnabled(string modName)
        {
            int index = modNames.IndexOf(modName);
            return modStates[index];
        }

        public static Page GetCurrentPage()
        {
            return currentPage;
        }

        public static Page GetNextPage(bool moveForward = true)
        {
            int currentIndex = Pages.IndexOf(currentPage);
            int newIndex;

            if (moveForward)
            {
                if (currentIndex < Pages.Count - 1)
                {
                    newIndex = currentIndex + 1;
                }
                else
                {
                    newIndex = 0;
                }
            }
            else
            {
                if (currentIndex > 0)
                {
                    newIndex = currentIndex - 1;
                }
                else
                {
                    newIndex = Pages.Count - 1;
                }
            }

            if (newIndex >= 0 && newIndex < Pages.Count)
            {
                if (currentPage != null)
                {
                    currentPage.DisableCurrentPage();
                }
                
                Pages[newIndex].EnableCurrentPage();

                return Pages[newIndex];
            }
            else
            {
                Debug.LogError("Invalid page index encountered.");
                return null;
            }
        }
    }

    [RegisterTypeInIl2Cpp]
    class Button : MonoBehaviour
    {
        public string Name;
        public GameObject buttonObject;
        public bool state;
        public string modName;
        public Page ParentPage;
        public bool pressDelay = false;
        public float timeDelay = 0.2f;

        public Button (IntPtr ptr) : base(ptr) { }
        public Button () { }

        public void OnTriggerEnter(Collider other)
        {
            if (pressDelay)
            {
                timeDelay -= 0.1f;
                if (timeDelay <= 0)
                {
                    pressDelay = false;
                    timeDelay = 0.2f;
                }
            }
            if (!pressDelay)
            {
                var gt = GorillaTagger.Instance;
                gt.StartVibration(false, gt.taggedHapticStrength / 2, gt.taggedHapticDuration);
                if (modName == "<")
                {
                    Page previousPage = MenuHandler.GetNextPage(moveForward: false);
                    if (previousPage != null)
                    {
                        MenuHandler.SetCurrentPage(previousPage);
                        state = false;
                    }
                }
                else if (modName == ">")
                {
                    Page nextPage = MenuHandler.GetNextPage();
                    if (nextPage != null)
                    {
                        MenuHandler.SetCurrentPage(nextPage);
                        state = false;
                    }
                }
                else
                {
                    ParentPage.ToggleModStateAndColor(modName);
                }
                pressDelay = true;
                timeDelay = 0.2f;
            }
        }
    }

    // This is for easier page creation
    class PageInfo
    {
        public string Name; // The name of the page used in the title text
        public string[] Mods; // A list of all mods for the page

        public PageInfo(string name, string[] mods)
        {
            Name = name;
            Mods = mods;
        }
    }

    class Page
    {
        public List<GameObject> Pages = new List<GameObject>(); // A list of the page objects for later disabling of inactive pages
        public List<Button> Buttons = new List<Button>(); // A list of the buttons for later use
        public List<GameObject> ButtonObjects = new List<GameObject>(); // A list of the button GameObjects
        public List<string> Mods = new List<string>();
        public Dictionary<string, Button> ModNameToButtonMap = new Dictionary<string, Button>();
        public Dictionary<Button, bool> ButtonState = new Dictionary<Button, bool>(); // A list of the button states for detecting if the button is enabled
        public GameObject MenuObject = null;
        public string ForwardButtonName = "<"; // Default forward navigation button name
        public string BackwardButtonName = ">"; // Default backward navigation button name
        public GameObject SharedCanvas = null;
        public GameObject titleObject = null;
        public string Title;

        public Page(string title, string[] mods)
        {
            // Dynamically create the page and the associated buttons with the text
            if (MenuObject == null)
            {
                MenuObject = CreateMenu();
            }
            InitializeSharedCanvas();
            foreach (var modName in mods)
            {
                if (!Mods.Contains(modName))
                {
                    Mods.Add(modName);
                }
                CreateButton(modName);
            }
            Title = title;
            CreateNavigationButtons();
        }

        private void CreateNavigationButtons()
        {
            GameObject forwardNavBtn = CreateButton(ForwardButtonName);
            GameObject backwardNavBtn = CreateButton(BackwardButtonName);
        }

        private GameObject CreateMenu()
        {
            GameObject menu = GameObject.CreatePrimitive(PrimitiveType.Cube);
            menu.transform.localScale = new Vector3(0.04f, 0.35f, 0.53f);
            menu.transform.position = Player.Instance.leftHandTransform.position + new Vector3(0, 0, -1f);
            menu.transform.rotation = Player.Instance.leftHandTransform.rotation;
            UnityEngine.Object.Destroy(menu.GetComponent<BoxCollider>());
            menu.GetComponent<Renderer>().material.SetColor("_Color", Color.blue);
            return menu;
        }

        public GameObject CreateButton(string modName)
        {
            // Create the button GameObject
            GameObject btn = GameObject.CreatePrimitive(PrimitiveType.Cube);
            btn.transform.localScale = new Vector3(0.08f, 0.6f, 0.08f);

            // Ensure the button has a Collider component and set it as a trigger
            Collider btnColl = btn.GetComponent<Collider>();
            if (btnColl != null)
            {
                btnColl.isTrigger = true; // So OnTriggerEnter can be used
            }
            btn.layer = 18;
            
            float Offset = Buttons.Count * 0.06f;
            Vector3 btnOffset = new Vector3(-0.15f + Offset, 0, 0.03f);
            btn.transform.SetParent(MenuObject.transform, false);
            btn.transform.position = MenuObject.transform.position + btnOffset;
            btn.transform.rotation = MenuObject.transform.rotation;
            btn.GetComponent<Renderer>().material.SetColor("_Color", Color.grey);

            Button buttonComponent = btn.AddComponent<Button>();
            buttonComponent.Name = modName;
            buttonComponent.buttonObject = btn;
            buttonComponent.modName = modName;
            buttonComponent.ParentPage = this;

            ModNameToButtonMap[modName] = buttonComponent;
            MenuHandler.modStates.Add(false);
            Buttons.Add(buttonComponent);
            ButtonObjects.Add(btn);

            ButtonState[buttonComponent] = false;

            Vector3 textOffset = new Vector3(-0.15f + Offset + 0.03f, 0, 0.032f);
            GameObject textObj = new GameObject($"{modName}_Text");
            textObj.transform.SetParent(SharedCanvas.transform, false);
            textObj.transform.transform.position = MenuObject.transform.position + textOffset;
            textObj.transform.rotation = MenuObject.transform.rotation;

            Text buttonText = textObj.AddComponent<Text>();
            buttonText.text = (modName != "" || modName != null) ? modName : "N/A";
            buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            buttonText.fontSize = 12;
            buttonText.resizeTextForBestFit = true;

            RectTransform textRectTransform = textObj.GetComponent<RectTransform>();

            textRectTransform.anchorMin = new Vector2(0, 0);
            textRectTransform.anchorMax = new Vector2(1, 1);
            textRectTransform.pivot = new Vector2(0.5f, 0.5f);

            textRectTransform.position = MenuObject.transform.position + textOffset;
            Quaternion newRotation = btn.transform.rotation * Quaternion.Euler(180f, 90f, 90f);
            textRectTransform.rotation = newRotation;
            textRectTransform.sizeDelta = new Vector2(0.2f, 0.03f);
            textRectTransform.localScale = new Vector3(0.002f, 0.002f, 0.002f);
            CreateTitle();
            return btn;
        }

        private void CreateTitle()
        {
            if (titleObject == null)
            {
                Vector3 textOffset = new Vector3(-0.34f, 0, 0.031f);
                GameObject textObj = new GameObject($"{Title}_Text");
                textObj.transform.SetParent(SharedCanvas.transform, false);
                textObj.transform.transform.position = MenuObject.transform.position + textOffset;
                textObj.transform.rotation = MenuObject.transform.rotation;

                Text buttonText = textObj.AddComponent<Text>();
                ContentSizeFitter sizeFitter = textObj.AddComponent<ContentSizeFitter>();
                buttonText.text = (Title != "" || Title != null) ? Title : "N/A";
                buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                buttonText.fontSize = 64;
                buttonText.resizeTextForBestFit = false;
                buttonText.alignment = TextAnchor.MiddleCenter;
                sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

                RectTransform textRectTransform = textObj.GetComponent<RectTransform>();
                textRectTransform.anchorMin = new Vector2(0, 0);
                textRectTransform.anchorMax = new Vector2(1, 1);
                textRectTransform.pivot = new Vector2(0.5f, 0.5f);
                textRectTransform.position = MenuObject.transform.position + textOffset;
                Quaternion newRotation = MenuObject.transform.rotation * Quaternion.Euler(180f, 90f, 90f);
                textRectTransform.rotation = newRotation;
                textRectTransform.sizeDelta = new Vector2(0.6f, 0.1f);
                textRectTransform.localScale = new Vector3(0.0024f, 0.0024f, 0.0024f);
            }
        }

        private void InitializeSharedCanvas()
        {
            if (SharedCanvas == null)
            {
                SharedCanvas = new GameObject("SharedCanvas");
                Canvas canvas = SharedCanvas.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                SharedCanvas.transform.SetParent(MenuObject.transform, false);
                SharedCanvas.transform.position = MenuObject.transform.position;
                SharedCanvas.transform.rotation = MenuObject.transform.rotation;
            }
        }
        
        public void UpdateMenuPosition(Transform hand)
        {
            if(MenuObject != null)
            {
                MenuObject.transform.position =  hand.position;
                MenuObject.transform.rotation = hand.rotation;
            }
        }

        public void EnableCurrentPage()
        {
            if (MenuObject != null)
            {
                MenuObject.SetActive(true);
            }
            foreach (GameObject pageObj in Pages)
            {
                pageObj.SetActive(true);
            }
            foreach (GameObject buttonObject in ButtonObjects)
            {
                if (buttonObject != null)
                {
                    buttonObject.SetActive(true);
                }
            }
        }

        public void DisableCurrentPage()
        {
            if(MenuObject != null)
            {
                MenuObject.SetActive(false);
            }
            foreach (GameObject pageObj in Pages)
            {
                pageObj.SetActive(false);
            }
            foreach (GameObject buttonObject in ButtonObjects)
            {
                if (buttonObject != null)
                {
                    buttonObject.SetActive(false);
                }
            }
        }

        public void ToggleModStateAndColor(string modName)
        {
            // Find the button associated with the modName
            Button targetButton = Buttons.FirstOrDefault(b => b.modName == modName);
            if (targetButton != null)
            {
                // Toggle the state
                targetButton.state = !targetButton.state;
                ButtonState[targetButton] = !ButtonState[targetButton];
                // Get the index of the mod button to toggle the bool in the menu handler
                int modIndex = MenuHandler.modNames.IndexOf(modName);
                if (modIndex != -1)
                {
                    MenuHandler.modStates[modIndex] = !MenuHandler.modStates[modIndex];
                }
                Color originalColor = targetButton.buttonObject.GetComponent<Renderer>().material.color = Color.magenta;
                Color newColor = originalColor == Color.red ? Color.magenta : Color.red;
                targetButton.buttonObject.GetComponent<Renderer>().material.SetColor("_Color", newColor);
            }
        }
    }
}
