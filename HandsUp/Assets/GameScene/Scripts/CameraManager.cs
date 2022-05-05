using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Android;
public class CameraManager : MonoBehaviour
{
    WebCamTexture camTexture;
    public RawImage cameraViewImage; //ī�޶� ������ ȭ��
    public Texture2D captureTexture;

    private GameData gameData;

    void Start()
    {
        //ī�޶� ���� Ȯ��
        /*
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }
        */

        if (WebCamTexture.devices.Length == 0)
        {
            Debug.Log("no camera!");
            return;
        }

        WebCamDevice[] devices = WebCamTexture.devices;
        int selectedCameraIndex = -1;

        //�ĸ� ī�޶� ã��
        for (int i = 0; i < devices.Length; i++)
        {
            if (devices[i].isFrontFacing == true)
            {
                selectedCameraIndex = i;
                Debug.Log(selectedCameraIndex);
                break;
            }
        }

        //ī�޶� �ѱ�
        if (selectedCameraIndex >= 0)
        {
            camTexture = new WebCamTexture(devices[selectedCameraIndex].name);
            camTexture.requestedFPS = 30;
            cameraViewImage.texture = camTexture;
            camTexture.Play();
        }
    }

    public void CaptureScreen()
    {
        if (camTexture != null)
        {
            Texture2D captureTexture = new Texture2D(camTexture.width, camTexture.height);
            captureTexture.SetPixels(camTexture.GetPixels());
            captureTexture.Apply();
            byte[] jpg = captureTexture.EncodeToJPG();
            string jpgBase64 = System.Convert.ToBase64String(jpg);
            GetObjectFromImg(jpgBase64);
        }
    }

    public void GetObjectFromImg(string jpgBase64)
    {
        gameData = new GameData();
        gameData.base64 = jpgBase64;
        var req = JsonConvert.SerializeObject(gameData);

        StartCoroutine(DataManager.sendDataToServer("game/result/object", req, (raw) =>
        {
            Debug.Log(raw);
            JObject res = JObject.Parse(raw);

            //if(res.result[')


        }));
    }


}