# Making Billiard Balls Stop Sooner - Guide

## What I Changed

### 1. ✅ Created `BallPhysics.cs` Script
A new component that adds drag to make balls stop rolling sooner.

**Location:** `Assets/Source/BallBehavior/BallPhysics.cs`

### 2. ✅ Updated `Billiard_Ball.physicMaterial`
Increased friction values to add more surface resistance.

**Changes:**
- `Dynamic Friction`: 0.4 → **0.6** (more rolling resistance)
- `Static Friction`: 0.2 → **0.5** (harder to start moving)
- `Bounciness`: 0.85 → **0.75** (less bouncy)

## How to Use

### Setup (Do this for each ball):

1. **Select a ball** in your scene/hierarchy
2. **Add Component** → Search for "BallPhysics"
3. **Configure settings** (or use defaults):
   ```
   Linear Drag: 2.0      ← Higher = stops moving sooner
   Angular Drag: 2.0     ← Higher = stops spinning sooner
   Stop Velocity Threshold: 0.05
   Stop Angular Velocity Threshold: 0.05
   ```

### Adjusting Speed (Make them stop even faster):

**Option A: Increase Drag (Recommended)**
- Select ball → Find `BallPhysics` component
- Increase `Linear Drag` slider (try 3.0 or 4.0)
- Increase `Angular Drag` slider (try 3.0 or 4.0)

**Option B: Increase Friction**
- Open: `Assets/Materials/Physics Materials/Billiard_Ball.physicMaterial`
- Increase `Dynamic Friction` (try 0.7 or 0.8)
- Increase `Static Friction` (try 0.6 or 0.7)

**Option C: Both!**
- Use both methods above for maximum effect

## Settings Explained

### Linear Drag
- **What it does:** Slows down ball movement (translation)
- **Default:** 2.0
- **Recommended range:** 1.5 - 4.0
- **Higher = Stops moving faster**

### Angular Drag
- **What it does:** Slows down ball rotation (spinning)
- **Default:** 2.0
- **Recommended range:** 1.5 - 4.0
- **Higher = Stops spinning faster**

### Stop Velocity Threshold
- **What it does:** Forces ball to stop if moving very slowly
- **Default:** 0.05
- **Purpose:** Prevents endless slow rolling
- **Lower = Stops at higher speeds (stops sooner)**

### Dynamic Friction (Physics Material)
- **What it does:** Surface resistance when ball is moving
- **Current:** 0.6
- **Real billiard balls:** ~0.15-0.20 (very smooth)
- **For faster stopping:** 0.6-0.8 (more resistance)

### Static Friction (Physics Material)
- **What it does:** Resistance when ball starts moving
- **Current:** 0.5
- **Higher = Harder to start rolling**

## Quick Comparison

### Original Settings (Slower to stop):
```
Linear Drag: 0.0 (Unity default)
Angular Drag: 0.05 (Unity default)
Dynamic Friction: 0.4
Static Friction: 0.2
Bounciness: 0.85
```

### New Settings (Stops sooner):
```
Linear Drag: 2.0
Angular Drag: 2.0
Dynamic Friction: 0.6
Static Friction: 0.5
Bounciness: 0.75
```

### Aggressive Settings (Stops very quickly):
```
Linear Drag: 4.0
Angular Drag: 4.0
Dynamic Friction: 0.8
Static Friction: 0.7
Bounciness: 0.6
```

## Testing

1. **Add the script** to your balls
2. **Hit Play** and shoot a ball
3. **Watch how long it rolls**
4. **Adjust drag sliders** in real-time (while playing)
5. **Find your preferred feel**

## Tips

✅ **Start with defaults** (2.0 drag) and adjust from there  
✅ **Use consistent values** across all balls for realistic physics  
✅ **Test different shot strengths** - weak vs strong hits  
✅ **Consider table size** - larger tables may need less drag  
✅ **Match linear and angular drag** for balanced stopping  

## Troubleshooting

### Balls stop too quickly:
- **Reduce** Linear Drag (try 1.0 or 1.5)
- **Reduce** Angular Drag (try 1.0 or 1.5)
- **Lower** Dynamic Friction (try 0.4 or 0.5)

### Balls roll forever:
- **Increase** Linear Drag (try 3.0 or 4.0)
- **Increase** Angular Drag (try 3.0 or 4.0)
- **Raise** Dynamic Friction (try 0.7 or 0.8)
- **Lower** Stop Velocity Threshold (try 0.1 instead of 0.05)

### Balls feel "sticky":
- **Lower** Static Friction in the physics material
- **Reduce** Linear Drag slightly

### Balls bounce too much:
- **Already reduced** Bounciness to 0.75
- Can reduce further to 0.5-0.6 if needed

## Public Methods (For Scripting)

### Force Stop a Ball:
```csharp
BallPhysics ballPhysics = ball.GetComponent<BallPhysics>();
ballPhysics.ForceStop();
```

### Change Drag at Runtime:
```csharp
BallPhysics ballPhysics = ball.GetComponent<BallPhysics>();
ballPhysics.SetDrag(3.0f, 3.0f); // linear, angular
```
