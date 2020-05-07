using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;

namespace VRCapture {

    [RequireComponent(typeof(Camera))]
    public class VRCaptureVideo : MonoBehaviour {

        [DllImport("VRCaptureLib")]
        static extern System.IntPtr LibVideoCaptureAPI_Get(int width, int height, int rate, string path, string ffpath);

        [DllImport("VRCaptureLib")]
        static extern void LibVideoCaptureAPI_SendFrame(System.IntPtr api, byte[] data);

        [DllImport("VRCaptureLib")]
        static extern void LibVideoCaptureAPI_Close(System.IntPtr api);

        public enum CaptureType {
            NORMAL,
            EQUIRECTANGULAR,    //Kugelpanorama
        }

        public enum FrameSizeType {
            /// <summary>
            /// 480p (640 x 480) Standard Definition (SD).
            /// </summary>
            _640x480,
            /// <summary>
            /// 480p (720 x 480) Standard Definition (SD) (resolution of DVD video).
            /// </summary>
            _720x480,
            /// <summary>
            /// 720p (1280 x 720) High Definition (HD).
            /// </summary>
            _1280x720,
            /// <summary>
            /// 1080p (1920 x 1080) Full High Definition (FHD).
            /// </summary>
            _1920x1080,
            /// <summary>
            /// 2K (2048 x 1080).
            /// </summary>
            _2048x1080,
            /// <summary>
            /// 4K (3840 x 2160) Quad Full High Definition (QFHD)
            /// (also known as UHDTV/UHD-1, resolution of Ultra High Definition TV).
            /// </summary>
            _3840x2160, 
            /// <summary>
            /// 4K (4096 x 2160) Ultra High Definition (UHD).
            /// </summary>
            _4096x2160,
        }

        public enum CubemapSizeType {
            _512, _1024, _2048, _4096,
        }
        
        public enum EquirecatngularFrameSizeType {
            _1280x640,
            _1920x960,
            _2048x1024,
            _4096x2048,
        }

        public enum EncodeQualityType {
            /// <summary>
            /// Lower quality will decrease filesize on disk.
            /// Low = 1000 bitrate.
            /// </summary>
            Low,
            /// <summary>
            /// Medium = 2500 bitrate.
            /// </summary>
            Medium,
            /// <summary>
            /// High = 5000 bitrate.
            /// </summary>
            High,
        }

        public enum AntiAliasingType {
            _1, _2, _4, _8,
        }

        public enum TargetFramerateType {
            _24, _30, _45, _60,
        }

        [Tooltip("Decide record flat or equirectangular")]
        public CaptureType captureType = CaptureType.NORMAL;
        [Tooltip("Resolution of recorded video")]
        public FrameSizeType frameSize = FrameSizeType._1280x720;
        [Tooltip("The cubemap size capture render to")]
        public CubemapSizeType cubemapSize = CubemapSizeType._1024;
        [Tooltip("The equirecatngular output video size")]
        public EquirecatngularFrameSizeType equirecatngularFrameSize = 
            EquirecatngularFrameSizeType._2048x1024;
        [Tooltip("Lower quality will decrease filesize on disk")]
        public EncodeQualityType encodeQuality = EncodeQualityType.Medium;
        [Tooltip("Anti aliasing setting for recorded video")]
        public AntiAliasingType antiAliasing = AntiAliasingType._1;
        [Tooltip("Target frameRate for recorded video")]
        public TargetFramerateType targetFramerate = TargetFramerateType._30;

        public int FrameWidth {
            get {
                int width = 1280;
                if (captureType == CaptureType.NORMAL) {
                    if (frameSize == FrameSizeType._640x480) {
                        width = 640;
                    }
                    else if (frameSize == FrameSizeType._720x480) {
                        width = 720;
                    }
                    else if (frameSize == FrameSizeType._1280x720) {
                        width = 1280;
                    }
                    else if (frameSize == FrameSizeType._1920x1080) {
                        width = 1920;
                    }
                    else if (frameSize == FrameSizeType._2048x1080) {
                        width = 2048;
                    }
                    else if (frameSize == FrameSizeType._3840x2160) {
                        width = 3840;
                    }
                    else if (frameSize == FrameSizeType._4096x2160) {
                        width = 4096;
                    }
                }
                else if (captureType == CaptureType.EQUIRECTANGULAR) {
                    if (equirecatngularFrameSize == EquirecatngularFrameSizeType._1280x640) {
                        width = 1280;
                    }
                    else if (equirecatngularFrameSize == EquirecatngularFrameSizeType._1920x960) {
                        width = 1920;
                    }
                    else if (equirecatngularFrameSize == EquirecatngularFrameSizeType._2048x1024) {
                        width = 2048;
                    }
                    else if (equirecatngularFrameSize == EquirecatngularFrameSizeType._4096x2048) {
                        width = 4096;
                    }
                }
                if (!isDedicated) {
                    width = videoCamera.pixelWidth;
                }
                return width;
            }
        }

