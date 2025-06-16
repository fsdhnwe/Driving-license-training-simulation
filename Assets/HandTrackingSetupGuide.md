# 手部追蹤駕駛訓練系統設置指南

## 系統概述

這個系統整合了 MediaPipe 手部檢測、iVCam 攝像頭輸入和 Unity 3D 手部模型，讓您可以使用真實的手部動作來控制虛擬駕駛環境中的物件。

## 核心組件

### 1. HandController.cs
- **功能**: 主要的手部控制器，負責整合 MediaPipe 檢測結果和 3D 手模型
- **關鍵特性**:
  - 將 MediaPipe 座標轉換為 Unity 世界座標
  - 控制 3D 手部骨骼動畫
  - 手指尖碰撞檢測
  - 與可互動物件的交互

### 2. HandTrackingManager.cs
- **功能**: 整合 iVCam 攝像頭和 MediaPipe 的管理器
- **關鍵特性**:
  - 統一管理攝像頭和手部檢測
  - 可調節的追蹤頻率
  - 系統狀態監控

### 3. MediaPipeResultBridge.cs
- **功能**: 在 MediaPipe 和手部控制器之間橋接數據
- **關鍵特性**:
  - 多種結果獲取方式
  - 靜態結果緩存
  - 自動結果更新

### 4. DrivingInteractable.cs
- **功能**: 駕駛場景中的可互動物件
- **支持的互動類型**:
  - 方向盤 (SteeringWheel)
  - 檔桿 (GearShift)
  - 踏板 (Pedal)
  - 按鈕 (Button)
  - 開關 (Switch)
  - 滑桿 (Slider)

## 設置步驟

### 步驟 1: 基礎設置

1. **確保您的項目已經安裝了 MediaPipeUnity**
   - 檢查 `Assets/MediaPipeUnity` 資料夾是否存在
   - 確認 Hand Landmark Detection 場景可以正常運行

2. **設置 iVCam**
   - 確保手機上安裝了 iVCam 應用
   - 電腦上安裝了 iVCam 驅動程序
   - 測試 iVCam 連接是否正常

### 步驟 2: 場景設置

1. **創建手部追蹤管理器**
   ```csharp
   // 在場景中創建一個空的 GameObject
   GameObject manager = new GameObject("HandTrackingManager");
   manager.AddComponent<HandTrackingManager>();
   ```

2. **設置攝像頭**
   ```csharp
   // 添加 ivcam 組件到攝像頭物件
   Camera cam = Camera.main;
   cam.gameObject.AddComponent<ivcam>();
   ```

3. **創建手部控制器**
   ```csharp
   // 創建手部控制器
   GameObject handControllerObj = new GameObject("HandController");
   HandController handController = handControllerObj.AddComponent<HandController>();
   ```

### 步驟 3: 3D 手部模型設置

1. **導入手部模型**
   - 將您的 3D 手部模型 (handbynadevaynoskix.fbx) 拖入場景
   - 確保模型有正確的骨骼結構

2. **配置骨骼映射**
   ```csharp
   // 在 HandController 的 handBones 陣列中設置 21 個關鍵點對應的骨骼
   // MediaPipe 手部關鍵點順序：
   // 0: 手腕
   // 1-4: 拇指 (從手腕到指尖)
   // 5-8: 食指 (從手腕到指尖)
   // 9-12: 中指 (從手腕到指尖)
   // 13-16: 無名指 (從手腕到指尖)
   // 17-20: 小指 (從手腕到指尖)
   ```

### 步驟 4: MediaPipe 整合

1. **替換 HandLandmarkerRunner**
   - 在 MediaPipe 場景中找到 HandLandmarkerRunner
   - 替換為 ExtendedHandLandmarkerRunner
   - 或者添加 MediaPipeResultBridge 組件

2. **配置結果橋接**
   ```csharp
   // 設置 MediaPipeResultBridge
   MediaPipeResultBridge bridge = gameObject.AddComponent<MediaPipeResultBridge>();
   bridge.targetHandController = handController;
   bridge.handLandmarkerRunner = handLandmarkerRunner;
   ```

### 步驟 5: 可互動物件設置

