using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    private PlayerManager playerManager;

    public GameObject cardItem;

    private List<Card> cards;

    private void Awake()
    {
        playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
        cards = new List<Card>();
    }


    public void InitCards(int categoryId, string path, bool isGame = false, string api = "category/card")
    {
        //string path = "CardViewPage";

        if (!isGame)
            DestoryCards();
        cards.Clear();

        GetCardsFromServer(categoryId, isGame, path, api);
    }

    public void DestoryCards()
    {
        GameObject[] content = GameObject.FindGameObjectsWithTag("cardItem");
        for (int i = 0; i < content.Length; i++)
        {
            Destroy(content[i]);
        }
    }

    public void GetCardsFromServer(int categoryId, bool isGame, string path, string api)
    {
        CardData cardData = new CardData();
        cardData.category_id = categoryId;
        if(playerManager.GetUserId() >= 0)
            cardData.user_id = playerManager.GetUserId();

        var req = JsonConvert.SerializeObject(cardData);
        Debug.Log(api);

        StartCoroutine(DataManager.sendDataToServer(api, req, (raw) =>
        {
            Debug.Log(raw);
            JObject applyJObj = JObject.Parse(raw);
            foreach (JObject tmpCard in applyJObj["cards"])
            {
                Card tmp = new Card();
                tmp.SetCardId((int)tmpCard["card_id"]);
                tmp.SetName(tmpCard["card_name"].ToString());
                tmp.SetCategoryId(categoryId);
                tmp.SetImagePath(tmpCard["card_img_path"].ToString());
                if ((int)tmpCard["card_is_built_in"] == 1)
                {
                    tmp.SetCardIsBuiltIn(true);
                    tmp.SetUserId(-1);
                }
                else
                {
                    tmp.SetCardIsBuiltIn(false);
                    tmp.SetUserId(playerManager.GetUserId());
                }
                
                cards.Add(tmp);
            }

            if(!isGame)
                CreateNewCardItems(cards, path);
            else
                GameManager.isCardLoaded = true;

        }));
    }

    public void CreateNewCardItems(List<Card> cards,  string path)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            GameObject newCardItem = Instantiate(cardItem, new Vector3(0, 0, 0), Quaternion.identity);
            newCardItem.transform.SetParent(GameObject.Find("Canvas").transform.Find(path).transform.Find("CardsScrollView/Viewport/Content").transform);
            newCardItem.transform.localScale = new Vector3(1, 1, 1);
            newCardItem.GetComponent<Card>().SetCardId(cards[i].GetCardId());
            newCardItem.GetComponent<Card>().SetImagePath(cards[i].GetImagePath());
            newCardItem.GetComponent<Card>().SetName(cards[i].GetName());
            if (cards[i].GetCardIsBuiltIn())
            {
                newCardItem.GetComponent<Card>().SetCardIsBuiltIn(true);
                newCardItem.GetComponent<Image>().color = new Color32(210, 211, 241, 255);
            }
            else
            {
                newCardItem.GetComponent<Card>().SetCardIsBuiltIn(false);
                newCardItem.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            }

            newCardItem.GetComponentInChildren<Text>().text = cards[i].GetName();
            StartCoroutine(getImagesFromURL(cards[i].GetImagePath(), newCardItem));
            if (path.Equals("EditCategoryPage")) // 2: tts button , 3: edit img
            {
                newCardItem.transform.GetChild(2).gameObject.SetActive(false);
                if (cards[i].GetCardIsBuiltIn())
                    newCardItem.transform.GetChild(3).gameObject.SetActive(false); 
                else
                    newCardItem.transform.GetChild(3).gameObject.SetActive(true); // edit page & custom -> set edit image visible
            }
            else
            {
                newCardItem.transform.GetChild(2).gameObject.SetActive(true);
                newCardItem.transform.GetChild(3).gameObject.SetActive(false); 
            }
        }
    }

    public List<Card> GetCards()
    {
        return cards;
    }

    public Card GetCardInfo(int id)
    {
        foreach (Card tmp in cards)
        {
            if (tmp.GetCardId().Equals(id))
                return tmp;
        }

        return null;
    }


    public IEnumerator getImagesFromURL(string imgurl, GameObject item, bool isGame = false)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imgurl);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            if(isGame)
                GameManager.isImgLoaded = true;
            item.GetComponentInChildren<RawImage>().texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
        }
    }
}
