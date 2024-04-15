using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRunner : MonoBehaviour
{

    public DraftRunner[] scripts = new DraftRunner[8];

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < scripts.Length; i++)
        {
            if (GameObject.Find("Player Runner " + (i + 1)) != null)
            {
                scripts[i] = GameObject.Find("Player Runner " + (i + 1)).GetComponent<DraftRunner>();
            }
            else
            {
                scripts[i] = GameObject.Find("Player Runner " + (i + 1) + " (bot)").GetComponent<DraftRunner>();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShiftCardObjectsBetweenScripts()
    {
        GameObject[,] packs = new GameObject[8, 15];

        for (int pack = 0; pack < scripts.Length; pack++)
        {
            for (int card = 0; card < 15; card++)
            {
                packs[pack, card] = scripts[pack].cardObjects[card];
            }
        }

        GameObject[] lastPack = new GameObject[15];
        for (int i = 0; i < 15; i++)
        {
            lastPack[i] = scripts[7].cardObjects[i];
        }
        for (int i = 1; i < scripts.Length; i++)
        {
            for (int j = 0; j < 15; j++)
            {
                scripts[i].cardObjects[j] = packs[i - 1, j];
            }
            scripts[i].nextPack();
        }

        for (int card = 0; card < 15; card++)
        {
            scripts[0].cardObjects[card] = lastPack[card];
        }
        
        scripts[0].nextPack();
    }
}
