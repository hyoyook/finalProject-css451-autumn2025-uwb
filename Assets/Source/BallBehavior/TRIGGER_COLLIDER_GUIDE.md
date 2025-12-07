# Ball Out of Bounds - Trigger Collider Setup

## Overview
The `BallOutOfBounds` script now supports two ways to return balls:
1. **Y-position detection**: Ball falls below `OutOfBoundsY` threshold (original behavior)
2. **Trigger Collider**: Ball touches a specific collider you assign (NEW!)

## Setup Instructions

### Option 1: Use Y-Position Only (Original)
1. Select the ball in Hierarchy
2. Find `BallOutOfBounds` component in Inspector
3. Leave `Trigger Collider` empty
4. Ball returns when it falls below `OutOfBoundsY`

### Option 2: Use Trigger Collider
Perfect for returning balls when they enter specific zones (pockets, return zones, etc.)

#### Step 1: Create the Trigger Zone
```
1. Create a new GameObject → 3D Object → Cube (or any shape)
2. Name it "BallReturnZone" (or "Pocket1", etc.)
3. Position it where you want the trigger
4. Scale it to cover the desired area
```

#### Step 2: Configure the Trigger
```
1. Select the trigger zone GameObject
2. In Inspector, find the Collider component
3. Check ✓ "Is Trigger"
4. Adjust size/position as needed
```

#### Step 3: Assign to Ball
```
1. Select the cue ball (or any ball)
2. Find "Ball Out Of Bounds" component
3. Drag the trigger zone into "Trigger Collider" field
4. Done!
```

### Option 3: Use Both!
You can use both methods at once:
- Ball returns if it falls too low (Y-position)
- Ball returns if it touches the trigger collider
- Whichever happens first triggers the return

## Example Use Cases

### Pocket Zones
```
Create invisible trigger zones at each pocket location.
When a ball enters a pocket, it returns to the top of the table.
```

### Return Zones
```
Create a trigger plane under the table.
Any ball that falls through gets returned.
```

### Special Areas
```
Create trigger zones for special game rules:
- Scratch pocket (cue ball returns)
- Out of bounds zones
- Reset zones
```

## Settings

### Trigger Collider
- **Type**: Collider (from any GameObject)
- **Optional**: Leave empty if not using
- **Works with**: Both trigger and solid colliders
- **Multiple balls**: Each ball can have different trigger colliders

### Out Of Bounds Y
- **Default**: 0.5
- **Purpose**: Fallback detection if ball falls off table
- **Works with**: Trigger collider (both methods active)

### Return Position
- **Type**: Vector3 (X, Y, Z coordinates)
- **Default**: (0, 2.147, 2) - head spot
- **Usage**: Where to place the ball when it returns

### Table Reference
- **Type**: Transform
- **Purpose**: Calculate correct height above table
- **Recommended**: Assign your table GameObject

## Testing

### Test Trigger Collider:
1. Add the trigger zone
2. Assign it to a ball's `Trigger Collider` field
3. Hit Play
4. Roll/shoot the ball into the trigger zone
5. Ball should return to the top of the table

### Verify Setup:
- Check Console for: `"[BallOutOfBounds] Ball touched trigger collider: [name]"`
- Ball should flash red briefly when returning
- Ball should stop moving (if `Freeze On Return` is checked)

## Visual Debugging

When you select a ball with this component:
- 🟢 **Green sphere**: Where the ball will return to
- 🔴 **Red grid**: The out-of-bounds Y plane
- 🔵 **Cyan line**: Connection to table (if assigned)

The trigger collider will show in Scene view as a green wireframe (if "Is Trigger" is checked).

## Tips

✅ **Use triggers for pockets**: More realistic than Y-position detection  
✅ **Set trigger zones slightly inside pockets**: Prevents early detection  
✅ **Use layers**: Put triggers on their own layer for organization  
✅ **Test with different shot speeds**: Make sure detection works at all speeds  
✅ **Combine both methods**: Trigger for pockets, Y-position as failsafe  

## Troubleshooting

### Ball doesn't return when touching trigger:
- ✓ Check `Trigger Collider` field is assigned
- ✓ Verify trigger GameObject has a Collider component
- ✓ Check collider has "Is Trigger" enabled (or leave unchecked for solid collision)
- ✓ Make sure ball's Rigidbody is not kinematic
- ✓ Check Console for debug messages

### Ball returns too early:
- Shrink the trigger collider size
- Move it deeper into the pocket/zone
- Adjust trigger position

### Ball doesn't return when falling:
- Check `Out Of Bounds Y` value
- Make sure it's above the bottom of your scene
- Increase the value if balls pass through before detection

### Multiple balls affected:
- Each ball needs its own `BallOutOfBounds` component
- Each can have different trigger colliders
- Or use the same trigger for all balls (like a shared reset zone)

## Advanced: Multiple Triggers Per Ball

Want a ball to return when touching ANY of several triggers?

**Solution 1**: Duplicate the script
```csharp
// Attach multiple BallOutOfBounds components
// Each with a different trigger collider
```

**Solution 2**: Use tags
```csharp
// Modify the script to check collision.collider.CompareTag("ReturnZone")
// Tag all your return zones with "ReturnZone"
```

## Code Behavior

The script now has two collision detection methods:

1. **`OnCollisionEnter`**: Detects solid collisions
   - Use when trigger collider is NOT set as trigger
   - Ball physically collides with the object

2. **`OnTriggerEnter`**: Detects trigger zones
   - Use when trigger collider IS set as trigger  
   - Ball passes through the trigger zone

Both methods check if the collided object matches your assigned `Trigger Collider`.
