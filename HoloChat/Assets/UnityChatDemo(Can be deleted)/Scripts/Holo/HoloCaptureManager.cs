﻿#if UNITY_EDITOR ||UNITY_WSA
using HoloCapture;
#endif

using System.Collections.Generic;
using UnityEngine;
public class HoloCaptureManager : MonoBehaviour {

    public static HoloCaptureManager Instance;
    private void Awake()
    {
        Instance = this;
    }
#if UNITY_EDITOR || UNITY_WSA
    //https://docs.microsoft.com/zh-cn/windows/mixed-reality/locatable-camera
    public enum HoloType 
    { 
        Holo1,
        Holo2,
    }

    public enum HoloCamFrame
    {
        Holo15,
        Holo30,
    }
    public enum HoloResolution
    {
        Holo_896x504,
        Holo_1280x720,
    }
    public HoloType holoType;
    public HoloResolution holoResolution; 
    public HoloCamFrame holoFrame;
    HoloCapture.Resolution resolution;
    public bool EnableHolograms = true;

    [Range(0,1)]
    public float Opacity=0.9f;


    public Texture2D _videoTexture { get; set; }


    void Start()
    {
        Init();
    }
    public void Init()
    {
        switch (holoResolution)
        {
            case HoloResolution.Holo_896x504:
                resolution = new HoloCapture.Resolution(896, 504);
                break;
            case HoloResolution.Holo_1280x720:
                resolution = new HoloCapture.Resolution(1280,720);
                break;
        }
        int frame;
        switch (holoFrame)
        {
            case HoloCamFrame.Holo15:
                frame = 15;
                break;
            case HoloCamFrame.Holo30:
                frame = 30;
                break;
            default:
                frame = 15;
                break;
        }
        HoloCaptureHelper.Instance.Init(resolution, frame, true, EnableHolograms, Opacity, false,
UnityEngine.XR.WSA.WorldManager.GetNativeISpatialCoordinateSystemPtr(), OnFrameSampleCallback);

        _videoTexture = new Texture2D(resolution.width, resolution.height, TextureFormat.BGRA32, false);
    }

    private void OnDestroy()
    {
        HoloCaptureHelper.Instance.Destroy();
    }

    bool isStartCaputure;
    public void StartCapture()
    {
        if (isStartCaputure) return;
        isStartCaputure = true;
        HoloCaptureHelper.Instance.StartCapture();
    }
    public void StopCapture()
    {
        if (!isStartCaputure) return;
        isStartCaputure = false;
        HoloCaptureHelper.Instance.StopCapture();
    }

    void OnFrameSampleCallback(VideoCaptureSample sample)
    {

        byte[] imageBytes = new byte[sample.dataLength];

        sample.CopyRawImageDataIntoBuffer(imageBytes);

        sample.Dispose();

        UnityEngine.WSA.Application.InvokeOnAppThread(() =>
        {
            if (holoType==HoloType.Holo1)
            {
                ImageHorizontalMirror(imageBytes);
            }
            else if (holoType == HoloType.Holo2)
            {
                ImageVerticalMirror(imageBytes);
            }
            _videoTexture.LoadRawTextureData(imageBytes);
            _videoTexture.wrapMode = TextureWrapMode.Clamp;
            _videoTexture.Apply();
            UnityChatSDK.Instance.UpdateCustomTexture(_videoTexture);

        }, false);


    }
    void ImageHorizontalMirror(byte[] imageBytes)
    {
        int PixelSize = 4;
        int width = resolution.width;
        int height = resolution.height;
        int Line = width * PixelSize;

        for (int i = 0; i < height; ++i)
        {
            for (int j = 0; j + 4 < Line / 2; j += 4)
            {
                Swap<byte>(ref imageBytes[Line * i + j], ref imageBytes[Line * i + Line - j - 4]);
                Swap<byte>(ref imageBytes[Line * i + j + 1], ref imageBytes[Line * i + Line - j - 3]);
                Swap<byte>(ref imageBytes[Line * i + j + 2], ref imageBytes[Line * i + Line - j - 2]);
                Swap<byte>(ref imageBytes[Line * i + j + 3], ref imageBytes[Line * i + Line - j - 1]);
            }
        }
    }
    void ImageVerticalMirror(byte[] imageBytes)
    {
        int PixelSize = 4;
        int width = resolution.width;
        int height = resolution.height;
        int Line = width * PixelSize;

        for (int i = 0; i< width; i ++)
        {
            for (int j = 0; j < height / 2; j++)
            {
                Swap<byte>(ref imageBytes[Line * j + i * PixelSize], ref imageBytes[Line * (height-j-1)+ i * PixelSize]);
                Swap<byte>(ref imageBytes[Line * j + i * PixelSize + 1], ref imageBytes[Line * (height - j - 1) + i * PixelSize + 1]);
                Swap<byte>(ref imageBytes[Line * j + i * PixelSize + 2], ref imageBytes[Line * (height - j - 1) + i * PixelSize + 2]);
                Swap<byte>(ref imageBytes[Line * j + i * PixelSize + 3], ref imageBytes[Line * (height - j - 1) + i * PixelSize + 3]);
            }
        }
    }
    void Swap<T>(ref T lhs, ref T rhs)
    {
        T temp;
        temp = lhs;
        lhs = rhs;
        rhs = temp;
    }
#else
    public void StartCapture()
    {
    }
    public void StopCapture()
    {
    }
#endif
}
