using UnityEditor;

namespace VRCapture.Editor {
    
    [CustomEditor(typeof(VRCaptureVideo))]
    public class VRCaptureVideoEditor : UnityEditor.Editor {

        VRCaptureVideo captureVideo;

        VRCaptureVideo.FrameSizeType frameSize;
        VRCaptureVideo.CubemapSizeType cubemapSize;
        VRCaptureVideo.EquirecatngularFrameSizeType equirecatngularFrameSize;
        VRCaptureVideo.EncodeQualityType encodeQuality;
        VRCaptureVideo.AntiAliasingType antiAliasing;
        VRCaptureVideo.TargetFramerateType targetFramerate;
        bool isDedicated;
        bool isEnabled;
        bool offlineRender;

        void Awake() {
            captureVideo = (VRCaptureVideo)target;
            frameSize = captureVideo.frameSize;
            cubemapSize = captureVideo.cubemapSize;
            equirecatngularFrameSize = captureVideo.equirecatngularFrameSize;
            encodeQuality = captureVideo.encodeQuality;
            antiAliasing = captureVideo.antiAliasing;
            targetFramerate = captureVideo.targetFramerate;
            isDedicated = captureVideo.isDedicated;
            isEnabled = captureVideo.isEnabled;
            offlineRender = captureVideo.offlineRender;
        }
        
        public override void OnInspectorGUI() {
            captureVideo.captureType = 
                (VRCaptureVideo.CaptureType)EditorGUILayout.EnumPopup("Capture Type", captureVideo.captureType);
            if (captureVideo.captureType == VRCaptureVideo.CaptureType.NORMAL) {
                frameSize = (VRCaptureVideo.FrameSizeType)EditorGUILayout.EnumPopup("Frame Size", frameSize);
            }
            else if (captureVideo.captureType == VRCaptureVideo.CaptureType.EQUIRECTANGULAR) {
                cubemapSize = (VRCaptureVideo.CubemapSizeType)EditorGUILayout.EnumPopup("Cubemap Size", cubemapSize);
                equirecatngularFrameSize = (VRCaptureVideo.EquirecatngularFrameSizeType)EditorGUILayout.EnumPopup(
                    "Equirecatngular Frame Size", equirecatngularFrameSize);
                offlineRender = EditorGUILayout.Toggle("Offline Render", offlineRender);
            }
            encodeQuality = (VRCaptureVideo.EncodeQualityType)EditorGUILayout.EnumPopup("Encode Quality", encodeQuality);
            antiAliasing = (VRCaptureVideo.AntiAliasingType)EditorGUILayout.EnumPopup("Anti Aliasing", antiAliasing);
            targetFramerate = (VRCaptureVideo.TargetFramerateType)EditorGUILayout.EnumPopup("Target FrameRate", targetFramerate);
            isDedicated = EditorGUILayout.Toggle("Dedicated Camera", isDedicated);
            isEnabled = EditorGUILayout.Toggle("Enabled", isEnabled);

            // Capture video configuration.
            captureVideo.frameSize = frameSize;
            captureVideo.cubemapSize = cubemapSize;
            captureVideo.equirecatngularFrameSize = equirecatngularFrameSize;
            captureVideo.encodeQuality = encodeQuality;
            captureVideo.antiAliasing = antiAliasing;
            captureVideo.targetFramerate = targetFramerate;
            captureVideo.isDedicated = isDedicated;
            captureVideo.isEnabled = isEnabled;
            captureVideo.offlineRender = offlineRender;
        }
    }
}
