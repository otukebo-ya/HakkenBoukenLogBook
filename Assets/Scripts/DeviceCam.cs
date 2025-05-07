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
            // 撮影を行うためのボタン
            _captureButton.onClick.AddListener(OnCaptureButtonClicked);
        }

        public void CameraOn()
        {
            devices = WebCamTexture.devices;

            webCamTexture = new WebCamTexture(devices[0].name);
            _rawImage.texture = webCamTexture;
            webCamTexture.Play();

            // カメラから取得した映像のサイズを記録
            _imageWidth = webCamTexture.width;
            _imageHeight = webCamTexture.height;

            // カメラ映像がスマホ画面に収まるよう調整
            AdjustRawImage();
        }

        public void CameraOff()
        {
            if (webCamTexture != null && webCamTexture.isPlaying)
            {
                webCamTexture.Stop(); // カメラ停止
            }

            gameObject.SetActive(false);
        }

        // スマホのサイズにカメラ映像を合わせる
        void AdjustRawImage()
        {
            RectTransform canvasRT = _rawImage.canvas.GetComponent<RectTransform>();
            Vector2 canvasSize = canvasRT.rect.size;

            // 横幅が大きいほど、比が大きくなる
            float canvasAspect = canvasSize.x / canvasSize.y;
            float camAspect = (float)webCamTexture.width / webCamTexture.height;

            float width, height;
            if (camAspect > canvasAspect) // 横幅をキャンバスに合わせる
            {
                width = canvasSize.x;
                height = width / camAspect;
            }
            else　// タテ幅をキャンバスに合わせる
            {
                height = canvasSize.y;
                width = height * camAspect;
            }

            // 画像データの配置を初期化
            RectTransform rt = _rawImage.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(width, height);
            rt.anchorMin = new Vector2(0.5f, 1.0f);
            rt.anchorMax = new Vector2(0.5f, 1.0f);
            rt.pivot = new Vector2(0.5f, 1.0f);
            rt.anchoredPosition = new Vector2(0, -100f);

            rt.localEulerAngles = new Vector3(0, 0, -webCamTexture.videoRotationAngle);
        }

        // 撮影ボタンの処理
        public void OnCaptureButtonClicked()
        {
            // 空のテクスチャを作成し、カメラ映像をそこに保存
            Texture2D photo = new Texture2D(webCamTexture.width, webCamTexture.height);
            photo.SetPixels(webCamTexture.GetPixels());
            photo.Apply();

            byte[] imageBytes = photo.EncodeToPNG();

            // 画像の書き込み
            string filePath = Path.Combine(Application.persistentDataPath, "CapturedImage.png");
            File.WriteAllBytes(filePath, imageBytes);

            Debug.Log("画像保存先: " + filePath);

            // 撮影された画像を小さく表示
            DiscoveryInputForm.Instance.SetCapturedImage(photo, filePath);
            
            // 撮影した画像がアンドロイドのフォトアプリに乗るように
            AddImageToGallery(filePath);

            // 画像の情報を保存しておく
            _capturedImagePath = filePath;
            _capturedImage = photo;
            CameraOff();
        }

        // 画像をほかのアプリでも見れるように
        // アンドロイドのみで行える処理
        private void AddImageToGallery(string filePath)
        {
#if UNITY_ANDROID
    if (Application.platform == RuntimePlatform.Android)
    {
        // UnityPlayer の currentActivity を取得
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        using (AndroidJavaClass mediaScanner = new AndroidJavaClass("android.media.MediaScannerConnection"))
        {
            mediaScanner.CallStatic("scanFile", currentActivity, new string[] { filePath }, null, null);
        }
    }
#endif
        }

    }
}