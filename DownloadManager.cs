using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class DownloadManager : MonoBehaviour
{

    public string fileName = "CubeForge.txt";
    public List<string> deckList = new List<string>();
    public List<string> sideboard = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void downloadFile()
    {
        string downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";

        int tries = 0;
        while (File.Exists(Path.Combine(downloadsFolder, fileName)))
        {
            tries++;

            fileName = "CubeForge(" + tries + ").txt";
        }

        string filePath = Path.Combine(Application.temporaryCachePath, fileName);

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine("Deck:");
            writer.WriteLine("");
            foreach (string line in deckList)
            {
                writer.WriteLine(line);
            }
            
            writer.WriteLine("");
            writer.WriteLine("Sideboard:");
            writer.WriteLine("");
            foreach (string line in sideboard)
            {
                writer.WriteLine(line);
            }
        }

        StartCoroutine(DownloadPrompt(filePath));
    }

    public IEnumerator DownloadPrompt(string filePath)
    {
        yield return new WaitForSeconds(0.5f);

        byte[] fileBytes = File.ReadAllBytes(filePath);

        WWW www = new WWW("file://" + filePath);

        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            string destinationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", fileName);
            File.Move(filePath, destinationPath);
        }
        else
        {
            Debug.LogError("Error downloading file: " + www.error);
        }
    }
}
