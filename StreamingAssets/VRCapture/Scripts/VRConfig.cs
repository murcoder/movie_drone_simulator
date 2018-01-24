using System;
using UnityEngine;

namespace VRCapture {
    /// <summary>
    /// Basic config for VRCapture.
    /// </summary>
    public class VRCaptureConfig {

        //OLD VERISION
        public const string CAPTURE_FOLDER = "VRCapture";
        public const string REPLAY_FOLDER = "VRCapture/VRReplay";
        public const string FFMPEG_WIN_PATH = "VRCapture/FFmpeg/Win/ffmpeg.exe";
        public const string FFMPEG_MAC_PATH = "VRCapture/FFmpeg/Mac/ffmpeg";

        //NEW VERSION
        public static string MY_DOCUMENTS_PATH = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public static string DATA_PATH = Application.dataPath;
        public static string STREAMING_ASSETS_PATH = Application.streamingAssetsPath;
    }




    public class CaptureConfig
    {

        public static string SaveFolder
        {
            get
            {
                return VRCaptureConfig.MY_DOCUMENTS_PATH + "/VRCapture/";
            }
        }

        public static string FFmpegEditorFolder
        {
            get
            {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                return VRCaptureConfig.DATA_PATH + "/VRCapture/FFmpeg/Win/";
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
                return VRCommonConfig.DATA_PATH + "/VRCapture/FFmpeg/Mac/";
#endif
            }
        }

        public static string FFmpegEditorPath
        {
            get
            {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                return FFmpegEditorFolder + "ffmpeg.exe";
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
                return FFmpegEditorFolder + "ffmpeg";
#endif
            }
        }

        public static string FFmpegBuildFolder
        {
            get
            {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                return VRCaptureConfig.STREAMING_ASSETS_PATH + "/VRCapture/FFmpeg/Win/";
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
                return VRCommonConfig.STREAMING_ASSETS_PATH + "/VRCapture/FFmpeg/Mac/";
#endif
            }
        }

        // TODO, fix path using Unity Build Pipeline
        public static string FFmpegBuildPath
        {
            get
            {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                return FFmpegBuildFolder + "ffmpeg.exe";
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
                return FFmpegBuildFolder + "ffmpeg";
#endif
            }
        }

        public static string FFmpegPath
        {
            get
            {
#if UNITY_EDITOR
                return FFmpegEditorPath;
#else
                return FFmpegBuildPath;
#endif
            }
        }
    }
}