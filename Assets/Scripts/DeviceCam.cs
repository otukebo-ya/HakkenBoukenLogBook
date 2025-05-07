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
            // �B�e���s�����߂̃{�^��
            _captureButton.onClick.AddListener(OnCaptureButtonClicked);
        }

        public void CameraOn()
        {
            devices = WebCamTexture.devices;

            webCamTexture = new WebCamTexture(devices[0].name);
            _rawImage.texture = webCamTexture;
            webCamTexture.Play();

            // �J��������擾�����f���̃T�C�Y���L�^
            _imageWidth = webCamTexture.width;
            _imageHeight = webCamTexture.height;

            // �J�����f�����X�}�z��ʂɎ��܂�悤����
            AdjustRawImage();
        }

        public void CameraOff()
        {
            if (webCamTexture != null && webCamTexture.isPlaying)
            {
                webCamTexture.Stop(); // �J������~
            }

            gameObject.SetActive(false);
        }

        // �X�}�z�̃T�C�Y�ɃJ�����f�������킹��
        void AdjustRawImage()
        {
            RectTransform canvasRT = _rawImage.canvas.GetComponent<RectTransform>();
            Vector2 canvasSize = canvasRT.rect.size;

            // �������傫���قǁA�䂪�傫���Ȃ�
            float canvasAspect = canvasSize.x / canvasSize.y;
            float camAspect = (float)webCamTexture.width / webCamTexture.height;

            float width, height;
            if (camAspect > canvasAspect) // �������L�����o�X�ɍ��킹��
            {
                width = canvasSize.x;
                height = width / camAspect;
            }
            else�@// �^�e�����L�����o�X�ɍ��킹��
            {
                height = canvasSize.y;
                width = height * camAspect;
            }

            // �摜�f�[�^�̔z�u��������
            RectTransform rt = _rawImage.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(width, height);
            rt.anchorMin = new Vector2(0.5f, 1.0f);
            rt.anchorMax = new Vector2(0.5f, 1.0f);
            rt.pivot = new Vector2(0.5f, 1.0f);
            rt.anchoredPosition = new Vector2(0, -100f);

            rt.localEulerAngles = new Vector3(0, 0, -webCamTexture.videoRotationAngle);
        }

        // �B�e�{�^���̏���
        public void OnCaptureButtonClicked()
        {
            // ��̃e�N�X�`�����쐬���A�J�����f���������ɕۑ�
            Texture2D photo = new Texture2D(webCamTexture.width, webCamTexture.height);
            photo.SetPixels(webCamTexture.GetPixels());
            photo.Apply();

            byte[] imageBytes = photo.EncodeToPNG();

            // �摜�̏�������
            string filePath = Path.Combine(Application.persistentDataPath, "CapturedImage.png");
            File.WriteAllBytes(filePath, imageBytes);

            Debug.Log("�摜�ۑ���: " + filePath);

            // �B�e���ꂽ�摜���������\��
            DiscoveryInputForm.Instance.SetCapturedImage(photo, filePath);
            
            // �B�e�����摜���A���h���C�h�̃t�H�g�A�v���ɏ��悤��
            AddImageToGallery(filePath);

            // �摜�̏���ۑ����Ă���
            _capturedImagePath = filePath;
            _capturedImage = photo;
            CameraOff();
        }

        // �摜���ق��̃A�v���ł������悤��
        // �A���h���C�h�݂̂ōs���鏈��
        private void AddImageToGallery(string filePath)
        {
#if UNITY_ANDROID
    if (Application.platform == RuntimePlatform.Android)
    {
        // UnityPlayer �� currentActivity ���擾
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