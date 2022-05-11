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
    private DetectionManager detectionManager;

    WebCamDevice[] devices;
    int selectedCameraIndex = -1;
    int backCameraIndex = -1;
    int frontCameraIndex = -1;
    WebCamTexture camTexture;
    public RawImage cameraViewImage; //카메라가 보여질 화면
    public Texture2D captureTexture;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        detectionManager = GameObject.Find("ObjectDetectionManager").GetComponent<DetectionManager>();

        //카메라 권한 확인
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

        //후면 카메라 찾기
        for (int i = 0; i < devices.Length; i++)
        {
            if (devices[i].isFrontFacing == true)
            {
                selectedCameraIndex = i;
                frontCameraIndex = i;
                Debug.Log("front: " + selectedCameraIndex);
            }
            else
            {
                backCameraIndex = i;
                Debug.Log("back: " + selectedCameraIndex);
            }
        }
    }

    public void ChangeCamera()
    {
        if (selectedCameraIndex == frontCameraIndex && backCameraIndex != -1)
            selectedCameraIndex = backCameraIndex;
        else if (selectedCameraIndex == backCameraIndex && frontCameraIndex != -1)
            selectedCameraIndex = frontCameraIndex;
    }

    public void CameraOn()
    {
        //카메라 켜기
        if (selectedCameraIndex >= 0)
        {
            camTexture = new WebCamTexture(devices[selectedCameraIndex].name);
            camTexture.requestedFPS = 30;
            cameraViewImage.texture = camTexture;
            camTexture.Play();
            GameObject.Find("GamePage").transform.Find("Btns/CaptureBtn").GetComponent<Button>().interactable = true;
        
        }
    }

    public void CameraOff()
    {
        GameObject.Find("GamePage").transform.Find("Btns/CaptureBtn").GetComponent<Button>().interactable = false;
        if(camTexture != null)
        {
            camTexture.Stop();
            WebCamTexture.Destroy(camTexture);
            camTexture = null;
        }
    }

    public void CaptureScreen()
    {
        GameObject.Find("GamePage").transform.Find("Btns/CaptureBtn").GetComponent<Button>().interactable = false;
        if (camTexture != null)
        {
            Texture2D captureTexture = new Texture2D(camTexture.width, camTexture.height);
            captureTexture.SetPixels(camTexture.GetPixels());
            captureTexture.Apply();
            byte[] jpg = captureTexture.EncodeToJPG();
            string jpgBase64 = System.Convert.ToBase64String(jpg);
            camTexture.Stop();

            // Check Correct/Wrong
            detectionManager.GetResultFromImg(jpgBase64);
        }
    }
}