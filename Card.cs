using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{

    private int manaCost;
    private string cardName;
    private string cardType;
    public string color;
    private bool isInDeck;

    public bool isInSideboard = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int getManaCost()
    {
        return manaCost;
    }

    public void setManaCost(int costToSet)
    {
        manaCost = costToSet;
    }



    public string getCardName()
    {
        return cardName;
    }

    public void setCardName(string nameToSet)
    {
        cardName = nameToSet;
    }



    public string getCardType()
    {
        return cardType;
    }

    public void setCardType(string typeToSet)
    {
        cardType = typeToSet;
    }

    public void setIsInDeck(bool toSet)
    {
        isInDeck = toSet;
    }

    public bool getIsInDeck()
    {
        return isInDeck;
    }

    public void setIsInSideboard(bool toSet)
    {
        isInSideboard = toSet;
    }

    public bool getIsInSideboard()
    {
        return isInSideboard;
    }

    public void setColor(string toSet)
    {
        color = toSet;
    }

    public string getColor()
    {
        return color;
    }
}
