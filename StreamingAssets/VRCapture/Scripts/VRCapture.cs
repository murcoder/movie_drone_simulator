using UnityEngine;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System;

namespace VRCapture {
    /// <summary>
    /// VRCapture is a plugin helping VR player to record and share
    /// their gameplay easily and nicely.
    /// </summary>
    public class VRCapture : MonoBehaviour {
        /// <summary>
        /// VRCapture instance reference.
        /// </summary>
        /// <value>The instance.</value>
        public static VRCapture Instance {
            get; private set;
        }

        /// <summary>
        /// Indicates the current status of the capturing session.
        /// </summary>
        public enum SessionStatusCode {
            /// <summary>
            /// The capturing session has encountered no errors.
            /// </summary>
            Success = 1,

            /// <summary>
            /// No camera or audio was found to perform video or audio recording.
            /// You must specify one or the other before calling BeginCaptureSession().
            /// </summary>
            CaptureNotFound = -1,

            /// <summary>
            /// The ffmpeg executable file not found, this plugin current is depend
            /// on this to generate the videos.
            /// </summary>
            FFmpegNotFound = -2,

            /// <summary>
            /// The capture process is interrupted by user or unexcept quit.
            /// </summary>
            Interrupted = -3,

            /// <summary>
            /// The process of merge video and audio failed.
            /// </summary>
            MergeProcessFailed = -4,
        }

        /// <summary>
        /// To be notified when an error occurs during a capture session, register
        /// a delegate using this signature by calling RegisterSessionErrorDelegate.
        /// </summary>
        public delegate void SessionErrorDelegate(SessionStatusCode code);

        /// <summary>
        /// To be notified when the capture is complete, register a delegate 
        /// using this signature by calling RegisterSessionCompleteDelegate.
        /// </summary>
        public delegate void SessionCompleteDelegate();

        /// <summary>
        /// Register a delegate to be invoked when an error occurs during a
        /// capture session. Multiple delegates may be registered and 
        /// they will be invoked in the order of registration.
        /// </summary>
        /// <param name='del'>
        /// The delegate to be invoked when an error occurs.
        /// </param>
        public void RegisterSessionErrorDelegate(SessionErrorDelegate del) {
            sessionErrorDelegate += del;
        }

        /// <summary>
        /// Unregister a previously registered session error delegate.
        /// </summary>
        /// <param name='del'>
        /// The delegate to be unregistered.
        /// </param>
        public void UnregisterSessionErrorDelegate(SessionErrorDelegate del) {
            sessionErrorDelegate -= del;
        }

        /// <summary>
        /// Register a delegate to be invoked when the capture is complete.
        /// </summary>
        /// <param name='del'>
        /// The delegate to be invoked when capture complete.
        /// </param>
        public void RegisterSessionCompleteDelegate(SessionCompleteDelegate del) {
            sessionCompleteDelegate += del;
        }

        /// <summary>
        /// Unregister a previously registered session complete delegate.
        /// </summary>
        /// <param name='del'>
        /// The delegate to be unregistered.
        /// </param>
        public void UnregisterSessionCompleteDelegate(SessionCompleteDelegate del) {
            sessionCompleteDelegate -= del;
        }

        // The video record session error delegate variable.
        SessionErrorDelegate sessionErrorDelegate;

        // The video record session complete delegate variable.
        SessionCompleteDelegate sessionCompleteDelegate;

        /// <summary>
        /// Reference to the VRCaptureVideo capture objects (i.e. cameras) from
        /// which video will be recorded.
        /// Generally you will want to specify at least one.
        /// </summary>
        [Tooltip("Capture cameras for video recording")]
        public VRCaptureVideo[] vrCaptureVideos;

        /// <summary>
        /// Reference to the VRCaptureAudio object for writing audio files.
        /// This needs to be set when you are recording a video with audio.
        /// </summary>
        [Tooltip("Audiolistener for audio recording")]
        public VRCaptureAudio vrCaptureAudio;

        /// <summary>
        /// Show debug message.
        /// </summary>
        [Tooltip("Show debug info message")]
        public bool debug = false;

        /// <summary>
        /// Capturing session status.
        /// </summary>
        SessionStatusCode sessionStatus;

        /// <summary>
        /// The ffmpeg executable file.
        /// </summary>
        string dataPath;
        public string FFmpegPath {
            get {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                //return dataPath + "/" + VRCaptureConfig.FFMPEG_WIN_PATH;
                return CaptureConfig.FFmpegPath;
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
                return dataPath + "/" + VRCaptureConfig.FFMPEG_MAC_PATH;
#endif
            }
        }
        /// <summary>
        /// Folder to place capture video and audio.
        /// </summary>
        /// <value>The folder path.</value>
        public string FolderPath {
            get {
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                                  "/" + VRCaptureConfig.CAPTURE_FOLDER;
            }
        }

        /// <summary>
        /// Check how many video capture is complete currently.
        /// </summary>
        int captureFinishCount;
        /// <summary>
        /// The count of camera is enabled for capturing.
        /// </summary>
        int captureRequiredCount;
        /// <summary>
        /// Check wether the video and audio is still in merging process.
        /// </summary>
        bool isMerging;
        /// <summary>
        /// Check wether the capture process is interrupted.
        /// </summary>
        bool isInterrupted;