        public int FrameHeight {
            get {
                int height = 720;
                if (captureType == CaptureType.NORMAL) {
                    if (frameSize == FrameSizeType._640x480 || frameSize == FrameSizeType._720x480) {
                        height = 480;
                    }
                    else if (frameSize == FrameSizeType._1280x720) {
                        height = 720;
                    }
                    else if (frameSize == FrameSizeType._1920x1080 || frameSize == FrameSizeType._2048x1080) {
                        height = 1080;
                    }
                    else if (frameSize == FrameSizeType._3840x2160 || frameSize == FrameSizeType._4096x2160) {
                        height = 2160;
                    }
                }
                else if (captureType == CaptureType.EQUIRECTANGULAR) {
                    if (equirecatngularFrameSize == EquirecatngularFrameSizeType._1280x640) {
                        height = 640;
                    }
                    else if (equirecatngularFrameSize == EquirecatngularFrameSizeType._1920x960) {
                        height = 960;
                    }
                    else if (equirecatngularFrameSize == EquirecatngularFrameSizeType._2048x1024) {
                        height = 1024;
                    }
                    else if (equirecatngularFrameSize == EquirecatngularFrameSizeType._4096x2048) {
                        height = 2048;
                    }
                }
                if (!isDedicated) {
                    height = videoCamera.pixelHeight;
                }
                return height;
            }
        }

        public int CubemapSize {
            get {
                int size = 1024;
                if (cubemapSize == CubemapSizeType._512) {
                    size = 512;
                }
                else if (cubemapSize == CubemapSizeType._1024) {
                    size = 1024;
                }
                else if (cubemapSize == CubemapSizeType._2048) {
                    size = 2048;
                }
                else if (cubemapSize == CubemapSizeType._4096) {
                    size = 4096;
                }
                return size;
            }
        }

        public int AntiAliasing {
            get {
                int anti = 1;
                if (antiAliasing == AntiAliasingType._1) {
                    anti = 1;
                }
                else if (antiAliasing == AntiAliasingType._2) {
                    anti = 2;
                }
                else if (antiAliasing == AntiAliasingType._4) {
                    anti = 4;
                }
                else if (antiAliasing == AntiAliasingType._8) {
                    anti = 8;
                }
                return anti;
            }
        }

        public int Bitrate {
            get {
                int bitrate = 1000;
                if (encodeQuality == EncodeQualityType.Low) {
                    bitrate = 1000;
                }
                else if (encodeQuality == EncodeQualityType.Medium) {
                    bitrate = 2500;
                }
                else if (encodeQuality == EncodeQualityType.High) {
                    bitrate = 5000;
                }
                return bitrate;
            }
        }

        public int TargetFramerate {
            get {
                int framerate = 30;
                if (targetFramerate == TargetFramerateType._24) {
                    framerate = 24;
                }
                else if (targetFramerate == TargetFramerateType._30) {
                    framerate = 30;
                }
                else if (targetFramerate == TargetFramerateType._45) {
                    framerate = 45;
                }
                else if (targetFramerate == TargetFramerateType._60) {
                    framerate = 60;
                }
                return framerate;
            }
        }

        /// <summary>
        /// To be notified when the video is complete, register a delegate 
        /// using this signature by calling VideoCaptureCompleteDelegate.
        /// </summary>
        public delegate void VideoCaptureCompleteDelegate();

        /// <summary>
        /// The video capturing complete delegate variable.
        /// </summary>
        VideoCaptureCompleteDelegate videoCaptureCompleteDelegate;

        /// <summary>
        /// Register a delegate to be invoked when the video is complete.
        /// </summary>
        /// <param name='del'>
        /// The delegate to be invoked when complete.
        /// </param>
        public void RegisterCaptureCompleteDelegate(VideoCaptureCompleteDelegate del) {
            videoCaptureCompleteDelegate += del;
        }

