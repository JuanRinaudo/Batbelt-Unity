using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class GoogleSheetBalance : EditorWindow
{
    private string sheetUrl = "";
    private SimpleConfig targetConfig;
    private bool syncAllConfigs = true;

    private const string PrefKey = "GoogleSheetBalance_LastUrl";

    [MenuItem("Tools/Google Sheet Balance")]
    public static void ShowWindow()
    {
        GetWindow<GoogleSheetBalance>("Google Sheet Balance");
    }

    private void OnEnable()
    {
        sheetUrl = EditorPrefs.GetString(
            PrefKey,
            "https://docs.google.com/spreadsheets/d/1gYBruyfgzHMot2Qzpe4F1jEgLDBqwhF4OCHFlZwVgm4/edit?gid=0#gid=0"
        );
    }

    private void OnGUI()
    {
        GUILayout.Label("Google Sheet Balance Importer", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();
        sheetUrl = EditorGUILayout.TextField("Google Sheet URL", sheetUrl);
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetString(PrefKey, sheetUrl);
        }

        EditorGUILayout.Space();
        syncAllConfigs = EditorGUILayout.Toggle("Sync All SimpleConfigs", syncAllConfigs);

        if (!syncAllConfigs)
        {
            targetConfig = (SimpleConfig)EditorGUILayout.ObjectField("Target Config", targetConfig, typeof(SimpleConfig), false);
        }

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Download & Sync Balance", GUILayout.Height(35)))
        {
            _ = SyncGoogleSheetAsync();
        }
    }

    private async Task SyncGoogleSheetAsync()
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
                {
                    updatedCount += ApplyDataToConfig(config, balanceData);
                }
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

    private static string ConvertToTsvExportUrl(string url)
    {
        var idMatch = Regex.Match(url, @"/d/([a-zA-Z0-9-_]+)");
        if (!idMatch.Success) return null;
        string sheetId = idMatch.Groups[1].Value;

        string gid = "0";
        var gidMatch = Regex.Match(url, @"[#&?]gid=([0-9]+)");
        if (gidMatch.Success)
        {
            gid = gidMatch.Groups[1].Value;
        }

        return $"https://docs.google.com/spreadsheets/d/{sheetId}/export?format=tsv&gid={gid}";
    }

    private static Dictionary<string, List<string>> ParseTsvData(string tsv)
    {
        var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        string[] rows = tsv.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string[][] grid = new string[rows.Length][];
        for (int r = 0; r < rows.Length; r++)
        {
            grid[r] = rows[r].Split('\t');
        }

        for (int r = 0; r < grid.Length; r++)
        {
            for (int c = 0; c < grid[r].Length; c++)
            {
                string cell = grid[r][c].Trim();

                if (cell.StartsWith("#") && cell.Length > 1)
                {
                    string key = cell.Substring(1).Trim();
                    var values = new List<string>();

                    for (int subR = r + 1; subR < grid.Length; subR++)
                    {
                        if (c >= grid[subR].Length) break;

                        string val = grid[subR][c].Trim();

                        if (string.IsNullOrEmpty(val) || val.StartsWith("#"))
                        {
                            break;
                        }

                        values.Add(val);
                    }

                    result[key] = values;
                }
            }
        }

        return result;
    }

    private static int ApplyDataToConfig(SimpleConfig config, Dictionary<string, List<string>> balanceData)
    {
        int modifiedCount = 0;
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        Undo.RecordObject(config, "Google Sheet Balance Update");

        // Process Fields
        foreach (var field in config.GetType().GetFields(flags))
        {
            var valAttr = field.GetCustomAttribute<GoogleSheetValueAttribute>();
            if (valAttr != null)
            {
                string key = !string.IsNullOrEmpty(valAttr.Key) ? valAttr.Key : field.Name;
                if (balanceData.TryGetValue(key, out var valList) && valList.Count > 0)
                {
                    field.SetValue(config, ConvertValue(valList[0], field.FieldType));
                    modifiedCount++;
                    continue;
                }
            }

            var arrAttr = field.GetCustomAttribute<GoogleSheetArrayAttribute>();
            if (arrAttr != null)
            {
                string key = !string.IsNullOrEmpty(arrAttr.Key) ? arrAttr.Key : field.Name;
                if (balanceData.TryGetValue(key, out var arrList))
                {
                    object collection = CreateCollection(field.FieldType, arrList);
                    if (collection != null)
                    {
                        field.SetValue(config, collection);
                        modifiedCount++;
                    }
                }
            }
        }

        // Process Properties (if they have setters)
        foreach (var prop in config.GetType().GetProperties(flags))
        {
            if (!prop.CanWrite) continue;

            var valAttr = prop.GetCustomAttribute<GoogleSheetValueAttribute>();
            if (valAttr != null)
            {
                string key = !string.IsNullOrEmpty(valAttr.Key) ? valAttr.Key : prop.Name;
                if (balanceData.TryGetValue(key, out var valList) && valList.Count > 0)
                {
                    prop.SetValue(config, ConvertValue(valList[0], prop.PropertyType));
                    modifiedCount++;
                    continue;
                }
            }

            var arrAttr = prop.GetCustomAttribute<GoogleSheetArrayAttribute>();
            if (arrAttr != null)
            {
                string key = !string.IsNullOrEmpty(arrAttr.Key) ? arrAttr.Key : prop.Name;
                if (balanceData.TryGetValue(key, out var arrList))
                {
                    object collection = CreateCollection(prop.PropertyType, arrList);
                    if (collection != null)
                    {
                        prop.SetValue(config, collection);
                        modifiedCount++;
                    }
                }
            }
        }

        if (modifiedCount > 0)
        {
            EditorUtility.SetDirty(config);
        }

        return modifiedCount;
    }

    private static object CreateCollection(Type targetType, List<string> rawList)
    {
        if (targetType.IsArray)
        {
            Type elementType = targetType.GetElementType();
            Array arrayInstance = Array.CreateInstance(elementType, rawList.Count);
            for (int i = 0; i < rawList.Count; i++)
            {
                arrayInstance.SetValue(ConvertValue(rawList[i], elementType), i);
            }
            return arrayInstance;
        }

        if (typeof(IList).IsAssignableFrom(targetType) && targetType.IsGenericType)
        {
            Type elementType = targetType.GetGenericArguments()[0];
            var listInstance = (IList)Activator.CreateInstance(targetType);
            for (int i = 0; i < rawList.Count; i++)
            {
                listInstance.Add(ConvertValue(rawList[i], elementType));
            }
            return listInstance;
        }

        return null;
    }

    private static object ConvertValue(string raw, Type targetType)
    {
        raw = raw.Trim();

        if (targetType == typeof(string)) return raw;
        if (targetType == typeof(float)) return float.Parse(raw.Replace(',', '.'), CultureInfo.InvariantCulture);
        if (targetType == typeof(double)) return double.Parse(raw.Replace(',', '.'), CultureInfo.InvariantCulture);
        if (targetType == typeof(int)) return int.Parse(raw, CultureInfo.InvariantCulture);
        if (targetType == typeof(long)) return long.Parse(raw, CultureInfo.InvariantCulture);

        if (targetType == typeof(bool))
        {
            if (bool.TryParse(raw, out bool b)) return b;
            if (raw == "1") return true;
            if (raw == "0") return false;
        }

        if (targetType.IsEnum)
        {
            return Enum.Parse(targetType, raw, true);
        }

        return Convert.ChangeType(raw, targetType, CultureInfo.InvariantCulture);
    }
}