        /// <summary>
        /// Get the VRCaptureVideo in vrCaptureVideos list.
        /// </summary>
        /// <returns>VRCaptureVideo.</returns>
        /// <param name="index">Camera index.</param>
        public VRCaptureVideo GetCaptureVideo(int index) {
            if (index < 0 || index >= vrCaptureVideos.Length) {
                Debug.LogWarning("VRCapture: GetCaptureVideoByIndex called with " +
                                 "invalid camera index!");
                return null;
            }
            return vrCaptureVideos[index];
        }

        /// <summary>
        /// Get the VRCaptureAudio if attached.
        /// </summary>
        /// <returns>The capture audio.</returns>
        public VRCaptureAudio GetCaptureAudio() {
            return vrCaptureAudio;
        }

        /// <summary>
        /// Check any of video capture or merge is still processing.
        /// </summary>
        /// <returns><c>true</c>, if still processing, <c>false</c> otherwise.</returns>
        public bool IsProcessing() {
            bool isPorcessing = false;
            foreach (VRCaptureVideo captureVideo in vrCaptureVideos) {
                if (captureVideo.IsProcessing()) {
                    isPorcessing = true;
                    break;
                }
            }
            return isPorcessing || isMerging;
        }

        /// <summary>
        /// Initialize the attributes of the capture session and and start capturing. 
        /// </summary>
        /// <returns>
        /// The status of the capturing session. This may be Success or a failure code.
        /// See SessionStatusCode for more information.
        /// </returns>
        public SessionStatusCode BeginCaptureSession() {
            if (vrCaptureVideos.Length < 1 && vrCaptureAudio == null) {
                Debug.LogError("VRCapture: BeginCaptureSession called but no attached " +
                               "cameras and audio listener were found!");
                sessionStatus = SessionStatusCode.CaptureNotFound;
                return sessionStatus;
            }

            if (IsProcessing()) {
                Debug.LogWarning("VRCapture: BeginCaptureSession called before, and " +
                                 "capture still processing!");
                return sessionStatus;
            }

            //UPDATE------------------------------------------------------
            if (!File.Exists(CaptureConfig.FFmpegPath))
            {
                Debug.LogError("VRCapture: FFmpeg not found, please fix this " +
                               "before capture!");
                sessionStatus = SessionStatusCode.FFmpegNotFound;
                return sessionStatus;
            }

            if (!Directory.Exists(CaptureConfig.SaveFolder))
            {
                Directory.CreateDirectory(CaptureConfig.SaveFolder);
            }


            //if (!File.Exists(FFmpegPath)) {
            //    Debug.LogError("VRCapture: FFmpeg not found, please fix this " +
            //                   "before capture!");
            //    sessionStatus = SessionStatusCode.FFmpegNotFound;
            //    return sessionStatus;
            //}
            if (!Directory.Exists(FolderPath)) {
                Directory.CreateDirectory(FolderPath);
            }

            // Reset sessionStatus.
            sessionStatus = SessionStatusCode.Success;
            isInterrupted = false;
            // Loop through each of the video capture objects, initialize them 
            // and start them recording.
            captureRequiredCount = 0;
            for (int i = 0; i < vrCaptureVideos.Length; i++) {
                VRCaptureVideo vrCaptureVideo = vrCaptureVideos[i];
                if (vrCaptureVideo == null || !vrCaptureVideo.isEnabled) {
                    continue;
                }
                captureRequiredCount++;
                vrCaptureVideo.Index = i;
                vrCaptureVideo.StartCapture();
                vrCaptureVideo.RegisterCaptureCompleteDelegate(
                    HandleVideoCaptureComplete);
            }

            // Check if we capture audio.
            if (vrCaptureAudio != null && vrCaptureAudio.isEnabled) {
                vrCaptureAudio.StartCapture();
                vrCaptureAudio.RegisterCaptureCompleteDelegate(
                    HandleAudioCaptureComplete);
            }
            captureFinishCount = 0;

            sessionStatus = SessionStatusCode.Success;
            return sessionStatus;
        }

        /// <summary>
        /// Stop capturing and produce the finalized video. Note that the video file
        /// may not be completely written when this method returns. In order to know
        /// when the video file is complete, register a SessionCompleteDelegate.
        /// </summary>
        /// <returns>The capture session.</returns>
        public SessionStatusCode EndCaptureSession() {
            // If the client calls EndRecordingSession for a failed session, do nothing.
            if (sessionStatus != SessionStatusCode.Success)
                return sessionStatus;

            foreach (VRCaptureVideo captureVideo in vrCaptureVideos) {
                if (!captureVideo.isEnabled) {
                    continue;
                }
                captureVideo.FinishCapture();
            }

            if (vrCaptureAudio != null && vrCaptureAudio.isEnabled) {
                vrCaptureAudio.FinishCapture();
            }

            sessionStatus = SessionStatusCode.Success;
            return sessionStatus;
        }

