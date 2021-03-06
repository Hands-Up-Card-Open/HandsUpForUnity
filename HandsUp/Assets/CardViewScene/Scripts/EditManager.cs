using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EditManager : MonoBehaviour
{
    private PlayerManager playerManager;
    private CategoryManager categoryManager;
    private CardManager cardManager;
    private ImageManager imageManager;

    private Category category;
    private Card card;

    public InputField categoryName;
    public InputField cardName;
    public Dropdown dropdown;

    private bool access;
    private int selectedCategoryId;
    private List<int> categoryIdList;

    void Start()
    {
        playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
        categoryManager = this.gameObject.GetComponent<CategoryManager>();
        cardManager = this.gameObject.GetComponent<CardManager>();
        imageManager = GameObject.Find("CardViewManager").GetComponent<ImageManager>();
    }

    public void OnClickEditBtn()
    {
        GameObject.Find("Canvas").transform.Find("CardViewPage").gameObject.SetActive(false);
        GameObject.Find("Canvas").transform.Find("EditCategoryPage").gameObject.SetActive(true);
        if(category.GetName().Equals("전체"))
            cardManager.InitCards(category.GetCategoryId(), "EditCategoryPage", false, "category/card/all");
        else
            cardManager.InitCards(category.GetCategoryId(), "EditCategoryPage");

        //설정 값 불러오기
        GameObject.Find("Canvas").transform.Find("EditCategoryPage/CategoryName").GetComponent<InputField>().text = category.GetName();
        GameObject.Find("Canvas").transform.Find("EditCategoryPage/Toggles/Public").GetComponent<Toggle>().isOn = category.GetAccess();
        
        // 빌트인 카테고리는 수정, 삭제 불가능
        bool isInteractable = !category.GetCategoryIsBuiltIn();
        GameObject.Find("Canvas").transform.Find("EditCategoryPage/CategoryName").GetComponent<InputField>().interactable = isInteractable;
        GameObject.Find("Canvas").transform.Find("EditCategoryPage/Toggles").gameObject.SetActive(isInteractable);
        GameObject.Find("Canvas").transform.Find("EditCategoryPage/Btns/CategoryDeleteBtn").GetComponent<Button>().interactable = isInteractable;
    }

    public void InitEditCategories(Category cate)
    {
        category = cate;
    }

    public void OnClickCategoryEditBtn()
    {
        CategoryData categoryData = new CategoryData();
        categoryData.category_id = category.GetCategoryId();
        categoryData.name = categoryName.text;
        categoryData.user_id = playerManager.GetUserId();
        categoryData.access = access;

        var req = JsonConvert.SerializeObject(categoryData);
        StartCoroutine(DataManager.sendDataToServer("category/update", req, (raw) =>
        {
            Debug.Log(raw);
            JObject applyJObj = JObject.Parse(raw);
            if (applyJObj["result"].ToString().Equals("success"))
            {
                Debug.Log("results : success");
                StartCoroutine(OpenPopUp("카테고리가 수정되었습니다.", true, true));
                category.SetName(categoryName.text);
                category.SetAccess(access);
            }
            else
            {
                Debug.Log("results : fail");
                StartCoroutine(OpenPopUp("카테고리가 수정되지 않았습니다.", false));
            }

        }));
    }

    public void OnClickDeleteBtn()
    {
        string explainTxt;
        if (GameObject.Find("Canvas").transform.Find("PopUpPages/EditCardPopUp").gameObject.activeSelf)
            explainTxt = "해당 카드를 삭제하시겠습니까?";
        else
            explainTxt = "해당 카테고리를 삭제하시겠습니까?";

        GameObject.Find("PopUpPages").transform.Find("DeleteCheckPopUp").gameObject.SetActive(true);
        GameObject.Find("PopUpPages").transform.Find("DeleteCheckPopUp/ExplainTxt").GetComponent<Text>().text = explainTxt;

    }

    public void OnClickCancelBtn()
    {
        GameObject.Find("PopUpPages").transform.Find("DeleteCheckPopUp").gameObject.SetActive(false);
    }

    public void OnClickDeleteCheckBtn()
    {
        if (GameObject.Find("Canvas").transform.Find("PopUpPages/EditCardPopUp").gameObject.activeSelf)
            DeleteCard();
        else
            DeleteCategory();
    }

    public void DeleteCategory()
    {
        CategoryData categoryData = new CategoryData();
        categoryData.category_id = category.GetCategoryId();

        var req = JsonConvert.SerializeObject(categoryData);
        StartCoroutine(DataManager.sendDataToServer("category/delete", req, (raw) =>
        {
            Debug.Log(raw);
            JObject applyJObj = JObject.Parse(raw);
            if (applyJObj["result"].ToString().Equals("success"))
            {
                Debug.Log("results : success");
                StartCoroutine(OpenPopUp("카테고리가 삭제되었습니다.", true, false, true));
            }
            else
            {
                Debug.Log("results : fail");
                StartCoroutine(OpenPopUp("카테고리가 삭제되지 않았습니다.", false));
            }

        }));
    }
    public void InitEditCard(Card ca)
    {
        card = ca;
        //설정 값 불러오기
        StartCoroutine(cardManager.getImagesFromURL(card.GetImagePath(), GameObject.Find("PopUpPages/EditCardPopUp").transform.Find("CardImg").gameObject));
        GameObject.Find("Canvas").transform.Find("PopUpPages/EditCardPopUp/CardName").GetComponent<InputField>().text = card.GetName();

        dropdown.value = categoryManager.GetCategoryIndex(category.GetCategoryId());
        dropdown.Select();
        dropdown.RefreshShownValue();
        selectedCategoryId = category.GetCategoryId();
    }

    public void OnClickCardEditBtn()
    {
        CardData cardData = new CardData();
        cardData.card_id = card.GetCardId();
        cardData.category_id = selectedCategoryId;
        cardData.name = cardName.text;
        cardData.img_path = imageManager.GetCurrentImgByte();

        var req = JsonConvert.SerializeObject(cardData);
        StartCoroutine(DataManager.sendDataToServer("category/card/update", req, (raw) =>
        {
            Debug.Log(raw);
            JObject applyJObj = JObject.Parse(raw);
            if (applyJObj["result"].ToString().Equals("success"))
            {
                Debug.Log("results : success");
                StartCoroutine(OpenPopUp("카드가 수정되었습니다.", true));
                card.SetCategoryId(selectedCategoryId);
                card.SetName(cardName.text);
                card.SetImagePath(imageManager.GetCurrentImgByte());
            }
            else
            {
                Debug.Log("results : fail");
                StartCoroutine(OpenPopUp("카드가 수정되지 않았습니다.", false));
            }

        }));
    }

    public void DeleteCard()
    {
        CardData cardData = new CardData();
        cardData.card_id = card.GetCardId();
        cardData.img_path = card.GetImagePath();

        var req = JsonConvert.SerializeObject(cardData);
        StartCoroutine(DataManager.sendDataToServer("category/card/delete", req, (raw) =>
        {
            Debug.Log(raw);
            JObject applyJObj = JObject.Parse(raw);
            if (applyJObj["result"].ToString().Equals("success"))
            {
                Debug.Log("results : success");
                StartCoroutine(OpenPopUp("카드가 삭제되었습니다.", true));
            }
            else
            {
                Debug.Log("results : fail");
                StartCoroutine(OpenPopUp("카드가 삭제되지 않았습니다.", false));
            }

        }));
    }

    public void InitDropdownOptions()
    {
        dropdown.options.Clear();
        List<Category> categories = categoryManager.GetCategories();
        categoryIdList = new List<int>();
        foreach (Category category in categories)
        {
            if (!category.GetName().Equals("전체"))
            {
                dropdown.options.Add(new Dropdown.OptionData(category.GetName()));
                categoryIdList.Add(category.GetCategoryId());
            }
        }
        dropdown.value = 0;
        dropdown.RefreshShownValue();
    }

    public void OnDropdownChanged(Dropdown select)
    {
        selectedCategoryId = categoryIdList[select.value];
    }

    public void OnClickCloseBtn()
    {
        GameObject.Find("PopUpPages").transform.Find("EditCardPopUp").gameObject.SetActive(false);
    }

    public int GetEditCardsCategory()
    {
        return category.GetCategoryId();
    }

    public string GetEditCardsCategoryName()
    {
        return category.GetName();
    }

    private IEnumerator OpenPopUp(string content, bool isSucess = false, bool editCategory = false, bool deleteCategory = false)
    {
        GameObject.Find("PopUpPages").transform.Find("AlarmPopUp").GetComponentInChildren<Text>().text = content;
        GameObject.Find("PopUpPages").transform.Find("AlarmPopUp").gameObject.SetActive(true);
        yield return new WaitForSeconds(2.0f);
        GameObject.Find("PopUpPages").transform.Find("AlarmPopUp").gameObject.SetActive(false);
        if (isSucess)
        {
            if(editCategory)
            {
                GameObject.Find("Canvas").transform.Find("CardViewPage").gameObject.SetActive(true);
                GameObject.Find("Canvas").transform.Find("EditCategoryPage").gameObject.SetActive(false);
                int category_id = GameObject.Find("CardViewManager").GetComponent<EditManager>().GetEditCardsCategory();
                GameObject.Find("CardViewManager").GetComponent<CardManager>().InitCards(category_id, "CardViewPage");
            }
            else if (deleteCategory)
            {
                GameObject.Find("Canvas").transform.Find("CardViewPage").gameObject.SetActive(true);
                GameObject.Find("CardViewManager").GetComponent<CategoryManager>().InitCategories(false);
                GameObject.Find("Canvas").transform.Find("EditCategoryPage").gameObject.SetActive(false);
                GameObject.Find("PopUpPages").transform.Find("DeleteCheckPopUp").gameObject.SetActive(false);
            }
            else
            {
                GameObject.Find("CardViewManager").GetComponent<CardManager>().InitCards(category.GetCategoryId(), "EditCategoryPage");
                GameObject.Find("PopUpPages").transform.Find("DeleteCheckPopUp").gameObject.SetActive(false);
                GameObject.Find("PopUpPages").transform.Find("EditCardPopUp").gameObject.SetActive(false);
            }
        }
    }

    public void ToggleClick(bool isOn)
    {
        if (isOn)
        {
            access = true;
        }
        else
        {
            access = false;
        }
    }

}
