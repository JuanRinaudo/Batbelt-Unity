using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class GoogleSheetBalance : EditorWindow
{
    string sheetUrl = "";
    SimpleConfig targetConfig;
    bool syncAllConfigs = true;

    const string PrefKey = "GoogleSheetBalance_LastUrl";

    [MenuItem("Tools/Google Sheet Balance")]
    public static void ShowWindow()
    {
        GetWindow<GoogleSheetBalance>("Google Sheet Balance");
    }

    void OnEnable()
    {
        sheetUrl = EditorPrefs.GetString(PrefKey, "https://docs.google.com/spreadsheets/d/");
    }

    void OnGUI()
    {
        GUILayout.Label("Google Sheet Balance Importer", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();
        sheetUrl = EditorGUILayout.TextField("Google Sheet URL", sheetUrl);
        if (EditorGUI.EndChangeCheck())
            EditorPrefs.SetString(PrefKey, sheetUrl);

        EditorGUILayout.Space();
        syncAllConfigs = EditorGUILayout.Toggle("Sync All SimpleConfigs", syncAllConfigs);

        if (!syncAllConfigs)
            targetConfig = (SimpleConfig)EditorGUILayout.ObjectField("Target Config", targetConfig, typeof(SimpleConfig), false);

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Download & Sync Balance", GUILayout.Height(35)))
            _ = SyncGoogleSheetAsync();
    }

    async Task SyncGoogleSheetAsync()
    {
        if (string.IsNullOrWhiteSpace(sheetUrl))
        {
            EditorUtility.DisplayDialog("Error", "Please provide a valid Google Sheet URL.", "OK");
            return;
        }

        string exportUrl = ConvertToTsvExportUrl(sheetUrl);
        if (string.IsNullOrEmpty(exportUrl))
        {
            EditorUtility.DisplayDialog("Error", "Could not extract Sheet ID and GID from the URL.", "OK");
            return;
        }

        EditorUtility.DisplayProgressBar("Google Sheet Balance", "Downloading TSV data...", 0.2f);

        string tsvContent;
        try
        {
            using var client = new HttpClient();
            tsvContent = await client.GetStringAsync(exportUrl);
        }
        catch (Exception ex)
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Download Failed", $"Failed to download TSV data:\n{ex.Message}", "OK");
            return;
        }

        EditorUtility.DisplayProgressBar("Google Sheet Balance", "Parsing TSV data...", 0.6f);

        var balanceData = ParseTsvData(tsvContent);

        EditorUtility.DisplayProgressBar("Google Sheet Balance", "Updating configs...", 0.8f);

        int updatedCount = 0;

        if (syncAllConfigs)
        {
            string[] guids = AssetDatabase.FindAssets("t:SimpleConfig");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var config = AssetDatabase.LoadAssetAtPath<SimpleConfig>(path);
                if (config != null)
                    updatedCount += ApplyDataToConfig(config, balanceData);
            }
        }
        else if (targetConfig != null)
        {
            updatedCount += ApplyDataToConfig(targetConfig, balanceData);
        }

        AssetDatabase.SaveAssets();
        EditorUtility.ClearProgressBar();

        Debug.Log($"[GoogleSheetBalance] Successfully updated {updatedCount} member(s) from sheet.");
        EditorUtility.DisplayDialog("Success", $"Balance updated successfully!\nTotal members modified: {updatedCount}", "OK");
    }

    static string ConvertToTsvExportUrl(string url)
    {
        var idMatch = Regex.Match(url, @"/d/([a-zA-Z0-9-_]+)");
        if (!idMatch.Success) return null;
        string sheetId = idMatch.Groups[1].Value;

        string gid = "0";
        var gidMatch = Regex.Match(url, @"[#&?]gid=([0-9]+)");
        if (gidMatch.Success)
            gid = gidMatch.Groups[1].Value;

        return $"https://docs.google.com/spreadsheets/d/{sheetId}/export?format=tsv&gid={gid}";
    }

    static Dictionary<string, List<string>> ParseTsvData(string tsv)
    {
        var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        string[] rows = tsv.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        string[][] grid = new string[rows.Length][];
        for (int r = 0; r < rows.Length; r++)
            grid[r] = rows[r].Split('\t');

        for (int r = 0; r < grid.Length; r++)
        {
            for (int c = 0; c < grid[r].Length; c++)
            {
                string cell = grid[r][c].Trim();

                // Read both # (table/key column) and ! (value/field columns)
                if ((cell.StartsWith("#") || cell.StartsWith("!")) && cell.Length > 1)
                {
                    string key = cell.Substring(1).Trim();
                    var values = new List<string>();

                    for (int subR = r + 1; subR < grid.Length; subR++)
                    {
                        if (c >= grid[subR].Length) break;

                        string val = grid[subR][c].Trim();

                        // Stop when reaching empty cell or another header row
                        if (string.IsNullOrEmpty(val) || val.StartsWith("#") || val.StartsWith("!"))
                            break;

                        values.Add(val);
                    }

                    result[key] = values;
                }
            }
        }

        return result;
    }

    static int ApplyDataToConfig(SimpleConfig config, Dictionary<string, List<string>> balanceData)
    {
        int modifiedCount = 0;
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        Undo.RecordObject(config, "Google Sheet Balance Update");

        // Inspect Fields
        foreach (var field in config.GetType().GetFields(flags))
        {
            var attributes = field.GetCustomAttributes<GoogleSheetBindingAttribute>(true);
            foreach (var attr in attributes)
            {
                if (attr.Apply(config, field, balanceData))
                    modifiedCount++;
            }
        }

        // Inspect Properties
        foreach (var prop in config.GetType().GetProperties(flags))
        {
            if (!prop.CanWrite) continue;

            var attributes = prop.GetCustomAttributes<GoogleSheetBindingAttribute>(true);
            foreach (var attr in attributes)
            {
                if (attr.Apply(config, prop, balanceData))
                    modifiedCount++;
            }
        }

        if (modifiedCount > 0)
            EditorUtility.SetDirty(config);

        return modifiedCount;
    }
}