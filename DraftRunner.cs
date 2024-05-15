using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using Unity.VisualScripting;
using TMPro;
using System;
using static System.Net.WebRequestMethods;
using UnityEngine.Experimental.Rendering;

public class DraftRunner : MonoBehaviour
{
    public List<GameObject> cardObjects = new List<GameObject>();
    public GameObject[] manaSlotPositions = new GameObject[10];
    public GameObject[] startPosObjects = new GameObject[15];
    public GameObject centerPosition;
    public GameObject saveScreen;

    public float[] targetDropPos =
    {
      4.16f,
      4.16f,
      4.16f,
      4.16f,
      4.16f,
      4.16f,
      4.16f,
      4.16f,
      1.4f,
      1.4f,
      1.4f,
      1.4f,
      1.4f,
      1.4f,
      1.4f
    };

    public string[] pack = new string[15];
    public string[] cardTypes = { "Land", "Creature", "Instant", "Sorcery", "Artifact", "Enchantment" };
    public List<string> deckList = new List<string>();
    public List<string> sideboardList = new List<string>();
    public List<string> colors = new List<string>();

    public int[] cardsOfEachType = new int[7];
    public int[] cardInManaSlot = new int[10];
    public int packNum = 1;
    public int pickNum = 0;
    public int cardsInDeck = 0;
    public int playernum;
    public int landPickedNum = 0;

    public bool hasChosen = false;

    public DraftRunner[] allRunners = new DraftRunner[8];

    public List<string> cubeList = new List<string>();

    public bool canClick;
    public bool canZoom;
    public bool cardIsUp;
    public bool isBot = false;

    public TextMeshProUGUI typeDisplayText;
    public TextMeshProUGUI packPickDisplay;

    public Vector3 originalPos;

    public GameObject cardPrefab;
    public GameObject cardUp;
    public GameObject[] startButtons = new GameObject[3];

    public Camera camera;

    public AccountManager accountManager;

    public DownloadManager downloader;

    public enum format
    { 
        vintage,
        randomCards,
        pauper
    }

    public format currentFormat = format.vintage;

    // Start is called before the first frame update
    void Start()
    {
        saveScreen.SetActive(false);
        downloader = GameObject.Find("Download Manager").GetComponent<DownloadManager>();
        accountManager = GameObject.Find("Log In Runner").GetComponent<AccountManager>();
    }

    public void startDraft(string formatChosen)
    {
        if (accountManager.isMovingCamera)
        {
            return;
        }

        if (transform.name.IndexOf("bot") == -1)
        {
            foreach (GameObject button in startButtons)
            {
                button.SetActive(false);
            }
        }

        if (formatChosen == "vintage")
        {
            currentFormat = format.vintage;
        }
        else if (formatChosen == "randomCards")
        {
            currentFormat = format.randomCards;
        }
        else if (formatChosen == "pauper")
        {
            currentFormat = format.pauper;
        }

        for (int i = 0; i < allRunners.Length; i++)
        {
            if (GameObject.Find("Player Runner " + (i + 1)) != null)
            {
                allRunners[i] = GameObject.Find("Player Runner " + (i + 1)).GetComponent<DraftRunner>();
            }
            else
            {
                allRunners[i] = GameObject.Find("Player Runner " + (i + 1) + " (bot)").GetComponent<DraftRunner>();
            }
        }

        canClick = false;
        canZoom = false;

        if (transform.name.IndexOf("bot") != -1)
        {
            isBot = true;
        }

        

        typeDisplayText.text = "Deck: 0   Land: 0  Creature: 0  Spells: 0  Artifacts: 0  Enchantments: 0 Other: 0";
        packPickDisplay.text = "Pack: 1 Pick: 1";

        StartCoroutine(setCubeList());
    }

