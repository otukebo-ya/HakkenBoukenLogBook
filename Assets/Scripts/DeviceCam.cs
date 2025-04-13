using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace ColorBath
{
    public class DeviceCam : MonoBehaviour
    {
        private int _imageWidth;
        private int _imageHeight;
        [SerializeField] private RawImage _rawImage;
        [SerializeField] private Button _captureButton;

        WebCamTexture webCamTexture;
        private string _capturedImagePath;
        private Texture2D _capturedImage;

        WebCamDevice[] devices;

        void Start()
        {
            _captureButton.onClick.AddListener(OnCaptureButtonClicked);
        }

        public void CameraOn()
        {
            devices = WebCamTexture.devices;

            webCamTexture = new WebCamTexture(devices[0].name);
            _rawImage.texture = webCamTexture;
            webCamTexture.Play();
            _imageWidth = webCamTexture.width;
            _imageHeight = webCamTexture.height;
            AdjustRawImage();
        }

        public void CameraOff()
        {
            if (webCamTexture != null && webCamTexture.isPlaying)
            {
                webCamTexture.Stop(); // ÉJÉÅÉâí‚é~
            }

            gameObject.SetActive(false);
        }

        void AdjustRawImage()
        {
            RectTransform canvasRT = _rawImage.canvas.GetComponent<RectTransform>();
            Vector2 canvasSize = canvasRT.rect.size;

            float canvasAspect = canvasSize.x / canvasSize.y;
            float camAspect = (float)webCamTexture.width / webCamTexture.height;

            RectTransform rt = _rawImage.GetComponent<RectTransform>();

            float width, height;
            if (camAspect > canvasAspect)
            {
                width = canvasSize.x;
                height = width / camAspect;
            }
            else
            {
                height = canvasSize.y;
                width = height * camAspect;
            }

            rt.sizeDelta = new Vector2(width, height);
            rt.anchorMin = new Vector2(0.5f, 1.0f);
            rt.anchorMax = new Vector2(0.5f, 1.0f);
            rt.pivot = new Vector2(0.5f, 1.0f);
            rt.anchoredPosition = new Vector2(0, -100f);

            rt.localEulerAngles = new Vector3(0, 0, -webCamTexture.videoRotationAngle);
        }

        public void OnCaptureButtonClicked()
        {
            Texture2D photo = new Texture2D(webCamTexture.width, webCamTexture.height);
            photo.SetPixels(webCamTexture.GetPixels());
            photo.Apply();

            byte[] imageBytes = photo.EncodeToPNG();

            string filePath = Path.Combine(Application.persistentDataPath, "CapturedImage.png");
            File.WriteAllBytes(filePath, imageBytes);

            Debug.Log("âÊëúï€ë∂êÊ: " + filePath);
            DiscoveryInputForm.Instance.SetCapturedImage(photo);
            AddImageToGallery(filePath);
            _capturedImagePath = filePath;
            _capturedImage = photo;
            CameraOff();
        }

        // androidÇÃèÍçáÇÃÇ›
        private void AddImageToGallery(string filePath)
        {
#if UNITY_ANDROID
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass mediaScanner = new AndroidJavaClass("android.media.MediaScannerConnection"))
            {
                AndroidJavaObject context = new AndroidJavaObject("android.content.ContextWrapper", UnityPlayer.currentActivity);
                mediaScanner.CallStatic("scanFile", context, new string[] { filePath }, null, null);
            }
        }
#endif
        }
    }
}