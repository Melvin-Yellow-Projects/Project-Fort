# Project: Fort
## A Chess-Like Strategy Game

![3D Piece Models](/Images/piece_models.png)

## Description
A stoic, chess-like strategy game where multiple players compete against one another to control forts and eliminate opposing forces

![Piece Silhouette](/Images/piece_silhouette.jpg)

## Rules

### General *(add more description)*
* Game is split up into an Economy Phase and a Player Phase
* Economy Phase
	* At the start of every round, a player is able to buy pieces
	* A player may purchase and sell pieces
* Player Phase
	* After the Economy Phase, a player will have 3 Turns to execute their plans

### Pieces
* **Axe**
* **Bow**
* **Horse**
* **Pike**
* **Wall**

### Lose Conditions
* have no remaining Forts at the end of a turn
* have no remaining Pieces at the end of a turn

## Current Version Builds (*v1.1-alpha*)
To play the current build of the game, please download the following **.zip** file to your corresponding operating system from **Dropbox**. **Do not change any of the contents of the folder** to run the game.
- [Windows Build Download (*tested on Windows 10*)](https://www.dropbox.com/s/tzstjnnlc2ulo8l/Windows.zip?dl=0 "Windows.zip download")
- ~~[macOS Build Download (*test failed on macOS Mojave 10.14.6; Steamworks cannot connect to Steam*)](https://www.dropbox.com/s/8f2y0f5ssqm869s/macOs.zip?dl=0 "macOs.zip download")~~

## How to play online through Steam
### 1. Download the .zip file of the game from Dropbox
![Dropbox Download](/Images/1_dropbox.gif)
### 2. Extract the contents of the .zip file to your machine
![Unzipping File](/Images/2_unzip.gif)
### 3. Add the non-Steam game to your Steam Library (this will look slightly different on Windows)
![Adding Game to Steam](/Images/3_steam.gif)
### 4. Launch the game through Steam
### 5. Once it is open, press the Online Game button; in the next menu the Join Game button will be non-interactable *(this is not a bug)*
### 6. Return to Steam to connect with a friend by right clicking on their name, and pressing Join Game (alternatively, you can accept their invite)
![Joining a friend's lobby](/Images/4_join.gif)
### 7. The screen will turn black for a few seconds while you are connecting, in a moment you will be in the same lobby with your friend!

## Known Bugs
List of known bugs in the current project

* Steam avatar picture will fail to load when a player has joined multiple lobbies 
* Movement Displays might not hide when a player loses
* Steamworks cannot connect to Steam for the macOS build exclusively
* Game Settings UI can overlap with player tags in the lobby menu
* *(maybe fixed)* End turn button can untoggle after a player has pressed it; however, it only changes the text, it doesn't actually disable a player's turn end

## Tags
**#strategy #chess #turn-based #RTS #board-game #medieval**