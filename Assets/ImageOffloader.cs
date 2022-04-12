using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageOffloader : MonoBehaviour
{
    public Text TestLabel;

    [SerializeField]
    [Tooltip("The ARCameraManager which will produce frame events.")]
    ARCameraManager m_CameraManager;

    /// <summary>
    /// Get or set the <c>ARCameraManager</c>.
    /// </summary>
    public ARCameraManager cameraManager
    {
        get => m_CameraManager;
        set => m_CameraManager = value;
    }

    [SerializeField]
    RawImage m_RawCameraImage;

    /// <summary>
    /// The UI RawImage used to display the image on screen.
    /// </summary>
    public RawImage rawCameraImage
    {
        get => m_RawCameraImage;
        set => m_RawCameraImage = value;
    }

    [SerializeField]
    Text m_ImageInfo;

    /// <summary>
    /// The UI Text used to display information about the image on screen.
    /// </summary>
    public Text imageInfo
    {
        get => m_ImageInfo;
        set => m_ImageInfo = value;
    }

    [SerializeField]
    Button m_TransformationButton;

    /// <summary>
    /// The button that controls transformation selection.
    /// </summary>
    public Button transformationButton
    {
        get => m_TransformationButton;
        set => m_TransformationButton = value;
    }

    XRCpuImage.Transformation m_Transformation = XRCpuImage.Transformation.MirrorY;

    /// <summary>
    /// Cycles the image transformation to the next case.
    /// </summary>
    public void CycleTransformation()
    {
        m_Transformation = m_Transformation switch
        {
            XRCpuImage.Transformation.None => XRCpuImage.Transformation.MirrorX,
            XRCpuImage.Transformation.MirrorX => XRCpuImage.Transformation.MirrorY,
            XRCpuImage.Transformation.MirrorY => XRCpuImage.Transformation.MirrorX | XRCpuImage.Transformation.MirrorY,
            _ => XRCpuImage.Transformation.None
        };

        if (m_TransformationButton)
        {
            m_TransformationButton.GetComponentInChildren<Text>().text = m_Transformation.ToString();
        }
    }

    void OnEnable()
    {
        if (m_CameraManager != null)
        {
            m_CameraManager.frameReceived += OnCameraFrameReceived;
        }
    }

    void OnDisable()
    {
        if (m_CameraManager != null)
        {
            m_CameraManager.frameReceived -= OnCameraFrameReceived;
        }
    }

    unsafe void UpdateCameraImage()
    {
        // Attempt to get the latest camera image. If this method succeeds,
        // it acquires a native resource that must be disposed (see below).
        if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            return;
        }

        // Display some information about the camera image
        m_ImageInfo.text = string.Format(
            "Image info:\n\twidth: {0}\n\theight: {1}\n\tplaneCount: {2}\n\ttimestamp: {3}\n\tformat: {4}",
            image.width, image.height, image.planeCount, image.timestamp, image.format);

        // Once we have a valid XRCpuImage, we can access the individual image "planes"
        // (the separate channels in the image). XRCpuImage.GetPlane provides
        // low-overhead access to this data. This could then be passed to a
        // computer vision algorithm. Here, we will convert the camera image
        // to an RGBA texture and draw it on the screen.

        // Choose an RGBA format.
        // See XRCpuImage.FormatSupported for a complete list of supported formats.
        var format = TextureFormat.RGBA32;

        if (m_CameraTexture == null || m_CameraTexture.width != image.width || m_CameraTexture.height != image.height)
        {
            m_CameraTexture = new Texture2D(image.width, image.height, format, false);
        }

        // Convert the image to format, flipping the image across the Y axis.
        // We can also get a sub rectangle, but we'll get the full image here.
        var conversionParams = new XRCpuImage.ConversionParams(image, format, m_Transformation);

        // Texture2D allows us write directly to the raw texture data
        // This allows us to do the conversion in-place without making any copies.
        var rawTextureData = m_CameraTexture.GetRawTextureData<byte>();
        try
        {
            image.Convert(conversionParams, new IntPtr(rawTextureData.GetUnsafePtr()), rawTextureData.Length);
        }
        finally
        {
            // We must dispose of the XRCpuImage after we're finished
            // with it to avoid leaking native resources.
            image.Dispose();
        }

        // Apply the updated texture data to our texture
        //m_CameraTexture.Apply();

        // Set the RawImage's texture so we can visualize it.
        //m_RawCameraImage.texture = m_CameraTexture;

        //TestLabel.text = "updated";
    }

    void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        UpdateCameraImage();
    }

    Texture2D m_CameraTexture;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}
