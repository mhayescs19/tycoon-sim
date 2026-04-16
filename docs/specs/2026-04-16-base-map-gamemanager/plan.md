# Plan — Base Map + GameManager

## 1. Scene — Room Geometry

1.1 Open `Assets/Scenes/SampleScene.unity`  
1.2 Delete default sample objects (keep directional light)  
1.3 Create a `Room` empty GameObject as parent  
1.4 Add primitive planes/cubes for floor, four walls, ceiling — scale to approximate one-room SF apartment (~6m × 8m × 3m)  
1.5 Apply a flat URP material to each surface (grey/white placeholder)  
1.6 Add a point light or adjust directional light for a dim-apartment feel  
1.7 Add Camera GameObject, attach `CameraController.cs`, set initial position (0, 7, -5)  

## 2. GameManager

2.1 Create `Assets/Scripts/GameManager.cs`  
2.2 Implement singleton pattern (`public static GameManager Instance`)  
2.3 Add serialized fields: `DollarBalance` (47f), `LOCToDollarRate` (0.1f)  
2.4 Add runtime fields: `LOCCount`, `LOCPerSec`  
2.5 Declare `public event Action<float> OnBalanceChanged` and `public event Action<int, float> OnLOCChanged`  
2.6 Implement `AddLOC(int amount)` — increments LOCCount, converts to Dollars, fires both events  
2.7 Implement `AddDollars(float amount)` and `SpendDollars(float amount)` — fire `OnBalanceChanged`  
2.8 Add `GameManager` GameObject to scene, attach script  

## 3. Stub HUD

3.1 Create `Assets/Scripts/HUD.cs`  
3.2 Add a Canvas (Screen Space — Overlay) to the scene  
3.3 Add two TextMeshPro labels: `locText` (top-left) and `balanceText` (below it)  
3.4 In `HUD.Start()`, subscribe to `GameManager.OnBalanceChanged` and `GameManager.OnLOCChanged`  
3.5 Update `locText` to show `"LOC: {count} ({locPerSec:F1}/sec)"`  
3.6 Update `balanceText` to show `"$ {balance:F2}"`  
3.7 Unsubscribe in `OnDestroy`  

## 4. Tests

4.1 Create `Assets/Tests/EditMode/GameManagerTests.cs`  
4.2 Test: `AddLOC` increases `LOCCount` by correct amount  
4.3 Test: `AddLOC` converts LOC to Dollars at `LOCToDollarRate`  
4.4 Test: `SpendDollars` reduces balance; cannot go below zero  
4.5 Test: `OnBalanceChanged` fires on balance change  
