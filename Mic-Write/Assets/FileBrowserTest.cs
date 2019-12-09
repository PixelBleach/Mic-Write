using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleFileBrowser;
using UnityEngine.UI;
using System.Linq;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

public class FileBrowserTest : MonoBehaviour
{
    // Warning: paths returned by FileBrowser dialogs do not contain a trailing '\' character
    // Warning: FileBrowser can only show 1 dialog at a time
    public int totalDictionarysToMerge = 0;
    public Text path1;
    public Text path2;
    public Text path3;
    public Text path4;
    public Text path5;

    //properties
    [SerializeField]
    private bool file1;
    private bool file2;
    private bool file3;
    private bool file4;
    private bool file5;
    public bool File1
    {
        get
        {
            return file1;
        }
        set
        {
            file1 = value;
        }
    }
    public bool File2
    {
        get
        {
            return file2;
        }
        set
        {
            file2 = value;
        }
    }
    public bool File3
    {
        get
        {
            return file3;
        }
        set
        {
            file3 = value;
        }
    }
    public bool File4
    {
        get
        {
            return file4;
        }
        set
        {
            file4 = value;
        }
    }
    public bool File5
    {
        get
        {
            return file5;
        }
        set
        {
            file5 = value;
        }
    }



    public Dictionary<DateTime, string> dictionary1 = new Dictionary<DateTime, string>();
    public Dictionary<DateTime, string> dictionary2 = new Dictionary<DateTime,string>();
    public Dictionary<DateTime, string> dictionary3 = new Dictionary<DateTime, string>();
    public Dictionary<DateTime, string> dictionary4 = new Dictionary<DateTime, string>();
    public Dictionary<DateTime, string> dictionary5 = new Dictionary<DateTime, string>();

    void Start()
    {
        // Set filters (optional)
        // It is sufficient to set the filters just once (instead of each time before showing the file browser dialog), 
        // if all the dialogs will be using the same filters
        //FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".jpg", ".png"), new FileBrowser.Filter("Text Files", ".txt", ".pdf"));

        // Set default filter that is selected when the dialog is shown (optional)
        // Returns true if the default filter is set successfully
        // In this case, set Images filter as the default filter
        //FileBrowser.SetDefaultFilter(".jpg");

        // Set excluded file extensions (optional) (by default, .lnk and .tmp extensions are excluded)
        // Note that when you use this function, .lnk and .tmp extensions will no longer be
        // excluded unless you explicitly add them as parameters to the function
        FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");

        // Add a new quick link to the browser (optional) (returns true if quick link is added successfully)
        // It is sufficient to add a quick link just once
        // Name: Users
        // Path: C:\Users
        // Icon: default (folder icon)
        FileBrowser.AddQuickLink("Users", "C:\\Users", null);

        // Coroutine example
        //StartCoroutine(ShowLoadDialogCoroutine());
    }

    IEnumerator ShowLoadDialogCoroutine(Text textField)
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: file, Initial path: default (Documents), Title: "Load File", submit button text: "Load"
        yield return FileBrowser.WaitForLoadDialog(false, null, "Load File", "Load");

        // Dialog is closed
        // Print whether a file is chosen (FileBrowser.Success)
        // and the path to the selected file (FileBrowser.Result) (null, if FileBrowser.Success is false)
        UnityEngine.Debug.Log(FileBrowser.Success + " " + FileBrowser.Result);
        textField.text = FileBrowser.Result;
    }

    public void SaveFilePath(Text textField)
    {
        StartCoroutine(ShowLoadDialogCoroutine(textField));

    }

    public void CombineSelectedDictionaries()
    {
        Dictionary<DateTime, string> megaDictionary = new Dictionary<DateTime, string>();

        if (file1)
        {
            dictionary1 = JsonConvert.DeserializeObject<Dictionary<DateTime, string>>(File.ReadAllText(@path1.text));
            megaDictionary = CombineDictionaries(dictionary1, megaDictionary);
        }
        if (file2)
        {
            dictionary2 = JsonConvert.DeserializeObject<Dictionary<DateTime, string>>(File.ReadAllText(@path2.text));
            megaDictionary = CombineDictionaries(dictionary2, megaDictionary);
        }
        if (file3)
        {
            dictionary3 = JsonConvert.DeserializeObject<Dictionary<DateTime, string>>(File.ReadAllText(@path3.text));
            megaDictionary = CombineDictionaries(dictionary3, megaDictionary);
        }
        if (file4)
        {
            dictionary4 = JsonConvert.DeserializeObject<Dictionary<DateTime, string>>(File.ReadAllText(@path4.text));
            megaDictionary = CombineDictionaries(dictionary4, megaDictionary);
        }
        if (file5)
        {
            dictionary5 = JsonConvert.DeserializeObject<Dictionary<DateTime, string>>(File.ReadAllText(@path5.text));
            megaDictionary = CombineDictionaries(dictionary5, megaDictionary);

        }
        SaveDictionary(megaDictionary);
    }

    private Dictionary<DateTime, string> CombineDictionaries(Dictionary<DateTime, string> dic1, Dictionary<DateTime, string> dic2)
    {
        foreach (var entry in dic1)
        {
            if (dic2.ContainsKey(entry.Key))
            {
                string prev = dic1[entry.Key];
                string newEntry = prev + dic2[entry.Key];
                dic2.Remove(entry.Key);
            }
        }

        Dictionary<DateTime, string> newDic = dic1.Concat(dic2).ToDictionary(e => e.Key, e => e.Value);
        return newDic;
    }

    public void SaveDictionary(Dictionary<DateTime, string> session)
    {
        Dictionary<DateTime, string> sortedDictionary = SortDictionary(session);
        DateTime now = DateTime.UtcNow;
        string path = Application.dataPath + "MEGA_Session_" + now.Month.ToString() + "_" + now.Day.ToString() + "_" + now.Year.ToString() + "_" + now.Hour.ToString() + "_" + now.Minute.ToString() + ".txt";
        using (StreamWriter file = new StreamWriter(path))
            foreach (var entry in sortedDictionary)
                file.WriteLine("{0}", entry.Value);

    }

    private Dictionary<DateTime, string> SortDictionary(Dictionary<DateTime, string> unsortedDictionary)
    {
        var list = unsortedDictionary.Keys.ToList();
        list.Sort();
        Dictionary<DateTime, string> sortedCombinationDictionary = new Dictionary<DateTime, string>();

        foreach (var key in list)
        {
            sortedCombinationDictionary[key] = unsortedDictionary[key];
        }

        return sortedCombinationDictionary;
    }

    public void CombineAndSave()
    {
        CombineSelectedDictionaries();
    }
}