using System;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;
using easyInputs;
using Util;
using GorillaLocomotion;
using HandHeldTemplate.Mods;

namespace HandHeldTemplate
{
    public class Main : MelonMod
    {

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            List<PageInfo> pages = new List<PageInfo>
            {
                new PageInfo("Harmony.lol", new string[] { "Fly", "ModB", "ModC" })
            };
            MenuHandler.InitializePages(pages);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            bool menuActivationButtonPressed = EasyInputs.GetSecondaryButtonDown(EasyHand.LeftHand);
            MenuHandler.UpdateMenuPosition();
            if (menuActivationButtonPressed)
            {
                MenuHandler.EnableMenu();
            }
            else
            {
                MenuHandler.DisableMenu();
            }
            HandleMods();
        }

        public void HandleMods()
        {
            if (MenuHandler.IsModEnabled("Fly"))
            {
                ModManager.Fly(25);
            }
        }
    }
}
