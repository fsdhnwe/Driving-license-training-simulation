using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera firstPersonCam;
    public Camera thirdPersonCam;

    private void Start()
    {
        // 一開始只啟用第三人稱
        firstPersonCam.enabled = false;
        thirdPersonCam.enabled = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C)) // 按 C 鍵切換
        {
            bool firstActive = firstPersonCam.enabled;
            firstPersonCam.enabled = !firstActive;
            thirdPersonCam.enabled = firstActive;
        }
    }
}
