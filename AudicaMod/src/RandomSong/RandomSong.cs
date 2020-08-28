﻿using System;
using Il2CppSystem.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Harmony;
using MelonLoader;

namespace AudicaModding
{
    public class RandomSong : MelonMod
    {
        private static GameObject randomSongButton;
        public static bool exists => randomSongButton != null;

        private static Vector3 randomButtonPos = new Vector3(10.2f, -9.4f, 24.2f);
        private static Vector3 randomButtonRot = new Vector3(0f, 0f, 0f);

        private static int randomSongBagSize = 10;
        private static int mainSongCount = 33;
        private static bool availableSongListsSetup = false;
        private static List<int> availableMainSongs = new List<int>();
        private static List<int> availableExtrasSongs = new List<int>();
        private static List<int> lastPickedSongs = new List<int>();
        private static List<int> availableSongs = new List<int>();

        private static SongSelect songSelect = null;

        private static Il2CppSystem.Collections.Generic.List<SongSelectItem> songs = new Il2CppSystem.Collections.Generic.List<SongSelectItem>();

        private void CreateConfig()
        {
            ModPrefs.RegisterPrefInt("RandomSong", "RandomSongBagSize", randomSongBagSize);

        }

        private void LoadConfig()
        {
            randomSongBagSize = ModPrefs.GetInt("RandomSong", "RandomSongBagSize");
            if (randomSongBagSize > mainSongCount) randomSongBagSize = mainSongCount;

        }

        public static void SaveConfig()
        {
            ModPrefs.SetInt("RandomSong", "RandomSongBagSize", randomSongBagSize);
        }

        public override void OnLevelWasLoaded(int level)
        {

            if (!ModPrefs.HasKey("RandomSong", "RandomSongBagSize"))
            {
                CreateConfig();
            }
            else
            {
                LoadConfig();

            }
        }

        public static void CreateRandomSongButton()
        {
            if (randomSongButton != null)
            {
                randomSongButton.SetActive(true);
                return;
            }
            var backButton = GameObject.Find("menu/ShellPage_Song/page/backParent/back");
            randomSongButton = GameObject.Instantiate(backButton, backButton.transform.parent.transform);
            GameObject.Destroy(randomSongButton.GetComponentInChildren<Localizer>());
            TextMeshPro buttonText = randomSongButton.GetComponentInChildren<TextMeshPro>();
            buttonText.text = "Random Song";
            GunButton button = randomSongButton.GetComponentInChildren<GunButton>();
            button.destroyOnShot = false;
            button.doMeshExplosion = false;
            button.doParticles = false;
            button.onHitEvent = new UnityEvent();
            button.onHitEvent.AddListener(new Action(() => { OnRandomSongButtonShot(); }));
            randomSongButton.transform.position = randomButtonPos;
            randomSongButton.transform.rotation = Quaternion.Euler(randomButtonRot);
        }
        private static void OnRandomSongButtonShot()
        {
            songSelect = GameObject.FindObjectOfType<SongSelect>();
            songs = songSelect.songSelectItems.mItems;
            int maxLength = songs.Count - 1;
            if (!availableSongListsSetup)
            {
                availableSongListsSetup = true;

                for (int i = 0; i < mainSongCount; i++)
                {
                    availableMainSongs.Add(i);
                }

                for (int i = mainSongCount; i < maxLength; i++)
                {
                    availableExtrasSongs.Add(i);
                }

                for (int i = 0; i < maxLength; i++)
                {
                    availableSongs.Add(i);
                }
            }
            SongSelect.Filter filter = songSelect.GetListFilter();

            var rand = new System.Random();
            int index;
            if (filter == SongSelect.Filter.All)
            {
                index = availableSongs[rand.Next(0, availableSongs.Count - 1)];
            }
            else if (filter == SongSelect.Filter.Main)
            {
                index = availableMainSongs[rand.Next(0, availableMainSongs.Count - 1)];
                if (availableMainSongs.Count > 0) availableMainSongs.Remove(index);
            }
            else
            {
                index = availableExtrasSongs[rand.Next(0, availableExtrasSongs.Count - 1)];
                if (availableExtrasSongs.Count > 0) availableExtrasSongs.Remove(index);
            }
            songs[index].OnSelect();
            lastPickedSongs.Add(index);
            if (availableSongs.Count > 0) availableSongs.Remove(index);


            if (lastPickedSongs.Count > randomSongBagSize)
            {
                int oldestIndex = lastPickedSongs[0];
                lastPickedSongs.Remove(oldestIndex);
                availableSongs.Add(oldestIndex);
                if (oldestIndex < 33) availableMainSongs.Add(index);
                else availableExtrasSongs.Add(index);
            }
        }

        [HarmonyPatch(typeof(MenuState), "SetState", new Type[] { typeof(MenuState.State) })]
        private static class RandomSongPatchMenuState
        {
            private static void Postfix(MenuState __instance, ref MenuState.State state)
            {
                if (state == MenuState.State.SongPage) CreateRandomSongButton();
            }
        }

    }
}
