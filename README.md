# Ranak's DunGen

![DunGen in action](https://github.com/user-attachments/assets/ae230344-dd2c-4157-a815-3e6eba32f836)

The DunGen (DUNgeon GENerator) is a tool that aims to take in a list of Unity prefabs and then connect them together like puzzle pieces to create random dungeon layouts for Unity games.

## How does it work?

To make a dungeon, you will need 3 things:
1. A gameobject with a DungeonGenerator script attached

<img width="341" height="469" alt="DungeonManager" src="https://github.com/user-attachments/assets/afa8cad9-758c-4df4-9971-be3d7dbea5e9" />

2. A set of prefabs (to build the dungeon with) that have a DungeonPart script attached (Squared in red)
3. The prefabs need to have ConnectionPoints placed within them that are facing outside of the prefab (Squared in pink)

### Example DungeonPart Prefab
<img width="1920" height="1080" alt="DungeonPart" src="https://github.com/user-attachments/assets/8fcbf898-4280-4b98-9b33-f90b502a4f02" />

### ConnectionPoint facing outwards
Local view ↓ enabled will show the red arrow facing outwards
![When checking the 'local' view, the red arrow faces away from the prefab](https://github.com/RanakOFour/Unity3DDungeonGenerator/blob/main/READMeAssets/ConnectionPoint.png)
The ConnectionPoint prefabs are only green planes so they can be seen when editing, they are hidden after the dungeon is generated.

With all of this setup, you just need to call the Generate() function from the DungeonGenerator and it should all be fine.
