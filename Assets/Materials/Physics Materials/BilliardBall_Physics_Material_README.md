**Billiard Ball Physics Material**

*Overview*

    This asset is a Physics Material configured specifically for billiard balls (Cue ball and object balls). Unlike the table cloth (Fabric), balls need to be hard, smooth, and elastic to conserve momentum and bounce correctly off rails and other balls.

*Material Properties*

    Dynamic Friction (Value = 0.2)

        Low resistance. Allows the ball to slide slightly upon impact (stun shots) and maintain momentum while rolling.

    Static Friction(Value = 0.2)

        Low resistance to starting movement. Prevents the ball from "sticking" to rails or the cloth.

    Bounciness (Value = 0.85)

        High elasticity. Essential for proper rebounds off cushions (rails) and the "click" reaction when hitting other balls.

    Friction Combine (Value = Minimum)

        Crucial Setting. Uses the lowest friction of the two touching objects. When touching the "Sticky" cloth, this forces the engine to use the Ball's slickness (0.2) rather than the Cloth's grip (0.6), letting it roll smoothly.

    Bounce Combine (Value = Maximum)

        Crucial Setting. Uses the highest bounciness. Since the Cloth has 0 bounce, this ensures the ball still bounces using its own high value (0.85) rather than averaging down to a dud.

*Interaction Logic (Ball vs. Cloth)*

    The magic happens in how this material interacts with your Fabric material:

        Friction: The Fabric has high friction (0.6). The Ball has low friction (0.2). Because we set the Ball's combine mode to Minimum, the physics engine uses 0.2.

            Result: The ball rolls smoothly across the cloth without stopping instantly.

        Bounciness: The Fabric has 0 bounce. The Ball has 0.85 bounce. Because we set the Ball's combine mode to Maximum, the physics engine uses 0.85.

            Result: The ball retains energy, but because the table is flat, it doesn't jump around. However, when it hits a Rail (which should also have a bouncy material), it rebounds sharply.

*How to Create & Apply*

    1. Create: Right-click in your Project window $\to$ Create $\to$ Physics Material. Name it BilliardBall.

    2. Configure: Input the values listed in the table above.

    3. Apply: Select your Cue Ball and all Object Balls. Drag this material into the Material slot on their Sphere Collider.

*Troubleshooting*

    Ball stops too fast? Lower the Dynamic Friction on this material to 0.1 or 0.05.

    Ball slides like ice (doesn't roll)? Increase the Dynamic Friction slightly. Rolling requires some friction to convert sliding energy into rotational energy.

    Rails feel dead? Ensure your Rails also have a Physics Material with high bounciness (e.g., 0.8).