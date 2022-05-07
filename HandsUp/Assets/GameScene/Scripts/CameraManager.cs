using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Android;
public class CameraManager : MonoBehaviour
{
    private GameManager gameManager;
    private ObjectDetectionManager objectDetectionManager;

    WebCamDevice[] devices;
    int selectedCameraIndex = -1;
    WebCamTexture camTexture;
    public RawImage cameraViewImage; //ī�޶� ������ ȭ��
    public Texture2D captureTexture;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        objectDetectionManager = GameObject.Find("ObjectDetectionManager").GetComponent<ObjectDetectionManager>();

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

        devices = WebCamTexture.devices;

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
    }

    public void CameraOn()
    {
        //ī�޶� �ѱ�
        if (selectedCameraIndex >= 0)
        {
            camTexture = new WebCamTexture(devices[selectedCameraIndex].name);
            camTexture.requestedFPS = 30;
            cameraViewImage.texture = camTexture;
            camTexture.Play();
            GameObject.Find("GamePage").transform.Find("CaptureBtn").GetComponent<Button>().interactable = true;
        
        }
    }

    public void CaptureScreen()
    {
        GameObject.Find("GamePage").transform.Find("CaptureBtn").GetComponent<Button>().interactable = false;
        if (camTexture != null)
        {
            Texture2D captureTexture = new Texture2D(camTexture.width, camTexture.height);
            captureTexture.SetPixels(camTexture.GetPixels());
            captureTexture.Apply();
            byte[] jpg = captureTexture.EncodeToJPG();
            string jpgBase64 = System.Convert.ToBase64String(jpg);
            camTexture.Stop();

            // Check Correct/Wrong
            objectDetectionManager.GetObjectFromImg(jpgBase64);
        }
    }
}