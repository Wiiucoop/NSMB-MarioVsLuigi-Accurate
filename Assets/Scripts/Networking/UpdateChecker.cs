using System;
using System.IO;
using System.Net;
using UnityEngine;

using Newtonsoft.Json.Linq;

public class UpdateChecker {

    private static readonly string API_URL = "http://api.github.com/repos/Wiiucoop/NSMB-MarioVsLuigi-Accurate/releases/latest";

    /// <summary>
    /// Returns if we're up to date, OR newer, compared to the latest GitHub release version number
    /// </summary>
    public async static void IsUpToDate(Action<bool, string> callback) {

        //get http results
        HttpWebRequest request = (HttpWebRequest) WebRequest.Create(API_URL);
        request.Accept = "application/json";
        request.UserAgent = "Wiiucoop/NSMB-MarioVsLuigi-Accurate";

        HttpWebResponse response = (HttpWebResponse) await request.GetResponseAsync();

        if (response.StatusCode != HttpStatusCode.OK)
            return;

        try {
            //get the latest release version number from github
            string json = new StreamReader(response.GetResponseStream()).ReadToEnd();
            JObject data = JObject.Parse(json);

            string tag = data.Value<string>("tag_name");
            if (tag.StartsWith("v"))
                tag = tag[1..];

            string[] splitTag = tag.Split(".");

            string ver = Application.version;
            if (ver.StartsWith("v"))
                ver = ver[1..];
            if (ver.Contains("-"))
                ver = ver.Split("-")[0];

            string[] splitVer = Application.version.Split(".");

            Debug.Log($"[UPDATE CHECK] Local version: {Application.version} / Remote version: {tag}");

            //check if we're a higher version
            bool upToDate = true;
            if ((Application.version != tag) && !Application.isEditor) {
                upToDate = false;
            }
            

            callback(upToDate, tag);
        } catch { }
    }
}