        /// <summary>
        /// Specifies whether or not the camera being used to capture video is 
        /// dedicated solely to video capture. When a dedicated camera is used,
        /// the camera's aspect ratio will automatically be set to the specified
        /// frame size.
        /// If a non-dedicated camera is specified it is assumed the camera will 
        /// also be used to render to the screen, and so the camera's aspect 
        /// ratio will not be adjusted.
        /// Use a dedicated camera to capture video at resolutions that have a 
        /// different aspect ratio than the device screen.
        /// </summary>
        public bool isDedicated = true;
        /// <summary>
        /// Setup Time.maximumDeltaTime to avoiding nasty stuttering.
        /// https://docs.unity3d.com/ScriptReference/Time-maximumDeltaTime.html
        /// </summary>
        public bool offlineRender = false;
        float prevMaximumDeltaTime = 0.3333333f ;
        /// <summary>
        /// Whether this camera is enabled for capture.
        /// </summary>
        public bool isEnabled = true;
        /// <summary>
        /// The index of the camera.
        /// </summary>
        public int Index {
            get; set;
        }
        /// <summary>
        /// Whether the video capture process failed.
        /// </summary>
        /// <value><c>true</c> if failed; otherwise, <c>false</c>.</value>
        public bool Failed {
            get; private set;
        }
        /// <summary>
        /// The camera that resides on the same game object as this script.
        /// It will be used for capturing video.
        /// </summary>
        Camera videoCamera;
        /// <summary>
        /// The texture holding the video frame data.
        /// </summary>
        Texture2D texture2d;
        RenderTexture renderTexture;
        /// <summary>
        /// For generate equirectangular video.
        /// </summary>
        Cubemap cubemap;
        Shader transformShader;
        Material transformMaterial;
        /// <summary>
        /// Whether or not capturing from this camera is currently in progress.
        /// </summary>
        bool isCapturing;
        /// <summary>
        /// Whether or not there is a frame capturing now.
        /// </summary>
        bool isCapturingFrame;
        /// <summary>
        /// The time spent during capturing.
        /// </summary>
        float capturingTime;
        /// <summary>
        /// The delta time of each frame.
        /// </summary>
        float deltaFrameTime;
        /// <summary>
        /// Frame statistics.
        /// </summary>
        int capturedFrameCount;
        int encodedFrameCount;
        /// <summary>
        /// Reference to native lib API.
        /// </summary>
        System.IntPtr libAPI;

        /// <summary>
        /// Thread shared resources.
        /// </summary>
        Queue<byte[]> frameQueue;
        Object threadLock;


        public bool IsProcessing() {
            return isCapturing || (frameQueue != null && frameQueue.Count > 0);
        }

        public string FilePath {
            get; private set;
        }

        public void Cleanup() {
            if (!isEnabled) {
                return;
            }
            texture2d = null;
            renderTexture = null;
            frameQueue = null;
            threadLock = null;
            videoCaptureCompleteDelegate = null;
            capturedFrameCount = 0;
            encodedFrameCount = 0;
            if (File.Exists(FilePath)) {
                File.Delete(FilePath);
            }
        }

        public void StartCapture() {
            if (!isEnabled) {
                return;
            }
            if (IsProcessing()) {
                Debug.LogWarning("VRCaptureVideo: capture still processing!");
                return;
            }
            if (captureType == CaptureType.EQUIRECTANGULAR && !isDedicated) {
                Debug.LogWarning("VRCaptureVideo: capture equirectangular video " + 
                                 "require dedicated camera!");
                return;
            }
            InitCapture();

            libAPI = LibVideoCaptureAPI_Get(
                FrameWidth,
                FrameHeight,
                TargetFramerate,
                FilePath,
                /*VRCapture.Instance.FFmpegPath*/ CaptureConfig.FFmpegPath);
            if (libAPI == System.IntPtr.Zero) {
                Debug.LogWarning("VRCaptureVideo: get native LibVideoCaptureAPI failed!");
                return;
            }
            if (offlineRender) {
                prevMaximumDeltaTime = Time.maximumDeltaTime;
                Time.maximumDeltaTime = Time.fixedDeltaTime;
            }

            isCapturing = true;
            // Start encoding thread.
            Thread encodingThread = new Thread(EncodingThreadFunction);
            encodingThread.Priority = System.Threading.ThreadPriority.Lowest;
            encodingThread.IsBackground = true;
            encodingThread.Start();
        }

        public void FinishCapture() {
            if (!isEnabled) {
                return;
            }
            if (!isCapturing) {
                Debug.LogWarning("VRCaptureVideo: capture not start yet!");
            }
            if (offlineRender) {
                Time.maximumDeltaTime = prevMaximumDeltaTime;
            }
            isCapturing = false;
        }

