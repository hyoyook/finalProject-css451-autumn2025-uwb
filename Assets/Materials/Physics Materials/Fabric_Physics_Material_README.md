**Fabric Physics Material**

*Overview*

    This asset is a Physics Material configured to simulate the physical properties of heavy fabric, thick carpet, or the felt on a billiard table. It is designed to provide high traction and zero restitution (bounciness), creating a "dead" surface that relies on the colliding object to determine smoothness.   
    
*Material Properties*

    Dynamic Friction (Value = 0.6)

        Moderate-high resistance while moving. Objects will decelerate quickly when sliding across this surface.

    Static Friction (Value = 0.8)

        High resistance to initial movement. Objects require significant force to start sliding.

    Bounciness (Value = 0)

        Zero elastic energy restitution. Objects will "thud" and stop immediately upon impact rather than bouncing.

    Friction Combine (Value = Average)

        The final friction is calculated by averaging this value with the colliding object's friction.

    Bounce Combine (Value = Average)

        The final bounciness is calculated by averaging this value with the colliding object's bounciness.

*Interaction Logic (Cloth vs. World)*

    This material acts as the "Base Layer" for your physics world.

    For Billiard Balls: The cloth provides a high friction value (0.6). However, because the Ball is set to Minimum combine, the physics engine ignores this high value in favor of the ball's low friction (0.2). This allows the ball to roll smoothly despite the rough surface.

    For Other Objects: If you drop a standard cube (with no physics material) on this cloth, it will default to Average combine. It will average its own friction with the cloth's 0.6, resulting in a sticky interaction where the cube stops quickly—exactly what you want for a chalk cube or a racking triangle.

*How to Create & Apply*

    Create: Right-click in your Project window $\to$ Create $\to$ Physics Material. Name it Fabric.

    Configure: Input the values listed in the table above.

    Apply: Select the Table Bed (the plane or mesh representing the cloth). Drag this material into the Material slot on its Mesh Collider or Box Collider.

*Troubleshooting*

    Balls stopping instantly? This usually means the Ball's material is missing or set to "Average" combine instead of "Minimum". The cloth is doing its job (being sticky), but the ball isn't overriding it.

    Balls bouncing on the table surface? Ensure Bounciness is set to 0. If it is, check that your table mesh collider is flat and doesn't have tiny bumps.