﻿
using AudicaModding;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

internal static class DifficultyDisplay
{
    static GameObject difficultyDisplay;
    static TextMeshPro difficultyText;
    static RectTransform rect;
    public static void Initialize()
    {
        if (difficultyDisplay == null)
        {
            difficultyDisplay = new GameObject();
            difficultyText = difficultyDisplay.AddComponent<TextMeshPro>();
            rect = difficultyDisplay.GetComponent<RectTransform>();
            rect.transform.position = new Vector3(0f, -11.5f, 23.85f);
            rect.sizeDelta = new Vector2(20f, 5f);
            rect.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
            difficultyText.fontSize = 18;
            difficultyText.margin = new Vector4(0f, 0f, 0f, -0.7521667f);
            difficultyText.alignment = TextAlignmentOptions.Center;
            difficultyText.text = "Difficulty Display Error";
            difficultyDisplay.layer = 5;
            difficultyText.font = GameObject.Find("menu/ShellPage_Launch/page/ShellPanel_Center/play/Label").GetComponent<TextMeshPro>().font;
        }
    }

    public static void Hide()
    {
        if(difficultyDisplay != null)
        {
            difficultyDisplay.SetActive(false);
            
        }
    }

    public static void Show()
    {
        if (difficultyDisplay != null)
        {
            difficultyDisplay.SetActive(true);
            UpdateTextFromList();
        }
        else
        {
            Initialize();
            UpdateTextFromList();
        }
    }

    public static void UpdateTextFromList()
    {
        difficultyText.text = CreateDisplayString(ScoreHistory.calculatedScores);
    }

    private struct fillData
    {
        public string easyAdditions;
        public string standardAdditions;
        public string advancedAdditions;
        public string expertAdditions;
    }
    //could be used for other info in the future
    public static string CreateDisplayStringAdditions(SongDataLoader.SongData currentSong, string tagtofind, string textcolor)
    {
        if (currentSong.HasCustomData())
        {
            //if adding 360 tags
            if (tagtofind == "easy360" || tagtofind == "advanced360" || tagtofind == "standard360" || tagtofind == "expert360")
            {
                if (currentSong.SongHasCustomDataKey(tagtofind))
                {
                    if(currentSong.GetCustomData<bool>(tagtofind))
                    {
                        return "<color=#" + textcolor + ">(360) </color>";
                    }
                }
            }

            //TODO: could do more tags here
        }

        return "";
    }

    private static fillData fillAdditions(string songID, fillData d)
    {
        SongDataLoader.SongData currentSong = SongDataLoader.AllSongData[songID];
        d.easyAdditions = CreateDisplayStringAdditions(currentSong, "easy360", "54f719");
        d.advancedAdditions = CreateDisplayStringAdditions(currentSong, "advanced360", "f7a919");
        d.standardAdditions = CreateDisplayStringAdditions(currentSong, "standard360", "19d2f7");
        d.expertAdditions = CreateDisplayStringAdditions(currentSong, "expert360", "b119f7");

        return d;
    }

    public static string CreateDisplayString(List<CalculatedScoreEntry> scores)
    {
        string output = "Difficulty Rating\n";
        var songData = SongDataHolder.I.songData;
        var calc = new DifficultyCalculator(songData);

        fillData AdditionHolder = new fillData();

        if (SongBrowser.songDataLoaderInstalled)
        {
            //fillAdditions() is separated into its own function to avoid errors if !songDataLoaderInstalled
            /*Explanation:
            When CreateDisplayString() is called the first time it loads in all the functions it needs.
            If we include anything from the Song Data Loader utility in CreateDisplayString() it will try to
            load from the dll. In the case they dont have the dll, it will throw an exception and the rest of
            CreateDisplayString() won't run. Since all mentions of Son Data Loader are in fillAdditions(), which 
            is behind a conditional we wont have that issue.
            */
            AdditionHolder.easyAdditions = "";
            AdditionHolder.advancedAdditions = "";
            AdditionHolder.standardAdditions = "";
            AdditionHolder.expertAdditions = "";

            AdditionHolder = fillAdditions(songData.songID, AdditionHolder);
        }

        if (calc.expert != null)
        {
            output += $"<color=#b119f7>{calc.expert.difficultyRating.ToString("n2")}</color>  ";
            output += AdditionHolder.expertAdditions;
        }
        if (calc.advanced != null)
        {
            output += $"<color=#f7a919>{calc.advanced.difficultyRating.ToString("n2")}</color>  ";
            output += AdditionHolder.advancedAdditions;
        }
        if (calc.standard != null)
        {
            output += $"<color=#19d2f7>{calc.standard.difficultyRating.ToString("n2")}</color>  ";
            output += AdditionHolder.standardAdditions;
        }
        if (calc.beginner != null)
        {
            output += $"<color=#54f719>{calc.beginner.difficultyRating.ToString("n2")}</color>  ";
            output += AdditionHolder.easyAdditions;
        }
        return output;
    }

}