        /// <summary>
        ///Create the video- and filepath and a renderTexture if it is not existing. 
        ///Fix the Aspect Ratio. 
        ///allocate frameQueue with an array of bytes. 
        ///</summary>
        void InitCapture() {
            if (videoCamera.targetTexture != null) {
                // Use binded rendertexture will ignore antiAliasing config.
                renderTexture = videoCamera.targetTexture;
            }
            else {
                // Create a rendertexture for video capture.    
                // Size it according to the desired video frame size.
                renderTexture = new RenderTexture(FrameWidth, FrameHeight, 24);
                renderTexture.antiAliasing = AntiAliasing;
                // Make sure the rendertexture is created.
                renderTexture.Create();
            }
            if (isDedicated) {
                // Set the aspect ratio of the camera to match the rendertexture.
                videoCamera.aspect = FrameWidth / ((float)FrameHeight);
                videoCamera.targetTexture = renderTexture;
            }
            if (captureType == CaptureType.EQUIRECTANGULAR) {
                // Create render cubemap.
                cubemap = new Cubemap(CubemapSize, TextureFormat.RGB24, false);
                // Setup transform shader.
                transformShader = Shader.Find("VRCapture/Cubemap2Equirectangular");
                transformMaterial = new Material(transformShader);
            }
            texture2d = new Texture2D(FrameWidth, FrameHeight, TextureFormat.RGB24, false);
            string videoPath =
                System.DateTime.Now.ToString("yyyy-MMM-d-HH-mm-ss") + "-" + Index + ".mp4";
            //FilePath = VRCapture.Instance.FolderPath + "/" + videoPath;
            FilePath = CaptureConfig.SaveFolder + "/" + videoPath;
            deltaFrameTime = 1f / TargetFramerate;
            capturingTime = 0f;
            frameQueue = new Queue<byte[]>();
            threadLock = new Object();
        }

        void Awake() {
            videoCamera = GetComponent<Camera>();
        }

#if UNITY_EDITOR
        void OnGUI() {
            if (isCapturing) {
                capturingTime += Time.deltaTime;
            }

            if (!isCapturingFrame && isCapturing) {
                int totalRequiredFrameCount =
                    (int)(capturingTime / deltaFrameTime);
                // Skip frames if we already got enough.
                if (totalRequiredFrameCount > capturedFrameCount) {
                    // Dedicated camera require a render call.
                    if (isDedicated) {
                        if (captureType == CaptureType.EQUIRECTANGULAR) {
                            videoCamera.RenderToCubemap(cubemap);
                        }
                        else {
                            videoCamera.Render();
                        }
                    }
                    StartCoroutine(CaptureFrame());
                }
            }
        }
#endif

        IEnumerator CaptureFrame() {
            // Wait few frames for rendering finish.
            if (isDedicated)
                yield return null;
            else
                yield return new WaitForEndOfFrame();
            isCapturingFrame = true;
            if (isCapturing) {
                RenderTexture prevRenderTexture = null;
                if (isDedicated) {
                    prevRenderTexture = RenderTexture.active;
                    RenderTexture.active = renderTexture;
                }
                if (captureType == CaptureType.EQUIRECTANGULAR) {
                    Graphics.Blit(cubemap, renderTexture, transformMaterial);
                }
                // TODO, remove the step of copying pixel data from GPU to CPU.
                texture2d.ReadPixels(new Rect(0, 0, FrameWidth, FrameHeight), 0, 0, false);
                texture2d.Apply();
                if (isDedicated) {
                    RenderTexture.active = prevRenderTexture;
                }
            }

            yield return null;
            // User may terminate the capture process during capturing frame.
            if (isCapturing) {
                byte[] pixels = texture2d.GetRawTextureData();
                int totalRequiredFrameCount = (int)(capturingTime / deltaFrameTime);
                int requiredFrameCount = totalRequiredFrameCount - capturedFrameCount;
                lock (threadLock) {
                    while (requiredFrameCount-- > 0) {
                        frameQueue.Enqueue(pixels);         //Fuegt am ende der queue ein objekt hinzu
                    }
                }
                capturedFrameCount = totalRequiredFrameCount;
            }
            isCapturingFrame = false;
        }

        void EncodingThreadFunction() {
            while (isCapturing || frameQueue.Count > 0) {
                if (frameQueue.Count > 0) {
                    lock (threadLock) {
                        byte[] frame = frameQueue.Dequeue();
                        LibVideoCaptureAPI_SendFrame(libAPI, frame);
                    }
                    encodedFrameCount++;
                    if (VRCapture.Instance.debug) {
                        Debug.Log("VRCaptureVideo: Encoded " +
                                  encodedFrameCount + " frames. " +
                                  frameQueue.Count + " frames remaining.");
                    }
                }
                else {
                    Thread.Sleep(1);
                }
            }
            if (VRCapture.Instance.debug) {
                Debug.Log("VRCaptureVideo: Encode process finish!");
            }
            // Notify native encoding process finish.
            LibVideoCaptureAPI_Close(libAPI);
            // Notify caller video capture complete.
            if (videoCaptureCompleteDelegate != null) {
                videoCaptureCompleteDelegate();
            }
        }
    }
}
