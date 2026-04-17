# Plan — Vibe Coding Computer

## 1. Source Corpus Asset

1.1 Fetch a readable GStack source file from Gary Tan's GitHub (a `.ts` or `.py` file, ~500–1000 lines)  
1.2 Save as `Assets/Resources/GStackSourceCode.txt`  
1.3 Verify it loads via `Resources.Load<TextAsset>("GStackSourceCode")`  

## 2. Computer Desk Placeholder

2.1 Add a Cube to the scene as the desk (~1.2m × 0.8m × 0.6m), position at (−1, 0.4, 1)  
2.2 Add a smaller Cube on top as the monitor face (~0.8m × 0.6m × 0.05m)  
2.3 Add a `SphereCollider` (Is Trigger = true, Radius = 2.5) to the desk GameObject  
2.4 Tag the desk GameObject `Computer`  

## 3. ComboMultiplier

3.1 Create `Assets/Scripts/ComboMultiplier.cs`  
3.2 Track time since last keypress; increment multiplier toward 3× on each keypress based on cadence  
3.3 Decay multiplier back to 1× if no keypress for `decayDelay` seconds  
3.4 Expose `float Current` property and `void RegisterKeypress()` method  
3.5 On each change: call `GameManager.Instance.SetLOCPerSec(baseLocPerSec * multiplier)`  

## 4. ComputerDisplay

4.1 Create `Assets/Scripts/ComputerDisplay.cs`  
4.2 On `Awake`, load `GStackSourceCode.txt` from Resources into `corpusText`  
4.3 Build UI: dark Canvas panel (anchored center, ~60% screen width/height), ScrollRect with a TextMeshPro text child using a monospace font  
4.4 Expose `void TypeCharacters(int count)` — appends next `count` chars from corpus to display buffer, auto-scrolls to bottom  
4.5 When a full line is appended, insert a line break and scroll  
4.6 Expose `void Activate()` / `void Deactivate()` — show/hide the panel, reset scroll position on deactivate  

## 5. TypingInput

5.1 Create `Assets/Scripts/TypingInput.cs`  
5.2 Subscribe to `Keyboard.onTextInput` (New Input System) — fires on any printable keypress  
5.3 Guard with `isActive` flag — ignore keypresses when not near computer  
5.4 On keypress: call `GameManager.Instance.AddLOC(1)`, `ComputerDisplay.TypeCharacters(charsPerKeypress)`, `ComboMultiplier.RegisterKeypress()`  
5.5 On `Update`: call `ComboMultiplier.Tick(Time.deltaTime)` to handle decay  

## 6. Proximity Trigger

6.1 Create `Assets/Scripts/ComputerProximity.cs` on the desk GameObject  
6.2 `OnTriggerEnter`: if collider is tagged `CameraPivot` → `TypingInput.isActive = true`, `ComputerDisplay.Activate()`  
6.3 `OnTriggerExit`: `TypingInput.isActive = false`, `ComputerDisplay.Deactivate()`, `GameManager.Instance.SetLOCPerSec(0)`  
6.4 Tag the camera pivot GameObject `CameraPivot`; add a small `SphereCollider` (non-trigger, radius 0.3) to it for detection  

## 7. Scene Wiring

7.1 Add `TypingInput` GameObject to scene, assign `ComputerDisplay` and `ComboMultiplier` references  
7.2 Confirm `GameManager` is in scene and `TypingInput` can reach `GameManager.Instance`  
7.3 Test proximity: pan camera toward desk → display activates; pan away → deactivates  

## 8. Tests

8.1 Create `Assets/Tests/EditMode/ComboMultiplierTests.cs`  
8.2 Test: multiplier starts at 1×  
8.3 Test: rapid keypresses push multiplier toward 3×  
8.4 Test: after `decayDelay` seconds of no input, multiplier returns to 1×  
8.5 Test: `LOCPerSec` passed to GameManager equals `baseLocPerSec * multiplier`  