    // Update is called once per frame
    void Update()
    {
        if (canClick || canZoom)
        {
            if (!isBot)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    if (!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1) && hit.collider.gameObject.tag == "card" && !cardIsUp)
                    {
                        if (!hit.collider.gameObject.GetComponent<Card>().getIsInDeck())
                        {
                            hit.collider.gameObject.transform.localScale = new Vector3(1.72f, 2.38f, hit.collider.gameObject.transform.localScale.z);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < cardObjects.Count; i++)
                        {
                            cardObjects[i].transform.localScale = new Vector3(1.67f, 2.33f, cardObjects[i].transform.localScale.z);
                        }
                    }
                    if (hit.collider.gameObject.tag == "card")
                    {
                        if (Input.GetMouseButtonDown(0) && canClick && !cardIsUp)
                        {
                            Card cardScript = hit.collider.gameObject.GetComponent<Card>();
                            if (!cardScript.getIsInDeck())
                            {
                                cardScript.setIsInDeck(true);
                                cardIsUp = false;
                                StartCoroutine(centerSelectedCard(hit.collider.gameObject, 0.3f, centerPosition.transform.position));
                                StartCoroutine(MoveCardsOff());
                            }
                        }
                        if (Input.GetMouseButtonDown(0) && canZoom && hit.collider.gameObject.GetComponent<Card>().getIsInDeck() && hit.collider.gameObject != cardUp)
                        {
                            if (hit.collider.gameObject.GetComponent<Card>().getIsInSideboard())
                            {
                                StartCoroutine(moveOutOfSideboard(hit.collider.gameObject, 0.1f));
                            }
                            else
                            {
                                StartCoroutine(moveToSideboard(hit.collider.gameObject, 0.1f, manaSlotPositions[9].transform.position));
                            }
                        } 
                        if (Input.GetMouseButtonDown(1) && canZoom)
                        {
                            if (hit.collider.gameObject == cardUp)
                            {
                                StartCoroutine(zoomOutCard(cardUp, 0.2f, cardUp.transform.position, originalPos));
                                cardUp = null;
                            }
                            else
                            {
                                StartCoroutine(zoomInCard(hit.collider.gameObject, 0.2f, new Vector3(0f, 1.13f, 7.36000013f)));
                            }
                        }
                    }
                }
            }
        }
    }


    private void instantiateCardObjects()
    {
        for (int card = 0; card <= 14; card++)
        {
            Vector3 startPos = new Vector3(startPosObjects[card].transform.position.x, startPosObjects[card].transform.position.y, startPosObjects[card].transform.position.z);
            cardObjects.Add(Instantiate(cardPrefab, startPos, Quaternion.identity));
        }

        for (int i = 0; i < cardObjects.Count; i++)
        {
            cardObjects[i].transform.rotation = Quaternion.Euler(180f, 180f, 0f);
        }

        makePacks();
    }


    private IEnumerator dropDownCards()
    {
        hasChosen = false;
        pickNum++;
        packPickDisplay.text = "Pack: " + packNum + " Pick: " + pickNum;

        float timeElapsed = 0f;

        while (timeElapsed < 2f)
        {
            for (int i = 0; i < cardObjects.Count; i++)
            {
                Card cardScript = cardObjects[i].GetComponent<Card>();
                if (!cardScript.getIsInDeck())
                {
                    Vector3 startYPos = cardObjects[i].transform.position;

                    int targetYPosIndex = i;
                    if (targetYPosIndex > 14)
                    {
                        targetYPosIndex = i % 15;
                    }
                    float endYPos = targetDropPos[targetYPosIndex];

                    float t = timeElapsed / 10f;
                    float newY = Mathf.Lerp(startYPos.y, endYPos, Mathf.SmoothStep(0f, 2.5f, t));
                    cardObjects[i].transform.position = new Vector3(startYPos.x, newY, startYPos.z);
                }
            }

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < cardObjects.Count; i++)
        {
            Card cardScript = cardObjects[i].GetComponent<Card>();
            if (!cardScript.getIsInDeck())
            {
                cardObjects[i].transform.position = new Vector3(cardObjects[i].transform.position.x, targetDropPos[i], cardObjects[i].transform.position.z);
            }
        }

        StartCoroutine(rotateCards(180f, 500f));
    }

    public IEnumerator rotateCards(float rotDistance, float rotSpeed)
    {
        float totalRotation = 0f;

        while (totalRotation < rotDistance)
        {
            float deltaRotation = rotSpeed * Time.deltaTime;

            for (int i = 0; i < cardObjects.Count; i++)
            {
                Card cardScript = cardObjects[i].GetComponent<Card>();
                if (!cardScript.getIsInDeck())
                {
                    cardObjects[i].transform.Rotate(Vector3.up, deltaRotation);
                }
            }

            totalRotation += deltaRotation;

            yield return null;
        }

        canClick = true;
        canZoom = true;

        for (int i = 0; i < cardObjects.Count; i++)
        {
            cardObjects[i].transform.rotation = Quaternion.Euler(-180f, 0f, 0f);
        }

        if (isBot)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(2f, 5f));
            StartCoroutine(takeBotCard());
        }
    }

    public string[] powerCardNames = 
    {
        "Time Walk",
        "Black Lotus",
        "Sol Ring",
        "Mana Crypt",
        "Ancestral Recall",
        "Mox Sapphire",
        "Mox Ruby",
        "Mox Jet",
        "Mox Pearl",
        "Mox Emerald",
        "Minsc & Boo, Timeless Heroes",
        "Forth Eorlingas!",
        "Orcish Bowmasters",
        "Library of Alexandria",
        "Tolarian Academy",
        "Reanimate",
        "Oko, Thief of Crowns",
        "Lutri, the Spellchaser",
        "Seasoned Dungeoneer",
        "Caves of Chaos Adventurer",
        "Thoughtseize",
        "Lurrus of the Dream-Den",
        "Animate Dead",
        "Swords to Plowshares",
        "The One Ring",
        "Mana Vault",
        "Atraxa, Grand Unifier",
        "Comet, Stellar Pup",
        "Archon of Cruelty",
        "Flash",
        "Mox Diamond",
        "Mana Drain",
        "Force of Will",
        "Mystic Confluence",
        "Fable of the Mirror-Breaker",
        "Ragavan, Nimble Pilferer",
        "Demonic Tutor",
        "Hullbreacher",
        "Grief",
        "Urza, Lord High Artificer",
        "Solitude",
        "Lightning Bolt",
        "Urza's Saga"
    };

    public IEnumerator takeBotCard()
    {
        // Initialize variables
        int randomCardIndex = -1;
        bool foundPowerCard = false;
        bool allowLand = pickNum >= 5; // Allow land cards at pick 5 or later

        // Check for powerful cards first
        for (int i = 0; i < cardObjects.Count; i++)
        {
            if (!cardObjects[i].GetComponent<Card>().getIsInDeck())
            {
                string cardName = cardObjects[i].name;
                if (Array.Exists(powerCardNames, element => element == cardName))
                {
                    foundPowerCard = true;
                    randomCardIndex = i;
                    break;
                }
            }
        }

        // If found a powerful card, add its colors
        if (foundPowerCard)
        {
            addColors(cardObjects[randomCardIndex]);
        }
        else
        {
            // Initialize variables to store matching card indices
            List<int> matchingCardsIndices = new List<int>();

            // Iterate through card objects to find matching colors
            for (int i = 0; i < cardObjects.Count; i++)
            {
                if (!cardObjects[i].GetComponent<Card>().getIsInDeck())
                {
                    // Check if the card is a land
                    bool isLand = cardObjects[i].GetComponent<Card>().getCardType().IndexOf("Land") != -1;
                    if (isLand && !allowLand)
                    {
                        continue; // Skip land cards if not allowed
                    }

                    // If it's not a land or land is allowed, proceed with color matching
                    string cardColorString = cardObjects[i].GetComponent<Card>().getColor();
                    string[] cardColors = cardColorString.Trim('[', ']').Split(',');

                    // Check if the card's colors match any of the bot's colors
                    bool isMatchingColor = false;
                    foreach (string color in cardColors)
                    {
                        if (colors.Contains(color.Trim()))
                        {
                            isMatchingColor = true;
                            break;
                        }
                    }

                    // If a matching color is found, add the card index to the list
                    if (isMatchingColor)
                    {
                        matchingCardsIndices.Add(i);
                    }
                }
            }

            // If there are matching cards, pick a random one from them
            if (matchingCardsIndices.Count > 0)
            {
                randomCardIndex = matchingCardsIndices[UnityEngine.Random.Range(0, matchingCardsIndices.Count)];
            }
            else
            {
                // If no matching cards found, pick a random card
                randomCardIndex = UnityEngine.Random.Range(0, cardObjects.Count);
            }
        }

        // Ensure that randomCardIndex is within the valid range
        if (randomCardIndex >= 0 && randomCardIndex < cardObjects.Count)
        {
            // Set the selected card as "in deck"
            cardObjects[randomCardIndex].GetComponent<Card>().setIsInDeck(true);

            // Add colors if a powerful card is found or if it's picked due to color matching
            if (foundPowerCard || randomCardIndex != -1)
            {
                addColors(cardObjects[randomCardIndex]);
            }

            // Move the selected card to the center
            StartCoroutine(centerSelectedCard(cardObjects[randomCardIndex], 0.3f, centerPosition.transform.position));
            StartCoroutine(MoveCardsOff());
        }
        else
        {
            Debug.LogError("Random card index is out of range.");
        }

        yield return null;
    }


    public bool IsPowerfulCard(string cardName)
    {
        return Array.Exists(powerCardNames, element => element == cardName);
    }


    public bool checkForColorMatch(GameObject card)
    {
        string cardColors = card.GetComponent<Card>().getColor();

        if (cardColors == "[]")
        {
            return true;
        }

        cardColors = cardColors.Replace("[", "").Replace("]", "").Replace("\"", "");

        string[] letters = cardColors.Split(',');

        foreach (string color in letters)
        {
            if (!colors.Contains(color))
            {
                return true;
            }
        }
        return false;
    }

    public void addColors(GameObject card)
    {
        string cardColors = card.GetComponent<Card>().getColor();

        if (cardColors == "[]")
        {
            return;
        }
        
        cardColors = cardColors.Replace("[", "").Replace("]", "").Replace("\"", "");

        string[] letters = cardColors.Split(',');

        foreach (string color in letters)
        {
            if (!colors.Contains(color))
            {
                colors.Add(color);
            }
        }
    }

    public bool CheckForExtraColors(GameObject card)
    {
        string cardColors = card.GetComponent<Card>().getColor();
        return cardColors != "[]" && !colors.Contains(cardColors);
    }

    public void nextPack()
    {
        int numSkipped = 0;
        for (int card = 0; card < cardObjects.Count; card++)
        {
            Card cardScript = cardObjects[card].GetComponent<Card>();
            if (!cardScript.getIsInDeck())
            {
                cardObjects[card].transform.position = startPosObjects[card - numSkipped].transform.position;
                cardObjects[card].transform.rotation = Quaternion.Euler(-180f, 180f, 0f);
            }
            else
            {
                numSkipped++;
            }
        }

        StartCoroutine(dropDownCards());
    }

    public IEnumerator setCubeList()
    {
        string url = "cubecobra.com/cube/api/cubelist/";

        if (currentFormat == format.vintage)
        {
            url += "LSVCube";
        }
        else if (currentFormat == format.pauper)
        {
            url += "thepaupercube";
        }
        if (currentFormat != format.randomCards)
        {

            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Error: " + webRequest.error);
                }
                else
                {
                    string[] lines = webRequest.downloadHandler.text.Split('\n');

                    foreach (string line in lines)
                    {
                        string trimmedLine = line.Trim();

                        cubeList.Add(trimmedLine);
                    }
                }
            }
        }

        instantiateCardObjects();
    }

    public void makePacks()
    {
        if (currentFormat != format.randomCards)
        {
            int cardToAdd = 0;
            string cardName;

            for (int card = 0; card < 15; card++)
            {
                do
                {
                    cardToAdd = UnityEngine.Random.Range(0, cubeList.Count);
                    pack[card] = cubeList[cardToAdd];
                    cardName = cubeList[cardToAdd];
                    cubeList.RemoveAt(cardToAdd);
                }
                while (GameObject.Find(cardName) != null);

                cardObjects[card].name = cardName;
            }
            StartCoroutine(getApiCardInfo(false));
        }
        else
        {
            StartCoroutine(getApiCardInfo(true));
        }
    }


    public IEnumerator getApiCardInfo(bool isRandomFormat)
    {
        int count = 0;

        if (!isRandomFormat)
        {
            for (int card = 0; card < 15; card++, count++)
            {
                string cardName = pack[card];
                cardName = UnityWebRequest.EscapeURL(cardName);
                string url = "https://api.scryfall.com/cards/named?exact=" + cardName;

                using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
                {
                    yield return webRequest.SendWebRequest();

                    if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                    {
                        Debug.LogError("Error: " + webRequest.error);
                    }
                    else
                    {
                        Card cardScript = cardObjects[count].GetComponent<Card>();
                        cardScript.setCardName(pack[card]);
                        cardScript.setManaCost(getManaCostFromResponse(webRequest.downloadHandler.text));
                        cardScript.setCardType(getCardTypeFromResponse(webRequest.downloadHandler.text));
                        cardScript.setColor(getColorsFromResponse(webRequest.downloadHandler.text));
                        StartCoroutine(setCardObjectTexture(webRequest.downloadHandler.text, cardObjects[count]));
                    }
                }
            }
        }
        else
        {
            for (int card = 0; card < 15; card++, count++)
            {
                string url = "https://api.scryfall.com/cards/random";

                using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
                {
                    yield return webRequest.SendWebRequest();

                    if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                    {
                        Debug.LogError("Error: " + webRequest.error);
                    }
                    else
                    {
                        Card cardScript = cardObjects[count].GetComponent<Card>();
                        cardObjects[count].name = getCardNameFromResponse(webRequest.downloadHandler.text);
                        cardScript.setCardName(getCardNameFromResponse(webRequest.downloadHandler.text));
                        cardScript.setManaCost(getManaCostFromResponse(webRequest.downloadHandler.text));
                        cardScript.setCardType(getCardTypeFromResponse(webRequest.downloadHandler.text));
                        cardScript.setColor(getColorsFromResponse(webRequest.downloadHandler.text));
                        StartCoroutine(setCardObjectTexture(webRequest.downloadHandler.text, cardObjects[count]));
                    }
                }
            }
        }

        StartCoroutine(dropDownCards());
    }

    public string getCardNameFromResponse(string response)
    {
        response = response.Substring(response.IndexOf(",\"name\"") + 9);
        response = response.Substring(0, response.IndexOf("\""));

        return response;
    }

    public int getManaCostFromResponse(string response)
    {
        response = response.Substring(response.IndexOf("\"cmc\":") + 6);
        response = response.Substring(0, response.IndexOf("."));

        try
        {
            int manaCost = int.Parse(response);
            return manaCost;
        }
        catch
        {
            Debug.Log("Could not parse");
        }
        return 0;
    }

    public string getCardTypeFromResponse(string response)
    {
        response = response.Substring(response.IndexOf("\"type_line\":\"") + 13);
        response = response.Substring(0, response.IndexOf("\""));

        return response;
    }

    public string getColorsFromResponse(string response)
    {
        response = response.Substring(response.IndexOf("colors\":") + 8);
        response = response.Substring(0, response.IndexOf("\"color") - 1);

        return response;
    }

    public IEnumerator setCardObjectTexture(string response, GameObject cardObject)
    {
        response = response.Substring(response.IndexOf("\"border_crop\":") + 15);
        response = response.Substring(0, response.IndexOf(",\"mana_cost\"") - 2);
        string imageUrl = response;

        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error downloading image: " + www.error);
            }
            else
            {
                Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;

                Renderer renderer = cardObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material material = renderer.material;
                    if (material != null)
                    {
                        material.mainTexture = texture;
                    }
                    else
                    {
                        Debug.LogError("Material not found on card object.");
                    }
                }
                else
                {
                    Debug.LogError("Renderer not found on card object.");
                }
            }
        }
    }

    private IEnumerator zoomInCard(GameObject card, float glideDuration, Vector3 targetPos)
    {
        canClick = false;
        canZoom = false;
        if (cardIsUp)
        {
            StartCoroutine(zoomOutCard(cardUp, glideDuration, cardUp.transform.position, originalPos));
        }
        cardUp = card;
        cardIsUp = true;
        originalPos = card.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < glideDuration)
        {
            float t = elapsedTime / glideDuration;

            card.transform.position = Vector3.Lerp(originalPos, targetPos, t);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        card.transform.position = targetPos;

        canClick = true;
        canZoom = true;
    }

    private IEnumerator zoomOutCard(GameObject card, float glideDuration, Vector3 currentPos, Vector3 targetPos)
    {
        canClick = false;
        canZoom = false;
        cardIsUp = false;

        float elapsedTime = 0f;

        while (elapsedTime < glideDuration)
        {
            float t = elapsedTime / glideDuration;

            card.transform.position = Vector3.Lerp(currentPos, targetPos, t);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        card.transform.position = targetPos;
        canClick = true;
        canZoom = true;
    }

    private IEnumerator centerSelectedCard(GameObject card, float glideDuration, Vector3 targetPos)
    {
        canClick = false;
        canZoom = false;
        Vector3 startPosition = card.transform.position;

        float elapsedTime = 0f;

        while (elapsedTime < glideDuration)
        {
            float t = elapsedTime / glideDuration;

            card.transform.position = Vector3.Lerp(startPosition, targetPos, t);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        card.transform.position = targetPos;

        yield return new WaitForSeconds(1f);

        Card cardScript = card.GetComponent<Card>();

        StartCoroutine(moveSelectedToDeck(card, 0.1f, manaSlotPositions[Mathf.Clamp(cardScript.getManaCost(), 0, 8)].transform.position));
    }

    private IEnumerator moveSelectedToDeck(GameObject card, float glideDuration, Vector3 targetPos)
    {
        if (!isBot)
        {
            downloader.deckList.Add(card.name);
        }

        float elapsedTime = 0f;
        Card cardScript = card.GetComponent<Card>();
        Vector3 centerPosition = card.transform.position;
        for (int i = 0; i < cardInManaSlot[Mathf.Clamp(cardScript.getManaCost(), 0, 8)]; i++)
        {
            targetPos = new Vector3(targetPos.x, targetPos.y - 0.2f, targetPos.z + 0.01f);
        }

        while (elapsedTime < glideDuration)
        {
            float t = elapsedTime / glideDuration;

            card.transform.position = Vector3.Lerp(centerPosition, targetPos, t);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        card.transform.position = targetPos;

        cardInManaSlot[Mathf.Clamp(cardScript.getManaCost(), 0, 8)]++;

        cardsInDeck++;
        string typeLine = cardScript.getCardType();
        bool hasFoundType = false;
        for (int i = 0; i < cardTypes.Length; i++)
        {
            if (typeLine.IndexOf(cardTypes[i]) != -1f)
            {
                hasFoundType = true;
                cardsOfEachType[i]++;
            }
        }

        if (!hasFoundType)
        {
            cardsOfEachType[6]++;
        }

        typeDisplayText.text = "Deck: " + cardsInDeck + "   Land: " + cardsOfEachType[0] + "  Creature: " + cardsOfEachType[1] + "  Spells: " + (cardsOfEachType[2] + cardsOfEachType[3]) + "  Artifacts: " + cardsOfEachType[4] + "  Enchantments: " + cardsOfEachType[5] + " Other: " + cardsOfEachType[6];

        canZoom = true;

        yield return new WaitForSeconds(1f);

        //MoveCardToEnd(card.name);
        cardObjects.Remove(card);
        deckList.Add(card.name);

        hasChosen = true;

        if (allHaveChosen())
        {
            if (pickNum < 15)
            {
                if (packNum % 2 == 0)
                {
                    GameRunner bigRunner = GameObject.Find("Game Runner").GetComponent<GameRunner>();
                    bigRunner.ShiftCardObjectsBetweenScriptsRight();
                }
                else
                { 
                    GameRunner bigRunner = GameObject.Find("Game Runner").GetComponent<GameRunner>();
                    bigRunner.ShiftCardObjectsBetweenScripts();
                }
            }
            else if (packNum < 3)
            {
                yield return new WaitForSeconds(0.5f);
                for (int i = 0; i < allRunners.Length; i++)
                {
                    allRunners[i].packNum++;
                    allRunners[i].pickNum = 0;
                    allRunners[i].instantiateCardObjects();
                }
            }
            else
            {
                saveScreen.SetActive(true);
            }
        }
    }

    public bool allHaveChosen()
    {
        for (int i = 0; i < allRunners.Length; i++)
        {
            if (!allRunners[i].hasChosen)
            {
                return false;
            }
        }

        return true;
    }

    public IEnumerator MoveCardsOff()
    {
        float timeElapsed = 0f;
        float[] targetPosX = new float[cardObjects.Count];
        float offSet = 0;
        if (packNum == 1 || packNum == 3)
        {
            offSet = 30f;
        }
        else
        {
            offSet = -30f;
        }

        for (int i = 0; i < targetPosX.Length; i++)
        {
            targetPosX[i] = cardObjects[i].transform.position.x + offSet;
        }

        while (timeElapsed < 2f)
        {
            for (int i = 0; i < cardObjects.Count; i++)
            {
                Card cardScript = cardObjects[i].GetComponent<Card>();
                if (!cardScript.getIsInDeck())
                {
                    Vector3 startPos = cardObjects[i].transform.position;
                    float endXPos = targetPosX[i];

                    float t = timeElapsed / 10f;
                    float newX = Mathf.Lerp(startPos.x, endXPos, Mathf.SmoothStep(0f, 2.5f, t));
                    cardObjects[i].transform.position = new Vector3(newX, startPos.y, startPos.z);
                }
            }

            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }

    public IEnumerator moveToSideboard(GameObject card, float glideDuration, Vector3 targetPos)
    {
        if (!isBot)
        {
            for (int i = 0; i < downloader.deckList.Count; i++)
            {
                if (downloader.deckList[i] == card.name)
                {
                    downloader.deckList.RemoveAt(i);
                    downloader.sideboard.Add(card.name);
                    break;
                }
            }
        }

        for (int i = 0; i < deckList.Count; i++)
        {
            if (deckList[i] == card.name)
            {
                sideboardList.Add(deckList[i]);
                deckList.RemoveAt(i);
                break;
            }
        }
        Card cardScript = card.GetComponent<Card>();
        cardScript.setIsInSideboard(true);

        for (int i = 0; i < cardInManaSlot[9]; i++)
        {
            targetPos = new Vector3(targetPos.x, targetPos.y - 0.2f, targetPos.z + 0.01f);
        }

        GameObject[] cards = GameObject.FindGameObjectsWithTag("card");

        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i].transform.position.x == card.transform.position.x && cards[i].transform.position.y < card.transform.position.y)
            {
                if (cards[i].GetComponent<Card>().getIsInDeck())
                {
                    cards[i].transform.position = new Vector3(cards[i].transform.position.x, cards[i].transform.position.y + 0.2f, cards[i].transform.position.z - 0.01f);
                }
            }
        }

        Vector3 startPosition = card.transform.position;
        cardInManaSlot[9]++;
        cardInManaSlot[Mathf.Clamp(cardScript.getManaCost(), 0, 8)]--;

        float elapsedTime = 0f;

        while (elapsedTime < glideDuration)
        {
            float t = elapsedTime / glideDuration;

            card.transform.position = Vector3.Lerp(startPosition, targetPos, t);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        card.transform.position = targetPos;

        cardsInDeck--;
        string typeLine = cardScript.getCardType();
        bool hasFoundType = false;
        for (int i = 0; i < cardTypes.Length; i++)
        {
            if (typeLine.IndexOf(cardTypes[i]) != -1f)
            {
                hasFoundType = true;
                cardsOfEachType[i]--;
            }
        }

        if (!hasFoundType)
        {
            cardsOfEachType[6]--;
        }

        typeDisplayText.text = "Deck: " + cardsInDeck + "   Land: " + cardsOfEachType[0] + "  Creature: " + cardsOfEachType[1] + "  Spells: " + (cardsOfEachType[2] + cardsOfEachType[3]) + "  Artifacts: " + cardsOfEachType[4] + "  Enchantments: " + cardsOfEachType[5] + " Other: " + cardsOfEachType[6];
    }

    public IEnumerator moveOutOfSideboard(GameObject card, float glideDuration)
    {
        if (!isBot)
        {
            for (int i = 0; i < downloader.sideboard.Count; i++)
            {
                if (downloader.sideboard[i] == card.name)
                {
                    downloader.sideboard.RemoveAt(i);
                    downloader.deckList.Add(card.name);
                    break;
                }
            }
        }

        for (int i = 0; i < sideboardList.Count; i++)
        {
            if (sideboardList[i] == card.name)
            {
                deckList.Add(sideboardList[i]);
                sideboardList.RemoveAt(i);
                break;
            }
        }
        
        Card cardScript = card.GetComponent<Card>();
        cardScript.setIsInSideboard(false);

        Vector3 startPosition = card.transform.position;
        Vector3 targetPos = manaSlotPositions[Mathf.Clamp(cardScript.getManaCost(), 0, 8)].transform.position;

        for (int i = 0; i < cardInManaSlot[Mathf.Clamp(cardScript.getManaCost(), 0, 8)]; i++)
        {
            targetPos = new Vector3(targetPos.x, targetPos.y - 0.2f, targetPos.z + 0.01f);
        }

        GameObject[] cards = GameObject.FindGameObjectsWithTag("card");

        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i].transform.position.x == card.transform.position.x && cards[i].transform.position.y < card.transform.position.y)
            {
                if (cards[i].GetComponent<Card>().getIsInDeck())
                {
                    cards[i].transform.position = new Vector3(cards[i].transform.position.x, cards[i].transform.position.y + 0.2f, cards[i].transform.position.z - 0.01f);
                }
            }
        }

        cardInManaSlot[9]--;
        cardInManaSlot[Mathf.Clamp(cardScript.getManaCost(), 0, 8)]++;

        float elapsedTime = 0f;

        while (elapsedTime < glideDuration)
        {
            float t = elapsedTime / glideDuration;

            card.transform.position = Vector3.Lerp(startPosition, targetPos, t);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        card.transform.position = targetPos;

        cardsInDeck++;
        string typeLine = cardScript.getCardType();
        bool hasFoundType = false;
        for (int i = 0; i < cardTypes.Length; i++)
        {
            if (typeLine.IndexOf(cardTypes[i]) != -1f)
            {
                hasFoundType = true;
                cardsOfEachType[i]++;
            }
        }

        if (!hasFoundType)
        {
            cardsOfEachType[6]++;
        }

        typeDisplayText.text = "Deck: " + cardsInDeck + "   Land: " + cardsOfEachType[0] + "  Creature: " + cardsOfEachType[1] + "  Spells: " + (cardsOfEachType[2] + cardsOfEachType[3]) + "  Artifacts: " + cardsOfEachType[4] + "  Enchantments: " + cardsOfEachType[5] + " Other: " + cardsOfEachType[6];
    }
}
