using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using Unity.VisualScripting;
using TMPro;

public class DraftRunner : MonoBehaviour
{
    public GameObject[] cardObjects = new GameObject[15];
    public GameObject[] manaSlotPositions = new GameObject[10];
    public GameObject[] startPosObjects = new GameObject[15];
    public GameObject centerPosition;

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
    public List<string> colors = new List<string>();

    public int[] cardsOfEachType = new int[7];
    public int[] cardInManaSlot = new int[10];
    public int packNum = 1;
    public int pickNum = 0;
    public int cardsInDeck = 0;
    public int playernum;

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

    // Start is called before the first frame update
    void Start()
    {
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
                        for (int i = 0; i < cardObjects.Length; i++)
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
            cardObjects[card] = Instantiate(cardPrefab, startPos, Quaternion.identity);
        }

        for (int i = 0; i < cardObjects.Length; i++)
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
            for (int i = 0; i < cardObjects.Length; i++)
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

        for (int i = 0; i < cardObjects.Length; i++)
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

            for (int i = 0; i < 15; i++)
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

        for (int i = 0; i < cardObjects.Length; i++)
        {
            cardObjects[i].transform.rotation = Quaternion.Euler(-180f, 0f, 0f);
        }

        if (isBot)
        {
            yield return new WaitForSeconds(Random.Range(3f, 5f));
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
        int randomCard = 0;
        int higestPowerIndex = powerCardNames.Length + 1;
        for (int i = 0; i < cardObjects.Length; i++)
        {
            if (!cardObjects[i].GetComponent<Card>().getIsInDeck())
            {
                for (int j = 0; j < powerCardNames.Length; j++)
                {
                    if (cardObjects[i].name == powerCardNames[j])
                    {
                        if (j < higestPowerIndex)
                        {
                            higestPowerIndex = j;
                            randomCard = i;
                        }
                    }
                }
            }
        }
        if (higestPowerIndex != powerCardNames.Length + 1)
        {
            addColors(cardObjects[randomCard]);
        }

        bool foundCardInColor = false;
        bool putInSideboard = false;

        if (higestPowerIndex == powerCardNames.Length + 1)
        {
            if (colors.Count > 1 || (colors.Count > 0 && Random.Range(0, 3) == 0))
            {
                List <int> cardsInColorIndex = new List<int>();
                for (int i = 0; i < cardObjects.Length; i++)
                {
                    Card cardStats = cardObjects[i].GetComponent<Card>();
                    if (!cardStats.getIsInDeck())
                    {
                        for (int cardObj = 0; cardObj < cardObjects.Length; cardObj++)
                        {
                            for (int j = 0; j < colors.Count; j++)
                            {
                                if (cardObjects[cardObj].GetComponent<Card>().getColor().IndexOf(colors[j]) != -1)
                                {
                                    cardsInColorIndex.Add(cardObj);
                                    foundCardInColor = true;
                                } 
                            }
                        }
                    }
                }
                if (foundCardInColor)
                {
                    randomCard = cardsInColorIndex[Random.Range(0, cardsInColorIndex.Count)];
                    addColors(cardObjects[randomCard]);
                }
            }
            if (!foundCardInColor)
            {
                do
                {
                    randomCard = Random.Range(0, cardObjects.Length);
                }
                while(cardObjects[randomCard].GetComponent<Card>().getIsInDeck());

                if (colors.Count < 2)
                {
                    addColors(cardObjects[randomCard]);
                }
                else if (Random.Range(0, 100) != 69)
                {
                    putInSideboard = true;
                }
            }
        }

        Card cardScript = cardObjects[randomCard].GetComponent<Card>();
        cardScript.setIsInDeck(true);
        StartCoroutine(centerSelectedCard(cardObjects[randomCard], 0.3f, centerPosition.transform.position));
        StartCoroutine(MoveCardsOff());

        if (putInSideboard)
        {
            yield return new WaitForSeconds(1.75f);

            StartCoroutine(moveToSideboard(cardObjects[randomCard], 0.1f, manaSlotPositions[9].transform.position));
        }
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

    public void nextPack()
    {
        int numSkipped = 0;
        for (int card = 0; card < cardObjects.Length; card++)
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
        string url = "cubecobra.com/cube/api/cubelist/LSVCube";

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

        instantiateCardObjects();
    }

    public void makePacks()
    {
        int cardToAdd = 0;
        string cardName;

        for (int card = 0; card < 15; card++)
        {
            do
            {
                cardToAdd = Random.Range(0, cubeList.Count);
                pack[card] = cubeList[cardToAdd];
                cardName = cubeList[cardToAdd];
                cubeList.RemoveAt(cardToAdd);
            }
            while (GameObject.Find(cardName) != null);

            cardObjects[card].name = cardName;
        }

        StartCoroutine(getApiCardInfo());
    }


    public void MoveCardToEnd(string cardName)
    {
        // Find the index of the card with the given name
        int index = -1;
        for (int i = 0; i < cardObjects.Length; i++)
        {
            if (cardObjects[i].name == cardName)
            {
                index = i;
                break;
            }
        }

        // If the card with the given name is found
        if (index != -1)
        {
            // Move the card to the end of the array
            GameObject cardToMove = cardObjects[index];
            for (int i = index; i < cardObjects.Length - 1; i++)
            {
                cardObjects[i] = cardObjects[i + 1];
            }
            cardObjects[cardObjects.Length - 1] = cardToMove;
        }
        else
        {
            Debug.LogError("Card with name " + cardName + " not found!");
        }
    }

    public IEnumerator getApiCardInfo()
    {
        int count = 0;

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

        StartCoroutine(dropDownCards());
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

        MoveCardToEnd(card.name);

        hasChosen = true;

        if (allHaveChosen())
        {
            if (pickNum < 15)
            {
                GameRunner bigRunner = GameObject.Find("Game Runner").GetComponent<GameRunner>();
                bigRunner.ShiftCardObjectsBetweenScripts();
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
        float[] targetPosX = new float[15];

        for (int i = 0; i < targetPosX.Length; i++)
        {
            targetPosX[i] = cardObjects[i].transform.position.x + 30;
        }

        while (timeElapsed < 2f)
        {
            for (int i = 0; i < cardObjects.Length; i++)
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
