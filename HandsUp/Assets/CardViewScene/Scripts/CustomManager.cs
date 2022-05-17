﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CustomManager : MonoBehaviour
{
    private PlayerManager playerManager;
    private CategoryManager categoryManager;
    private ImageManager imageManager;


    public InputField categoryName;
    public InputField cardName;
    public Dropdown dropdown;

    private bool access;
    private int selectedCategoryId;

    private void Start()
    {
        playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
        categoryManager = GameObject.Find("CardViewManager").GetComponent<CategoryManager>();
        imageManager = GameObject.Find("CustomPage").GetComponent<ImageManager>();
        InitPages();
        InitDropdownOptions();
    }

    private void InitPages()
    {
        GameObject.Find("CustomPage").transform.Find("CustomCategoryPage").gameObject.SetActive(true);
        GameObject.Find("CustomPage").transform.Find("Menus/CategoryBtn/Panel").gameObject.SetActive(false);

        GameObject.Find("CustomPage").transform.Find("CustomCardPage").gameObject.SetActive(false);
        GameObject.Find("CustomPage").transform.Find("Menus/CardBtn/Panel").gameObject.SetActive(true);
    }

    //TO-DO: Refactoring Code
    public void OnClickBtn()
    {
        string[] tmp = EventSystem.current.currentSelectedGameObject.name.Split('B');
        if (tmp[0].Equals("Category"))
        {
            GameObject.Find("CustomPage").transform.Find("CustomCategoryPage").gameObject.SetActive(true);
            GameObject.Find("CustomPage").transform.Find("Menus/CategoryBtn/Panel").gameObject.SetActive(false);

            GameObject.Find("CustomPage").transform.Find("CustomCardPage").gameObject.SetActive(false);
            GameObject.Find("CustomPage").transform.Find("Menus/CardBtn/Panel").gameObject.SetActive(true);
        }
        else
        {
            GameObject.Find("CustomPage").transform.Find("CustomCategoryPage").gameObject.SetActive(false);
            GameObject.Find("CustomPage").transform.Find("Menus/CategoryBtn/Panel").gameObject.SetActive(true);

            GameObject.Find("CustomPage").transform.Find("CustomCardPage").gameObject.SetActive(true);
            GameObject.Find("CustomPage").transform.Find("Menus/CardBtn/Panel").gameObject.SetActive(false);
        }
    }

    private void InitDropdownOptions()
    {
        dropdown.options.Clear();
        List<Category> categories = categoryManager.GetAllCategories();
        foreach (Category category in categories)
        {
            dropdown.options.Add(new Dropdown.OptionData(category.GetName()));
        }
        dropdown.value = 0;
        dropdown.RefreshShownValue();
    }

    public void OnDropdownChanged(Dropdown select)
    {
        selectedCategoryId =  categoryManager.GetCategory(select.value).GetId();
    }

    public void OnClickAddCategoryBtn()
    {
        CategoryData categoryData = new CategoryData();
        categoryData.name = categoryName.text;
        categoryData.user_id = playerManager.GetUserId();
        categoryData.access = access;

        var req = JsonConvert.SerializeObject(categoryData);
        StartCoroutine(DataManager.sendDataToServer("category/create", req, (raw) =>
        {
            Debug.Log(raw);
            JObject applyJObj = JObject.Parse(raw);
            if (applyJObj["result"].ToString().Equals("success"))
            {
                Debug.Log("results : success");
                InitCustomPages();
                StartCoroutine(OpenPopUp("카테고리가 추가되었습니다.", true));
            }
            else
            {
                Debug.Log("results : fail");
                StartCoroutine(OpenPopUp("카테고리가 추가되지 않았습니다."));
            }

        }));
    }

    public void OnClickAddCardBtn()
    {
        CardData cardData = new CardData();

        cardData.user_id = playerManager.GetUserId();
        cardData.name = cardName.text;
        cardData.category_id = selectedCategoryId;
        cardData.img_path = imageManager.GetCurrentImgByte();

        var req = JsonConvert.SerializeObject(cardData);
        StartCoroutine(DataManager.sendDataToServer("category/card/create", req, (raw) =>
        {
            Debug.Log(raw);
            JObject applyJObj = JObject.Parse(raw);
            if (applyJObj["result"].ToString().Equals("success"))
            {
                Debug.Log("results : success");
                InitCustomPages();
                StartCoroutine(OpenPopUp("카드가 추가되었습니다.", true));
            }
            else
            {
                Debug.Log("results : fail");
                StartCoroutine(OpenPopUp("카드가 추가되지 않았습니다."));
            }

        }));
    }

    private IEnumerator OpenPopUp(string content, bool isSucess = false)
    {
        GameObject.Find("PopUpPages").transform.Find("AlarmPopUp").GetComponentInChildren<Text>().text = content;
        GameObject.Find("PopUpPages").transform.Find("AlarmPopUp").gameObject.SetActive(true);
        yield return new WaitForSeconds(2.0f);
        GameObject.Find("PopUpPages").transform.Find("AlarmPopUp").gameObject.SetActive(false);
        if (isSucess)
        {
            GameObject.Find("Canvas").transform.Find("ItemAddPage").gameObject.SetActive(true);
            GameObject.Find("Canvas").transform.Find("CustomPage").gameObject.SetActive(false);
        }
    }

    public void InitCustomPages()
    {
        imageManager.InitImage();
        categoryName.text = "";
        cardName.text = "";
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
