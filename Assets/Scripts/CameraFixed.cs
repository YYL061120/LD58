using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFixed : MonoBehaviour
{

    // 目标比例：4:3（800x600 就是这个比例）
    public float targetAspect = 4f / 3f;

    void Start() { Apply(); }
    void OnEnable() { Apply(); }
    void OnValidate() { Apply(); }

    void Apply()
    {
        var cam = GetComponent<Camera>();
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1f)
        {
            // 窗口比目标更“窄”——上下加黑边（letterbox）
            cam.rect = new Rect(0f, (1f - scaleHeight) / 2f, 1f, scaleHeight);
        }
        else
        {
            // 窗口比目标更“宽”——左右加黑边（pillarbox）
            float scaleWidth = 1f / scaleHeight;
            cam.rect = new Rect((1f - scaleWidth) / 2f, 0f, scaleWidth, 1f);
        }
    }
}
