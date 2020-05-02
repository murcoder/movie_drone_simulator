using UnityEngine;
using System.IO;
using System.Runtime.InteropServices;

namespace VRCapture {

    [RequireComponent(typeof(AudioListener))]
    public class VRCaptureAudio : MonoBehaviour {

        [DllImport("VRCaptureLib")]
        static extern System.IntPtr LibAudioCaptureAPI_Get(int rate, string path, string ffpath);

        [DllImport("VRCaptureLib")]
        static extern void LibAudioCaptureAPI_SendFrame(System.IntPtr api, byte[] data);

        [DllImport("VRCaptureLib")]
        static extern void LibAudioCaptureAPI_Close(System.IntPtr api);

        /// <summary>
        /// Whether this audio listiner is enabled for capture.
        /// </summary>
        public bool isEnabled = true;

        /// <summary>
        /// Whether or not capturing from this audio listener is currently in progress.
        /// </summary>
        bool isCapturing;
        /// <summary>
        /// Reference to native lib API.
        /// </summary>
        System.IntPtr libAPI;
        /// <summary>
        /// The audio capture prepare.
        /// </summary>
        System.IntPtr audioPointer;
        System.Byte[] audioByteBuffer;

        public string FilePath {
            get; private set;
        }

        /// <summary>
        /// To be notified when the audio is complete, register a delegate 
        /// using this signature by calling RegisterSessionCompleteDelegate.
        /// </summary>
        public delegate void AudioCaptureCompleteDelegate();

        /// <summary>
        /// The audio capturing complete delegate variable.
        /// </summary>
        AudioCaptureCompleteDelegate audioCaptureCompleteDelegate;

        /// <summary>
        /// Register a delegate to be invoked when the audio is complete.
        /// </summary>
        /// <param name='del'>
        /// The delegate to be invoked when complete.
        /// </param>
        public void RegisterCaptureCompleteDelegate(AudioCaptureCompleteDelegate del) {
            audioCaptureCompleteDelegate += del;
        }

        public bool IsProcessing() {
            return isCapturing;
        }

        public void Cleanup() {
            audioCaptureCompleteDelegate = null;
            if (File.Exists(FilePath)) {
                File.Delete(FilePath);
            }
        }

        public void StartCapture() {
            if (!isEnabled) {
                return;
            }
            if (IsProcessing()) {
                Debug.LogWarning("VRCaptureAudio: capture still processing!");
                return;
            }
            string audioPath = System.DateTime.Now.ToString("yyyy-MMM-d-HH-mm-ss") + ".wav";
            FilePath = VRCapture.Instance.FolderPath + "/" + audioPath;
            libAPI = LibAudioCaptureAPI_Get(
                AudioSettings.outputSampleRate,
                FilePath,
                VRCapture.Instance.FFmpegPath);
            if (libAPI == System.IntPtr.Zero) {
                Debug.LogWarning("VRCaptureAudio: get native LibAudioCaptureAPI failed!");
                return;
            }
            InitCapture();
            isCapturing = true;
        }

        public void FinishCapture() {
            if (!isEnabled) {
                return;
            }
            if (!isCapturing) {
                Debug.LogWarning("VRCaptureVideo: capture not start yet!");
            }
            isCapturing = false;
            LibAudioCaptureAPI_Close(libAPI);

            // Notif caller audio capture complete.
            if (audioCaptureCompleteDelegate != null) {
                audioCaptureCompleteDelegate();
            }

            if (VRCapture.Instance.debug) {
                Debug.Log("VRCaptureAudio: Encod process finish!");
            }
        }

        void InitCapture() {
            audioByteBuffer = new System.Byte[8192];
            GCHandle audioHandle = GCHandle.Alloc(audioByteBuffer, GCHandleType.Pinned);
            audioPointer = audioHandle.AddrOfPinnedObject();
        }

        void OnAudioFilterRead(float[] data, int channels) {
            if (isCapturing) {
                Marshal.Copy(data, 0, audioPointer, 2048);
                LibAudioCaptureAPI_SendFrame(libAPI, audioByteBuffer);
            }
        }
    }
}