1. **為駕駛物件添加互動功能**
   ```csharp
   // 為方向盤添加互動
   GameObject steeringWheel = GameObject.Find("SteeringWheel");
   DrivingInteractable steeringInteractable = steeringWheel.AddComponent<DrivingInteractable>();
   steeringInteractable.interactionType = DrivingInteractable.InteractionType.SteeringWheel;
   
   // 為油門踏板添加互動
   GameObject gasPedal = GameObject.Find("GasPedal");
   DrivingInteractable gasInteractable = gasPedal.AddComponent<DrivingInteractable>();
   gasInteractable.interactionType = DrivingInteractable.InteractionType.Pedal;
   ```

2. **配置車輛控制器整合**
   ```csharp
   // 在 RealisticCarController 中啟用手部控制
   RealisticCarController carController = FindObjectOfType<RealisticCarController>();
   carController.enableHandControl = true;
   carController.handController = handController;
   ```

### 步驟 6: UI 設置

1. **創建攝像頭顯示 UI**
   ```csharp
   // 創建 Canvas 和 RawImage 來顯示攝像頭畫面
   Canvas canvas = FindObjectOfType<Canvas>();
   GameObject rawImageObj = new GameObject("CameraDisplay");
   rawImageObj.transform.SetParent(canvas.transform);
   UnityEngine.UI.RawImage rawImage = rawImageObj.AddComponent<UnityEngine.UI.RawImage>();
   
   // 將 iVCam 的 targetObject 設置為這個 RawImage
   ivcam.targetObject = rawImageObj;
   ```

## 調試和測試

### 調試選項

1. **HandController 調試**
   - `showDebugInfo`: 顯示手部追蹤信息
   - `showLandmarks`: 顯示手部關鍵點

2. **HandTrackingManager 調試**
   - 內建的 OnGUI 顯示系統狀態
   - 可以手動開始/停止追蹤

### 常見問題解決

1. **手部檢測不工作**
   - 檢查 iVCam 連接
   - 確認 MediaPipe 模型文件存在
   - 檢查攝像頭權限

2. **3D 手部模型不動**
   - 檢查骨骼映射是否正確
   - 確認 HandController 收到了 MediaPipe 結果
   - 檢查座標轉換設置

3. **互動不響應**
   - 確認物件有 Collider 組件
   - 檢查 LayerMask 設置
   - 確認手指尖碰撞檢測器位置正確

## 性能優化

1. **降低追蹤頻率**
   ```csharp
   handTrackingManager.trackingFPS = 15; // 降低到 15 FPS
   ```

2. **調整平滑係數**
   ```csharp
   handController.smoothing = 0.05f; // 更快的響應
   ```

3. **禁用不必要的調試功能**
   ```csharp
   handController.showDebugInfo = false;
   handController.showLandmarks = false;
   ```

## 自定義擴展

### 添加新的互動類型

```csharp
public class CustomInteractable : MonoBehaviour, IInteractable
{
    public void OnInteractionStart(int fingerIndex)
    {
        // 自定義互動邏輯
    }
    
    public void OnInteractionEnd(int fingerIndex)
    {
        // 結束互動邏輯
    }
    
    public void OnInteractionStay(int fingerIndex)
    {
        // 持續互動邏輯
    }
}
```

### 自定義手勢識別

```csharp
public class GestureRecognizer : MonoBehaviour
{
    public void AnalyzeHand(HandLandmarkerResult result)
    {
        // 分析手部姿態
        // 識別特定手勢
        // 觸發相應事件
    }
}
```

## 項目結構

```
Assets/
├── HandController.cs              # 主要手部控制器
├── HandTrackingManager.cs         # 系統管理器
├── MediaPipeResultBridge.cs       # 結果橋接器
├── ExtendedHandLandmarkerRunner.cs # 擴展的 MediaPipe 運行器
├── FingerTipDetector.cs           # 手指尖檢測器
├── IInteractable.cs               # 互動介面
├── HandInteractableButton.cs      # 基本互動按鈕
├── DrivingInteractable.cs         # 駕駛互動物件
└── ivcam.cs                       # iVCam 攝像頭控制器 (已存在)
```

這個系統提供了一個完整的手部追蹤解決方案，讓您可以使用真實的手部動作來控制虛擬駕駛環境。通過調整各種參數和添加自定義互動物件，您可以創建豐富的手部互動體驗。
