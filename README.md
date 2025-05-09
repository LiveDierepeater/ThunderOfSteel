Spatial Hashing & Unit Sight System
=========================================
The project was made in my first month's coding with Unity C#.

This project implements realistic tank movement, unit targeting, and tactical gameplay 
in a real-time strategy game using Unity. The core mechanics revolves around units with 
specialized movement behaviors, such as tanks, infantry, and reconnaissance units, all 
working in conjunction with a spatial hash system for efficient unit management and interaction.
The goal for this project was to implement a sight system, spatial hashing and "realistic"
tank movement behavior.

The focus on this project was to implement the mentioned systems, so it's far away from having
a gameplay loop. The mechanics are heavily inspired by the arcade RTS game called "R.U.S.E" from
Eugene Systems published by Ubisoft in 2010.

-----------------------------------------
Table of Contents
-----------------------------------------
1. Project Overview
2. Features
3. How It Works
4. Technologies Used
6. License

-----------------------------------------
Project Overview
-----------------------------------------
The game focuses on creating realistic tank movements, including acceleration, deceleration, 
and turning behavior, along with an advanced line-of-sight system influenced by terrain 
(e.g., forests reducing sight range). The project implements a spatial hash system for 
efficient unit management, where each unit belongs to a specific player ID, and their behavior 
and visibility can be controlled.

Main Features:
--------------
- Spatial Hashing: Efficiently manage unit positions and interactions in the game world.
- Sight System: Units, especially scouts, can detect and target enemy units based on 
  line-of-sight, modified by terrain.
- Unit Targeting and Combat: Units can detect and attack enemy units within their range, 
  with specialized behavior for each unit type (e.g., infantry, tanks).
- Tank Movement: Implement realistic tank behavior, including slow turning and braking 
  based on velocity.

Sight and Visibility System:
----------------------------
- Line of Sight: Units can only target or attack enemy units that are within their field of view and range.
- Terrain Influence: Forests or other terrain types reduce the sight range for units.
- Reconnaissance Units: Special units like scouts can have extended sight ranges and can reveal enemy 
  positions to other units.

Spatial Hashing for Efficient Unit Management:
------------------------------------------------
- Efficient Lookups: Units are divided into grids using a spatial hash system, allowing for fast 
  lookups and nearby unit searches.
- Neighbor Search: Units can detect nearby enemies based on spatial hash data and attack or interact accordingly.

Tank Movement Behavior:
------------------------
- NavMesh Integration: The project uses Unity's NavMesh system to handle pathfinding and movement.
- Tank-Specific Logic: Tanks have unique behaviors, such as slow acceleration and deceleration, 
  and slower turning speeds.
- Acceleration and Deceleration Curves: The movement speed of tanks is modulated by predefined curves, 
  providing smooth, realistic motion.
- Custom Movement for Other Units: Infantry units and other types of units have different movement behaviors.

-----------------------------------------
How It Works
-----------------------------------------

Sight System:
-------------
- Line of Sight Calculation: The visibility of enemy units is determined by line of sight, 
  taking terrain like forests into account. When units are blocked by terrain, their sight range 
  is reduced accordingly.
- Dynamic Range: Depending on the terrain and unit type, the visibility range can dynamically adjust.
  So the possible view range of a unit is reduced in a direction when i.e. a forest is in the way.

Spatial Hashing:
----------------
- Grid-based Organization: The world is divided into a grid, and each grid cell holds a list of units. 
  This allows for fast access to units within a certain area.
- Efficient Searching: The system allows searching for nearby units using spatial hash keys, ensuring 
  quick lookups and minimizing unnecessary calculations.

Tank Movement:
--------------
- The TankMovement component implements the movement behavior for tanks, including:
  - Acceleration and Deceleration: The tank's movement speed is controlled using animation curves, 
    providing smooth transitions between stopping, starting, and turning.
  - Turning Mechanism: The turning of tanks is slower compared to infantry units, as it simulates the 
    slower rotation speed of real tanks.

-----------------------------------------
Technologies Used
-----------------------------------------
- Unity: Game engine used to build the project.
- NavMesh: Used for pathfinding and unit movement.
- C#: Programming language for scripting the game mechanics.
- Spatial Hashing: For efficient unit and position management.
- Animation Curves: Used to control acceleration, deceleration, and movement behavior.

-----------------------------------------
License
-----------------------------------------
This project is licensed under the MIT License - see the LICENSE file for details.
