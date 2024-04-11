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

    public int[] cardsOfEachType = new int[7];
    public int[] cardInManaSlot = new int[10];
    public int cardsInDeck = 0;

    public List<string> cubeList = new List<string>();

    public bool canClick;
    public bool cardIsUp;

    public TextMeshProUGUI typeDisplayText;

    public Vector3 originalPos;

    public GameObject cardPrefab;
    public GameObject cardUp;

    // Start is called before the first frame update
    void Start()
    {
        canClick = false;

        typeDisplayText.text = "Deck: 0   Land: 0  Creature: 0  Spells: 0  Artifacts: 0  Enchantments: 0 Other: 0";

        setCubeList();
        instantiateCardObjects();
    }

    // Update is called once per frame
    void Update()
    {
        if (canClick)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject.tag == "card")
                {
                    if (Input.GetMouseButtonDown(0) && canClick && !cardIsUp)
                    {
                        cardIsUp = false;
                        StartCoroutine(centerSelectedCard(hit.collider.gameObject, 0.3f, new Vector3(0f, 1.13f, 7.36000013f)));
                    }
                    if (Input.GetMouseButtonDown(1) && canClick)
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


    private void instantiateCardObjects()
    {
        for (int card = 0; card <= 14; card++)
        {
            GameObject startPosObject = GameObject.Find("Card Starts " + (card + 1));
            Vector3 startPos = new Vector3(startPosObject.transform.position.x, startPosObject.transform.position.y, startPosObject.transform.position.z);
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
        float timeElapsed = 0f;

        while (timeElapsed < 2f)
        {
            for (int i = 0; i < cardObjects.Length; i++)
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

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < cardObjects.Length; i++)
        {
            int targetYPosIndex = i;
            if (targetYPosIndex > 14)
            {
                targetYPosIndex = i % 15;
            }
            cardObjects[i].transform.position = new Vector3(cardObjects[i].transform.position.x, targetDropPos[targetYPosIndex], cardObjects[i].transform.position.z);
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
                cardObjects[i].transform.Rotate(Vector3.up, deltaRotation);
            }

            totalRotation += deltaRotation;

            yield return null;
        }

        canClick = true;


        for (int i = 0; i < cardObjects.Length; i++)
        {
            cardObjects[i].transform.rotation = Quaternion.Euler(-180f, 0f, 0f);
        }
    }

    public void setCubeList()
    {
        string filePath;

        filePath = Path.Combine(Application.dataPath, "_Scripts", "vintage_cube.txt");

        if (File.Exists(filePath))
        {
            using (StreamReader fileReader = new StreamReader(filePath))
            {
                string line;
                while ((line = fileReader.ReadLine()) != null)
                {
                    cubeList.Add(line);
                }
            }
        }
        else
        {
            Debug.LogError("The file does not exist: " + filePath);
        }
    }

    public void makePacks()
    {
        int cardToAdd = 0;

        for (int card = 0; card < 15; card++)
        {
            cardToAdd = Random.Range(0, cubeList.Count);
            pack[card] = cubeList[cardToAdd];
            cubeList.RemoveAt(cardToAdd);
        }

        StartCoroutine(getApiCardInfo());
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
                     StartCoroutine(setCardObjectTexture(webRequest.downloadHandler.text, cardObjects[count]));
                     // Debug.Log("API Response: " + webRequest.downloadHandler.text);
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
    }

    private IEnumerator zoomOutCard(GameObject card, float glideDuration, Vector3 currentPos, Vector3 targetPos)
    {
        canClick = false;
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
    }

    private IEnumerator centerSelectedCard(GameObject card, float glideDuration, Vector3 targetPos)
    {
        canClick = false;
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

    }
}
