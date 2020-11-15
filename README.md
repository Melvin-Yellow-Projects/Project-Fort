# Project: Fort
### Turn-Based Strategy Game


### Description
A medieval, chess-like strategy game where multiple players compete against one another for control of forts


### Tags
<strong>#strategy #chess #turn-based #medieval #RTS</strong>


### Todo
List of objectives to complete

- **Week 0 (September 21, 2020): Initialization I**
	- Game Design
		- [x] initial concept design
		- [x] keyframe design
		- [x] piece design I
		- [x] combat system I
		- [x] day and night cycle design
	- Tutorials
		- [x] Catlike Coding Hex Map, Creating a Hexagonal Grid
		- [x] Catlike Coding Hex Map, Blending Cell Colors
		- [x] Catlike Coding Hex Map, Elevation and Terraces
		- [x] Catlike Coding Hex Map, Irregularity
		- [x] Catlike Coding Hex Map, Larger Maps

- **Week 1 (September 28, 2020): Initialization II**
	- Game Design
		- [x] movement system (turn-based, simultaneous turns, realtime) 
		- [x] multiplayer design I
		- [x] piece stacking & collisions
	- Tutorials
		- [x] Catlike Coding Hex Map, Terrain Textures
		- [x] Catlike Coding Hex Map, Distances
		- [x] Catlike Coding Hex Map, Pathfinding
		- [x] Catlike Coding Hex Map, Saving and Loading
		- [x] Catlike Coding Hex Map, Managing Maps

- **Week 2 (October 5, 2020): Initialization III**
	- Game Design
		- [x] neutral game I
		- [x] camp fires
	- Gameplay
		- [x] hide editor UI in game mode
	- Quality of Life
		- [x] enter button auto loads a map
	- Tutorials
		- [x] Catlike Coding Hex Map, Limited Movement
		- [x] Catlike Coding Hex Map, Units
		- [x] Catlike Coding Hex Map, Animating Movement
		- [x] Catlike Coding Hex Map, Fog of War
		- [x] Catlike Coding Hex Map, Exploration
		- [x] Catlike Coding Hex Map, Advanced Sight
	- Other
		- [x] organize README.md
		- [x] emails 
		
- **Week 3 (October 12, 2020): Slow Start**
	- Game Design
		- [x] rotation as movement cost
	- Gameplay
		- hex curser
			- [x] arrow points from a start to an end point
			- [x] can gray out if not selected
			- [x] multiple cursers can exist simultaneously
		- new pathfinding
			- [x] get relative bridge direction from a point within a cell
			- [x] rotation now is added to the movement cost
		- [x] can change direction
	- Quality of Life
		- [x] refactor pathfinding logic from hex grid into new class

- **Week 4 (November 9, 2020): Version 1.0 Functionality**
	- Game Design
		- [x] hex curser inputs
		- [ ] turns and rounds
		- [ ] what unit mechanics are essential to implement?
	- Gameplay
		- pathfinding and path-building
			- [x] manual path building with direct mouse selection
			- [x] if manual path is too large, A* is used
			- [x] shift allows a unit to retrace cells; otherwise, retracing starts A*
			- [x] unit must remember path until it is allowed to forget 
			- [x] curser indicates if there is an error or if it is selected
			- [x] pieces cannot travel through eachother
		- [x] moves are stored by units until a button is pressed
		- [x] ally collisions causes a random piece to wait
		- main menu and scene navigation
			- [x] main menu
			- [x] complete navigation loop
			- [x] systems menu (settings, quit, more information)
			- [x] popup menu
			- [x] start scene passes map data to game scene
		- player GameObject
			- [ ] refactor HexGameUI script into player
			- [ ] left click for selection, right click for execution
			- [ ] has a list of owned units
			- [ ] cannot use enemy units
			- [ ] cannot see enemy units and their field of view
			- [ ] team data, color data 
			- [ ] list of base/forts
		- combat
			- [x] moving pieces kill enemies
			- [ ] multiple moving pieces trade eachother off
		- different game modes and solid game manager
			- [x] turn clock/timer
			- [x] simultaneous turns
			- [x] real-time
			- [ ] turn-based
			- [ ] hot seat
			- [ ] game mode class?
		- unit variety
			- [ ] refactor cell, edge, attack validation functions into unit class
			- [ ] pikeman
			- [ ] knight
			- [ ] pirate
			- [ ] wall
		- unit mechancis
			- [ ] knight can jump pieces
 			- [ ] knight cannot move onto a square with a Pikeman
			- [ ] knight has extra movement
			- [ ] walls cannot be killed unless pirate
			- [ ] walls move with priority
			- [ ] walls can be cursed
			- [ ] walls can not be cursed
		- [ ] some built in maps or simply be able to transfer map over client
		- [ ] server client relationship
	- Graphics
		- [x] hexcell lights up when mouse is tracing over it
		- [x] death animation
	- Debugging
		- [x] remove rotation as movement cost 😔
		- [x] remove direction as a gameplay factor 😔
	- Other
		- [ ] resume 1 second a day videos

- **Week 5 (November 16, 2020): Version 1.0 Multiplayer and Build**
	- Gameplay
		- [ ] water cells
		- [ ] archer
		- [ ] forts spawn units 
	- Graphics
		- [ ] piece model updates
		- [ ] can pick up an invisible instance of the piece when creating a path
		- [ ] water graphics
	- Sound
		- [ ] sounds
		- [ ] drum for piece movement
	- Quality of Life
		- [ ] refactoring
		- [ ] comments


### Future Dates & Features
List of future objectives to complete

- **Week 6 (November 23, 2020): xxx**
	- *add next set of deliverables*

- **Features**
	- Tutorials
		- [ ] Blender
		- [ ] 3D animation rigging
		- [ ] Spine weights
		- [ ] Spine UI assets
		- [ ] Shaders & Materials II
		- [ ] Mirror Multiplayer
		- [ ] fire flies
		- [ ] campfire scene
		- [ ] fountain asset
		- [ ] finish lima with multiplayer
	- Game Design
		- [ ] economy		
		- [ ] cell queue combat flow
	- Gameplay
		- [ ] variant map shapes
		- [ ] rivers edges
		- [ ] rock edges 
		- unit abilities
			- [ ] ability UI
			- [ ] left click to execute ability
		- [ ] cells and rotations can be added to a travel queue 1 at a time
		- [ ] mousing over cell edge can change a unit's direction in A* mode
	- Graphics
		- [ ] ability descriptions (like a modal view descriptor seen for card games)
		- hex curser details
			- [ ] can gray out a section of the path
			- [ ] supports overlapping, curser can go backwards ontop of itself cohesively
			- [ ] Bézier implementation
			- [ ] animation
		- basic piece combat
			- [ ] combat animation
			- [ ] dust effect
			- [ ] movement dust asset
	- Sound
		- *pending requirements*
	- Quality of Life
		- [ ] comprehend shader code
		- [ ] parameterize number of edge vertices
		- [ ] pathfinding display debugger is readded
	- Debugging
		- [ ] HexCurser has an error with HasError flag if it is spawned in A* range
	- Other
		- [ ] Trello
		- [ ] multiplayer test project
		- [ ] Reddit Page
		- [ ] Instagram
		- [ ] Twitter
		- [ ] Steam page
		- [ ] Discord
		- [ ] website
		- [ ] create a 1 second video collection starting from November 1st
		- [ ] *pending requirements*
