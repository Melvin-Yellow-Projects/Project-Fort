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
			- [x] animation
		- new pathfinding
			- [x] get relative bridge direction from a point within a cell
			- [x] rotation now is added to the movement cost
		- [x] can change direction
	- Quality of Life
		- [x] refactor pathfinding logic from hex grid into new class

- **Week 4 (November 9, 2020): xxx**

	- Version 1.0 **..sort this later**
		- [x] remove rotation as movement cost 😔
		- [x] remove direction as a gameplay factor 😔
		- [x] hexpath without A* / build path with just direct mouse selection
		- [x] pieces cannot travel through eachother
		- [x] ally collisions ends moves? or does a piece randomly wait
		- [x] hexcell lights up when mouse is tracing over it
		- [ ] moving pieces kill enemies
		- [ ] single player base/fort 
		- different game modes and solid game manager
			- [ ] turn-based
			- [ ] simultaneous turns
			- [ ] real-time
		- unit variety
			- [ ] Pikeman
			- [ ] Crusader/Horse
			- [ ] Pirate
			- [ ] Wall
		- unit mechancis
			- [ ] horses cannot move onto a square with a Pikeman
			- [ ] crusader has extra movement
			- [ ] walls cannot be killed unless pirate
			- [ ] walls move with priority
			- [ ] walls can be cursed
			- [ ] walls can not be cursed
		- [ ] server client relationship
		- [ ] stable refactoring; code fixes; and comments

	- Version 1.0 Stretch Goals
		- [ ] can pick up an invisible instance of the piece when creating a path
		- [ ] archer
		- [ ] crusader can jump pieces
		- [ ] forts spawn units 
		- [ ] piece model updates

	- Game Play 
		- hex curser
			- [ ] can gray out a section of the path
			- [ ] supports overlapping, curser can go backwards ontop of itself
			- [ ] Bézier implementation
			- [ ] multiple cursers can exist simultaneously and yet still comprehensively
		- move submission
			- [x] moves are stored by units until a button is pressed
			- [ ] pieces on the same cell will abruptly stop eachother's paths
		- travel queue
			- [ ] cells and rotations can be added to a travel queue 1 at a time
			- [ ] left click for selection, right click for execution
			- [ ] hold shift to activate travel queue, otherwise A* is used
			- [ ] unit must remember path until it is allowed to forget 
			- [ ] mouse over cell edge can change a unit's direction in A* mode
	- Quality of Life
		- [ ] comment existing code
		- [ ] refactor existing code
		- [ ] comprehend shader code
	- Other
		- [ ] resume 1 second a day videos
		

### Future Dates & Features
List of future objectives to complete
- **Week 5 (October 26, 2020): Break Week**
	- *add next set of deliverables*
	- finish lima with multiplayer
	- fountain asset
	- movement dust asset
	- fire flies
	- campfire scene
	- blender
	- 3D animation rigging

- **Features**
	- Game Design
		- [ ] cell queue combat flow
		- [ ] hex curser inputs
		- [ ] turns and rounds
		- [ ] economy		
		- [ ] cell queue combat flow
	- Gameplay
		- [ ] launch screen
		- [ ] hot seat
		- [ ] water cells 
		- [ ] variant map shapes
		- [ ] rivers edges
		- [ ] rock edges 
		- [ ] unit abilities
			- [ ] ability UI
			- [ ] left click to execute ability
		- [ ] basic piece combat
			- [ ] combat animation
			- [ ] dust effect
	- Graphics
		- [ ] ability descriptions (like a modal view descriptor seen for card games)
	- Sound
		- [ ] drum for piece movement
	- Quality of Life
		- [ ] parameterize number of edge vertices
		- [ ] pathfinding display debugger is readded
	- Tutorials
		- [ ] Blender
		- [ ] Spine weights
		- [ ] Spine UI assets
		- [ ] Shaders & Materials II
		- [ ] Mirror Multiplayer
	- Other
		- [ ] multiplayer test project
		- [ ] Reddit Page
		- [ ] Instagram
		- [ ] Twitter
		- [ ] Steam page
		- [ ] Discord
		- [ ] website
		- [ ] create a 1 second video collection starting from November 1st
		- [ ] *pending requirements*
