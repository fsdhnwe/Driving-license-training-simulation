using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ivcam : MonoBehaviour
{
    [Tooltip("目标渲染对象（可以是UI.RawImage或者Renderer组件）")]
    public GameObject targetObject;

    [Tooltip("iVCam设备名称，通常为'e2eSoft iVCam'")]
    public string iVCamDeviceName = "e2eSoft iVCam";

    [Tooltip("摄像头分辨率宽度")]
    public int resolutionWidth = 1280;

    [Tooltip("摄像头分辨率高度")]
    public int resolutionHeight = 720;

    [Tooltip("摄像头帧率")]
    public int frameRate = 30;

    [Tooltip("是否镜像摄像头画面")]
    public bool mirrorImage = true;

    [Tooltip("启动延迟时间（秒）")]
    public float startDelay = 0.5f;

    [Header("故障排除")]
    [Tooltip("是否在游戏对象上显示调试UI按钮")]
    public bool showDebugUI = false;

    private WebCamTexture webcamTexture;
    private bool isCamAvailable = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (targetObject == null)
        {
            targetObject = gameObject;
            Debug.LogWarning("没有指定目标渲染对象，将使用当前游戏对象。");
        }

        StartCoroutine(InitializeWebcam());
    }

    IEnumerator InitializeWebcam()
    {
        yield return new WaitForSeconds(startDelay);

        // 列出所有可用摄像头设备
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.LogError("未检测到任何摄像头设备！请确保iVCam已正确安装和连接。");
            yield break;
        }

        // 输出所有可用的摄像头设备信息
        Debug.Log("检测到 " + devices.Length + " 个摄像头设备:");
        bool deviceFound = false;
        foreach (var device in devices)
        {
            Debug.Log("摄像头: " + device.name);
            if (device.name.Contains(iVCamDeviceName))
            {
                deviceFound = true;
            }
        }

        if (!deviceFound)
        {
            Debug.LogWarning("未找到 iVCam 设备'" + iVCamDeviceName + "'。请确保：\n" +
                "1. 手机上已安装并运行 iVCam 应用\n" +
                "2. 电脑上已安装 iVCam 驱动程序\n" +
                "3. 手机和电脑连接到同一个WiFi网络或通过USB连接\n" +
                "4. iVCam 应用在手机上运行并显示'Connected'状态");
        }

        // 创建 WebCamTexture 并设置参数
        webcamTexture = new WebCamTexture(iVCamDeviceName, resolutionWidth, resolutionHeight, frameRate);
          // 应用到渲染对象
        if (targetObject.GetComponent<RawImage>() != null)
        {
            // UI方式显示
            RawImage rawImage = targetObject.GetComponent<RawImage>();
            rawImage.texture = webcamTexture;
            if (mirrorImage)
                rawImage.uvRect = new Rect(1, 0, -1, 1);
        }
        else if (targetObject.GetComponent<Renderer>() != null)
        {
            // 3D对象方式显示
            Renderer renderer = targetObject.GetComponent<Renderer>();
            
            // 确保对象有材质
            if (renderer.material == null)
            {
                renderer.material = new Material(Shader.Find("Unlit/Texture"));
            }
            
            renderer.material.mainTexture = webcamTexture;
            if (mirrorImage)
                renderer.material.mainTextureScale = new Vector2(-1, 1);
        }
        else
        {
            Debug.LogWarning("目标对象没有RawImage或Renderer组件，正在自动添加所需组件...");
            
            // 根据对象类型自动添加合适的组件
            if (targetObject.transform is RectTransform)
            {
                // 对象是UI元素，添加RawImage组件
                RawImage rawImage = targetObject.AddComponent<RawImage>();
                rawImage.texture = webcamTexture;
                if (mirrorImage)
                    rawImage.uvRect = new Rect(1, 0, -1, 1);
                
                Debug.Log("已自动添加RawImage组件到UI对象");
            }
            else
            {
                // 对象是3D对象，添加Renderer组件（如果没有）和MeshFilter（如果没有）
                if (targetObject.GetComponent<MeshFilter>() == null)
                {
                    MeshFilter meshFilter = targetObject.AddComponent<MeshFilter>();
                    meshFilter.mesh = CreateQuadMesh();
                    Debug.Log("已自动添加MeshFilter组件");
                }
                
                Renderer renderer = targetObject.AddComponent<MeshRenderer>();
                renderer.material = new Material(Shader.Find("Unlit/Texture"));
                renderer.material.mainTexture = webcamTexture;
                if (mirrorImage)
                    renderer.material.mainTextureScale = new Vector2(-1, 1);
                
                Debug.Log("已自动添加Renderer组件到3D对象");
            }
        }        // 启动摄像头
        webcamTexture.Play();
        
        // 增加更长的等待时间，某些设备需要更多时间初始化
        float waitTimeTotal = 0f;
        float maxWaitTime = 5.0f; // 最多等待5秒
        float checkInterval = 0.5f;
        
        while (waitTimeTotal < maxWaitTime)
        {
            yield return new WaitForSeconds(checkInterval);
            waitTimeTotal += checkInterval;
            
            // 检查摄像头是否开始提供画面
            if (webcamTexture.width > 100 && webcamTexture.height > 100) // 避免获取到无效的初始分辨率
            {
                Debug.Log("iVCam 摄像头已成功连接并开始工作。分辨率: " + webcamTexture.width + "x" + webcamTexture.height);
                isCamAvailable = true;
                break;
            }
            
            Debug.Log("等待摄像头初始化... 已等待 " + waitTimeTotal + " 秒");
        }
        
        // 检查摄像头是否正常工作
        if (!isCamAvailable)
        {
            // 提供详细的诊断信息
            string errorMsg = "iVCam 摄像头连接但未能正常工作。\n";
            errorMsg += "诊断信息:\n";
            errorMsg += "- 是否正在播放: " + webcamTexture.isPlaying + "\n";
            errorMsg += "- 当前分辨率: " + webcamTexture.width + "x" + webcamTexture.height + "\n";
            errorMsg += "- 设备名称: " + webcamTexture.deviceName + "\n";
            
            errorMsg += "\n可能的解决方案:\n";
            errorMsg += "1. 确保手机上的iVCam应用处于运行状态并显示'Connected'\n";
            errorMsg += "2. 重启手机上的iVCam应用\n";
            errorMsg += "3. 检查电脑上的iVCam驱动程序是否正确安装\n";
            errorMsg += "4. 尝试重新连接USB或确保手机和电脑在同一WiFi网络\n";
            errorMsg += "5. 在手机iVCam应用中尝试降低视频质量设置\n";
            errorMsg += "6. 检查防火墙设置是否阻止了iVCam连接\n";
            
            Debug.LogWarning(errorMsg);
            
            // 尝试再次启动摄像头
            webcamTexture.Stop();
            yield return new WaitForSeconds(1.0f);
            webcamTexture.Play();
            yield return new WaitForSeconds(1.0f);
            
            if (webcamTexture.width > 100 && webcamTexture.height > 100)
            {
                Debug.Log("iVCam 摄像头在第二次尝试后成功连接。分辨率: " + webcamTexture.width + "x" + webcamTexture.height);
                isCamAvailable = true;
            }
        }
    }

    // 创建一个简单的四边形网格
    private Mesh CreateQuadMesh()
    {
        Mesh mesh = new Mesh();
        
        // 顶点
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3(0.5f, -0.5f, 0),
            new Vector3(-0.5f, 0.5f, 0),
            new Vector3(0.5f, 0.5f, 0)
        };
        mesh.vertices = vertices;

        // 三角形
        int[] triangles = new int[6]
        {
            0, 2, 1,
            2, 3, 1
        };
        mesh.triangles = triangles;

        // UV坐标
        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        mesh.uv = uv;
        
        // 计算法线
        mesh.RecalculateNormals();
        
        return mesh;
    }

    // Update is called once per frame
    void Update()
    {
        if (isCamAvailable && webcamTexture != null)
        {
            // 在这里可以添加对摄像头画面的实时处理
            // 例如：获取摄像头像素数据进行分析
            // Color32[] pixelData = webcamTexture.GetPixels32();
        }
    }

    void OnDestroy()
    {
        // 关闭摄像头并释放资源
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            webcamTexture.Stop();
        }
    }

    // 公开的方法，允许外部重新连接摄像头
    public void ReconnectCamera()
    {
        if (webcamTexture != null)
        {
            if (webcamTexture.isPlaying)
            {
                webcamTexture.Stop();
            }
            
            StartCoroutine(InitializeWebcam());
        }
    }
    
    // 调试方法，用于输出所有可用的摄像头设备
    public void ListAllCameras()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        string devicesInfo = "检测到 " + devices.Length + " 个摄像头设备:\n";
        
        foreach (var device in devices)
        {
            devicesInfo += "- " + device.name + "\n";
            devicesInfo += "  是前置摄像头: " + device.isFrontFacing + "\n";
            
            // Unity 2022 及以上版本支持这些属性
            #if UNITY_2022_1_OR_NEWER
            devicesInfo += "  支持的分辨率: \n";
            foreach (var res in device.availableResolutions)
            {
                devicesInfo += "    " + res.width + "x" + res.height + "\n";
            }
            #endif
        }
        
        Debug.Log(devicesInfo);
    }

    private void OnGUI()
    {
        if (showDebugUI)
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            
            if (GUILayout.Button("重新连接摄像头", GUILayout.Height(40)))
            {
                ReconnectCamera();
            }
            
            if (GUILayout.Button("列出所有摄像头", GUILayout.Height(40)))
            {
                ListAllCameras();
            }
            
            if (webcamTexture != null)
            {
                GUILayout.Label("摄像头状态: " + (isCamAvailable ? "正常工作" : "未工作"));
                GUILayout.Label("分辨率: " + webcamTexture.width + "x" + webcamTexture.height);
                GUILayout.Label("实际FPS: " + (1.0f / Time.deltaTime).ToString("F1"));
                
                if (GUILayout.Button("切换镜像", GUILayout.Height(40)))
                {
                    mirrorImage = !mirrorImage;
                    UpdateMirrorSetting();
                }
            }
            
            GUILayout.EndArea();
        }
    }
    
    // 更新镜像设置
    private void UpdateMirrorSetting()
    {
        if (targetObject == null || webcamTexture == null) return;
        
        if (targetObject.GetComponent<RawImage>() != null)
        {
            RawImage rawImage = targetObject.GetComponent<RawImage>();
            rawImage.uvRect = mirrorImage ? new Rect(1, 0, -1, 1) : new Rect(0, 0, 1, 1);
        }
        else if (targetObject.GetComponent<Renderer>() != null)
        {
            Renderer renderer = targetObject.GetComponent<Renderer>();
            renderer.material.mainTextureScale = mirrorImage ? new Vector2(-1, 1) : new Vector2(1, 1);
        }
    }
}
