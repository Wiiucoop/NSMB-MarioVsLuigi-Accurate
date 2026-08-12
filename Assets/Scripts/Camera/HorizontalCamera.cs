using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HorizontalCamera : MonoBehaviour {
    public static float OFFSET_TARGET = 0f;
    public static float OFFSET_VELOCITY, OFFSET = 0f;
    private Camera ourCamera;
    
    public bool renderToTextureIfAvailable = true;
    private static float orthoSize = 3f;

    void Start() {
        ourCamera = GetComponent<Camera>();
        AdjustCamera();
    }
     
    private void Update() {
        OFFSET = Mathf.SmoothDamp(OFFSET, OFFSET_TARGET, ref OFFSET_VELOCITY, 1f);
        AdjustCamera();
        ourCamera.targetTexture = renderToTextureIfAvailable && Settings.Instance.ndsResolution && SceneManager.GetActiveScene().buildIndex != 0 
            ? GlobalController.Instance.ndsTexture 
            : null;
    }

    public static void setZoom(float size) {
        orthoSize = size;
    }

    public static float getZoom() {
        return orthoSize;
    }

    private void AdjustCamera() {
        //ACCURACY: Keep vertical FOV constant regardless of aspect ratio - horizontal FOV is left to vary naturally with the screen/window shape.
        double size = orthoSize + OFFSET;
        ourCamera.orthographicSize = (float) size;
    }
}
