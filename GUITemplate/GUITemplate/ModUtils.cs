using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
    class ModUtils
    {
        public static List<Page> Pages { get; private set; } = new List<Page>();
        public static int CurrentPageIndex { get; set; } = 0;
        public static int CurrentModIndex { get; private set; } = 0;
        public static Dictionary<Page, string[]> pageMods = new Dictionary<Page, string[]>();
        public static Dictionary<string, bool> modState = new Dictionary<string, bool>();
        public static bool IsGuiEnabled { get; private set; } = true;
        public static Page CurrentPage;

        public static void InitializePages(List<PageInfo> pageInfoList)
        {
            if (GameObject.Find("Main Camera") == null)
            {
                Debug.LogError("Main Camera not found! Cannot initialize pages.");
                return;
            }

            Pages.Clear();

            foreach (var pageInfo in pageInfoList)
            {
                foreach (var mod in pageInfo.Mods)
                {
                    modState[mod] = false;
                }

                bool addNavMods = pageInfo.Type == PageType.Standard;

                string[] navMods = { "NextPage", "PreviousPage" };

                var combinedMods = pageInfo.Mods.Concat(addNavMods ? navMods : Array.Empty<string>()).ToArray();

                var page = new Page(pageInfo.Title, pageInfo.Id, pageInfo.Type, combinedMods);

                pageMods[page] = combinedMods;

                Pages.Add(page);
            }

            if (Pages.Count > 1)
            {
                for (int i = 1; i < Pages.Count; i++)
                {
                    Pages[i].Disable();
                }
            }

            NavigateTo(0);
        }

        public static void NavigateTo(int pageIndex)
        {
            if (pageIndex >= 0 && pageIndex < Pages.Count)
            {
                foreach (var page in Pages)
                {
                    page.Disable();
                }

                Pages[pageIndex].Enable();

                CurrentPageIndex = pageIndex;
                CurrentPage = Pages[pageIndex];
                UpdateActiveModsBasedOnCurrentPageIndex();
            }
        }

        public static void NextPage()
        {
            bool validPageFound = false;

            for (int i = 0; i < Pages.Count - 1; i++)
            {
                int nextPageIndex = (CurrentPageIndex + 1) % Pages.Count;
                PageType nextPageType = Pages[nextPageIndex].PageType;

                if (nextPageType != PageType.Navigation)
                {
                    Pages[CurrentPageIndex].Disable();
                    CurrentPageIndex = nextPageIndex;
                    CurrentPage = Pages[CurrentPageIndex];
                    Pages[CurrentPageIndex].Enable();

                    if (pageMods.TryGetValue(CurrentPage, out string[] modsForCurrentPage))
                    {
                        Pages[CurrentPageIndex].UpdateMods(modsForCurrentPage);
                    }
                    validPageFound = true;
                    break;
                }
            }

            if (!validPageFound)
            {
                int smallestValidIndex = Pages.Count;

                for (int i = 0; i < Pages.Count; i++)
                {
                    int currentIndex = (i + CurrentPageIndex) % Pages.Count;
                    PageType pageType = Pages[currentIndex].PageType;

                    if (pageType != PageType.Navigation && currentIndex < smallestValidIndex)
                    {
                        smallestValidIndex = currentIndex;
                    }
                }

                if (smallestValidIndex != Pages.Count)
                {
                    Pages[CurrentPageIndex].Disable();
                    CurrentPageIndex = smallestValidIndex;
                    CurrentPage = Pages[CurrentPageIndex];
                    Pages[CurrentPageIndex].Enable();

                    if (pageMods.TryGetValue(CurrentPage, out string[] modsForCurrentPage))
                    {
                        Pages[CurrentPageIndex].UpdateMods(modsForCurrentPage);
                    }
                    validPageFound = true;
                }
                else
                {
                    Debug.LogError("No valid page found to navigate to.");
                }
            }
        }

        public static void PreviousPage()
        {
            bool validPageFound = false;

            for (int i = Pages.Count - 1; i > 0; i--)
            {
                int prevPageIndex = (CurrentPageIndex - 1 + Pages.Count) % Pages.Count;
                PageType prevPageType = Pages[prevPageIndex].PageType;

                if (prevPageType != PageType.Navigation)
                {
                    Pages[CurrentPageIndex].Disable();
                    CurrentPageIndex = prevPageIndex;
                    CurrentPage = Pages[CurrentPageIndex];
                    Pages[CurrentPageIndex].Enable();

                    if (pageMods.TryGetValue(CurrentPage, out string[] modsForCurrentPage))
                    {
                        Pages[CurrentPageIndex].UpdateMods(modsForCurrentPage);
                    }
                    validPageFound = true;
                    break;
                }
            }

            if (!validPageFound)
            {
                for (int i = Pages.Count - 1; i >= 0; i--)
                {
                    int pageIndex = (i + CurrentPageIndex) % Pages.Count;
                    PageType pageType = Pages[pageIndex].PageType;

                    if (pageType == PageType.Navigation)
                    {
                        pageIndex = (pageIndex - 1 + Pages.Count) % Pages.Count;
                    }

                    PageType prevPageType = Pages[pageIndex].PageType;
                    if (prevPageType != PageType.Navigation)
                    {
                        Pages[CurrentPageIndex].Disable();
                        CurrentPageIndex = pageIndex;
                        CurrentPage = Pages[CurrentPageIndex];
                        Pages[CurrentPageIndex].Enable();

                        if (pageMods.TryGetValue(CurrentPage, out string[] modsForCurrentPage))
                        {
                            Pages[CurrentPageIndex].UpdateMods(modsForCurrentPage);
                        }
                        validPageFound = true;
                        break;
                    }
                }
            }

            if (!validPageFound)
            {
                Debug.LogError("No valid page found to navigate to.");
            }
        }

        public static void GoToPageById(int id)
        {
            if (id >= 0 && id < Pages.Count)
            {
                CurrentPage.Disable();
                Pages[id].Enable();
                CurrentPageIndex = id;
                CurrentPage = Pages[id];
                UpdateActiveModsBasedOnCurrentPageIndex();
            }
            else
            {
                Debug.LogError($"No page found with ID: {id}");
            }
        }

        public static void NavigateWithMod(string modName)
        {
            if (modName == "NextPage")
            {
                NextPage();
            }
            else if (modName == "PreviousPage")
            {
                PreviousPage();
            }
        }

        public static void UpdateActiveModsBasedOnCurrentPageIndex()
        {
            if (CurrentPageIndex >= 0 && CurrentPageIndex < Pages.Count)
            {
                if (pageMods.TryGetValue(CurrentPage, out string[] modsForCurrentPage))
                {
                    Pages[CurrentPageIndex].UpdateMods(modsForCurrentPage);
                }
            }
        }

        public static void ToggleGUI()
        {
            IsGuiEnabled = !IsGuiEnabled;
            Pages[CurrentPageIndex].ToggleGUIVisibility(IsGuiEnabled);
            string message = $"GUI IS NOW: {(IsGuiEnabled ? "ENABLED" : "DISABLED")}";
            Notification.AddNotification(NotificationType.GUIChange, message, 2f, new Color(1f, 1f, 1f));
        }

        public static bool IsModEnabled(string modName)
        {
            return modState[modName];
        }

        public static void NextMod()
        {
            if (CurrentPageIndex >= 0 && CurrentPageIndex < Pages.Count)
            {
                Pages[CurrentPageIndex].NextModInCurrentPage();
                if (pageMods.TryGetValue(CurrentPage, out string[] modsForCurrentPage))
                {
                    CurrentPage.UpdateMods(modsForCurrentPage);
                }
            }
        }

        public static void PreviousMod()
        {
            if (CurrentPageIndex >= 0 && CurrentPageIndex < Pages.Count)
            {
                Pages[CurrentPageIndex].PreviousModInCurrentPage();
                if (pageMods.TryGetValue(CurrentPage, out string[] modsForCurrentPage))
                {
                    Pages[CurrentPageIndex].UpdateMods(modsForCurrentPage);
                }
            }
        }

        public static void ToggleActiveMod()
        {
            if (CurrentPageIndex >= 0 && CurrentPageIndex < Pages.Count)
            {
                Pages[CurrentPageIndex].ToggleActiveMod();
            }
        }

        public static void DisableModByName(string name)
        {
            foreach (var page in Pages)
            {
                page.DisableModByName(name);
            }
        }

        public static void DisableCurrentMod()
        {
            if (CurrentPageIndex >= 0 && CurrentPageIndex < Pages.Count)
            {
                Pages[CurrentPageIndex].DisableActiveMod();
            }
        }
    }

    public enum ModType
    {
        Navigation,
        Toggle
    }

    public class PageInfo
    {
        public string Title { get; set; }
        public string[] Mods { get; set; }
        public PageType Type { get; set; }
        public int Id { get; set; }

        public PageInfo(string title, int id, PageType type, params string[] mods)
        {
            Title = title;
            Mods = mods;
            Type = type;
            Id = id;
        }
    }

    public enum PageType
    {
        Standard,
        Navigation
    }

    public class Page
    {
        public GameObject CanvasObject { get; private set; }
        public GameObject TextObject { get; private set; }
        public PageType PageType { get; private set; }
        public int PageId { get; private set; }
        public List<string> Titles { get; private set; } = new List<string>();
        public List<string> Mods { get; private set; } = new List<string>();
        public List<bool> ModSelectionStates { get; private set; } = new List<bool>();
        public int selectedIndex = 0;
        private string GUITitle = "Fallback Name";

        public Page(string title, int id, PageType type = PageType.Standard, params string[] mods)
        {
            PageType = type;
            PageId = id;
            GUITitle = title;
            Titles.Add(title.Replace(" ", "_"));

            string uniqueTitle = Titles.Last() + "_" + Titles.Count;
            CanvasObject = new GameObject(uniqueTitle + "Canvas");
            CanvasObject.AddComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            TextObject = new GameObject(Titles.First() + "Text");
            TextObject.AddComponent<Text>();
            TextObject.AddComponent<ContentSizeFitter>();
            TextObject.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            TextObject.GetComponent<Text>().text = "LOADING...";
            TextObject.GetComponent<Text>().fontSize = 12;
            TextObject.GetComponent<Text>().alignment = TextAnchor.UpperLeft;
            TextObject.GetComponent<Text>().font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            TextObject.transform.SetParent(CanvasObject.transform, false);

            SetUiPos();

            foreach (var mod in mods)
            {
                if (!Mods.Contains(mod))
                {
                    Mods.Add(mod);
                }
            }

            selectedIndex = 0;
            UpdateMods(Mods.ToArray());
        }

        public void Disable()
        {
            CanvasObject.SetActive(false);
            TextObject.SetActive(false);
        }

        public void Enable()
        {
            CanvasObject.SetActive(true);
            TextObject.SetActive(true);
        }

        public bool IsLastMod()
        {
            return selectedIndex == Mods.Count - 1;
        }

        private void SetUiPos()
        {
            CanvasObject.transform.SetParent(GameObject.Find("Main Camera").transform, false);

            RectTransform textRectTransform = TextObject.GetComponent<RectTransform>();

            float originalTextWidth = textRectTransform.rect.width;
            textRectTransform.sizeDelta = new Vector2(originalTextWidth * 1.5f, textRectTransform.sizeDelta.y);

            textRectTransform.anchorMin = new Vector2(0, 0);
            textRectTransform.anchorMax = new Vector2(1, 1);
            textRectTransform.pivot = new Vector2(0.5f, 0.5f);

            textRectTransform.localPosition = new Vector3(0.16f, 0f, 0.4f);
            textRectTransform.localRotation = Quaternion.identity;
            textRectTransform.localScale = new Vector3(0.0008f, 0.0008f, 0.0008f);
        }

        public void UpdateMods(string[] mods)
        {
            Text text = TextObject.GetComponent<Text>();
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"{GUITitle}");

            for (int i = 0; i < mods.Length; i++)
            {
                string modName = mods[i];
                bool isSelected = IsModEnabled(modName);

                bool isNavigationMod = modName == "NextPage" || modName == "PreviousPage";

                string selectionIndicator = !isNavigationMod ? (isSelected ? "<b><color=green>+</color></b>" : "<b><color=red>-</color></b>") : "";
                string navigationIndicator = selectedIndex == i ? ">" : "";

                string line = $"{navigationIndicator} {selectionIndicator} {modName,-30}";

                sb.AppendLine(line);
            }

            text.text = sb.ToString();
        }


        public bool IsModEnabled(string modName)
        {
            if (ModUtils.modState.ContainsKey(modName))
            {
                return ModUtils.modState[modName];
            }
            else
            {
                Debug.LogError($"Mod '{modName}' not found in modState.");
                return false;
            }
        }

        public void DisableModByName(string name)
        {
            ModUtils.modState[name] = false;
        }

        public void NextModInCurrentPage()
        {
            selectedIndex = (selectedIndex + 1) % Mods.Count;
            UpdateMods(ModUtils.pageMods[this]);
        }

        public void PreviousModInCurrentPage()
        {
            selectedIndex = (selectedIndex - 1 + Mods.Count) % Mods.Count;
            UpdateMods(ModUtils.pageMods[this]);
        }

        public void DisableIfModMatches(string modName)
        {
            if (IsModEnabled(modName))
            {
                int modIndex = Mods.IndexOf(modName);
                if (modIndex != -1)
                {
                    ModSelectionStates[modIndex] = !ModSelectionStates[modIndex];
                    UpdateMods(ModUtils.pageMods[this]);
                }
            }
        }

        public void ToggleActiveMod()
        {
            if (selectedIndex >= 0 && selectedIndex < Mods.Count)
            {
                string modName = Mods[selectedIndex];
                if (modName == "NextPage" || modName == "PreviousPage")
                {
                    ModUtils.NavigateWithMod(modName);
                    return;
                }
                else
                {
                    bool newState = !ModUtils.modState[modName];
                    ModUtils.modState[modName] = newState;

                    string message = $"{modName} IS NOW {(newState ? "[ENABLED]" : "[DISABLED]")}";

                    Notification.AddNotification(NotificationType.ModStatusChange, message, 2f, new Color(1f, 1f, 1f));
                }

                UpdateMods(ModUtils.pageMods[this]);
            }
        }

        public void ToggleGUIVisibility(bool IsGuiEnabled)
        {
            CanvasObject.SetActive(IsGuiEnabled);
            TextObject.SetActive(IsGuiEnabled);
        }

        public void DisableActiveMod()
        {
            if (selectedIndex >= 0 && selectedIndex < Mods.Count)
            {
                ModUtils.modState[Mods[selectedIndex]] = false;
                UpdateMods(ModUtils.pageMods[this]);
            }
        }
    }
}