        public SessionStatusCode InterruptCaptureSession() {
            isInterrupted = true;
            EndCaptureSession();
            sessionStatus = SessionStatusCode.Interrupted;
            return sessionStatus;
        }

        void HandleVideoCaptureComplete() {
            captureFinishCount++;
            if (captureFinishCount == captureRequiredCount &&
                (vrCaptureAudio == null || !vrCaptureAudio.isEnabled)) {
                if (sessionCompleteDelegate != null) {
                    sessionCompleteDelegate();
                }
            }
        }

        void HandleAudioCaptureComplete() {
            if (isInterrupted) {
                isInterrupted = false;
                Cleanup();
                return;
            }
            bool hasVideoCapture = false;
            foreach (VRCaptureVideo captureVideo in vrCaptureVideos) {
                if (captureVideo.isEnabled) {
                    hasVideoCapture = true;
                    break;
                }
            }
            if (hasVideoCapture) {
                // Start merging thread when we have videos captured.
                Thread mergingThread = new Thread(MergingThreadFunction);
                mergingThread.Priority = System.Threading.ThreadPriority.Lowest;
                mergingThread.IsBackground = true;
                mergingThread.Start();
                isMerging = true;
            }
        }

        void MergingThreadFunction() {
            while (captureFinishCount < captureRequiredCount) {
                // Wait for all video capture finish.
                Thread.Sleep(1000);
                if (isInterrupted) {
                    return;
                }
            }

            foreach (VRCaptureVideo captureVideo in vrCaptureVideos) {
                if (captureVideo == null || !captureVideo.isEnabled) {
                    continue;
                }
                VRCaptureMerger merger = new VRCaptureMerger(captureVideo, vrCaptureAudio);
                merger.Merge();
                if (merger.Failed) {
                    sessionStatus = SessionStatusCode.MergeProcessFailed;
                    break;
                }
            }
            isMerging = false;
            Cleanup();
            if (sessionStatus != SessionStatusCode.Success) {
                if (sessionErrorDelegate != null)
                    sessionErrorDelegate(sessionStatus);
            }
            else {
                if (sessionCompleteDelegate != null) {
                    sessionCompleteDelegate();
                }
            }
        }

        void Cleanup() {
            captureFinishCount = 0;
            foreach (VRCaptureVideo captureVideo in vrCaptureVideos) {
                captureVideo.Cleanup();
            }
            if (vrCaptureAudio != null) {
                vrCaptureAudio.Cleanup();
            }
        }
        /// <summary>
        /// Initial instance and init variable.
        /// </summary>
        void Awake() {
            if (Instance == null)
                Instance = this;
            // DataPath can only be accesed throught main thread.
            dataPath = Application.dataPath;
            // For easy access the vrCaptureVideos var.
            if (vrCaptureVideos == null)
                vrCaptureVideos = new VRCaptureVideo[0];
        }
        /// <summary>
        /// Check if still processing on application quit.
        /// </summary>
        void OnApplicationQuit() {
            // Issue an interrupt if still capturing.
            if (IsProcessing()) {
                InterruptCaptureSession();
            }
        }
    }

    /// <summary>
    /// VRCaptureMerger is processed after temp video captured, with or without
    /// temp audio captured. If audio captured, it will merge the video and audio
    /// within same file.
    /// </summary>
    class VRCaptureMerger {

        [DllImport("VRCaptureLib")]
        static extern System.IntPtr LibVideoMergeAPI_Get(int rate, string path, string vpath, string apath, string ffpath);

        [DllImport("VRCaptureLib")]
        static extern void LibVideoMergeAPI_Merge(System.IntPtr api);

        VRCaptureVideo captureVideo;
        VRCaptureAudio captureAudio;

        public VRCaptureMerger(VRCaptureVideo video, VRCaptureAudio audio) {
            captureVideo = video;
            captureAudio = audio;
        }

        public bool Failed {
            get; private set;
        }

        public void Merge() {
            string videoPath = DateTime.Now.ToString("yyyy-MMM-d-HH-mm-ss") + "-" + captureVideo.Index + ".mp4";
            //string filePath = VRCapture.Instance.FolderPath + "/" + videoPath;
            string filePath = CaptureConfig.SaveFolder + "/" + videoPath;
            IntPtr libAPI = LibVideoMergeAPI_Get(
                captureVideo.Bitrate,
                filePath,
                captureVideo.FilePath,
                captureAudio.FilePath,
                /*VRCapture.Instance.FFmpegPath*/ CaptureConfig.FFmpegPath);
            if (libAPI == IntPtr.Zero) {
                Debug.LogWarning("VRCapture: get native LibVideoMergeAPI failed!");
                return;
            }
            LibVideoMergeAPI_Merge(libAPI);
            // Make sure generated the merge file.
            int waitTotal = 100;
            int waitCount = 0;
            while (!File.Exists(filePath)) {
                if (waitCount++ < waitTotal)
                    Thread.Sleep(500);
            }
            if (waitCount >= waitTotal) {
                Failed = true;
            }
        }
    }
}