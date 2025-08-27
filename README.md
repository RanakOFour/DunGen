# Ranak's DunGen

![DunGen in action](https://github.com/user-attachments/assets/ae230344-dd2c-4157-a815-3e6eba32f836)

The DunGen (DUNgeon GENerator) is a tool that aims to take in a list of Unity prefabs and then connect them together like puzzle pieces to create random dungeon layouts for Unity games.

## Installation
To import the package into your project, open the Services/Explore tab, and press the '+' button. Select 'Add package from Git URL' and enter https://github.com/RanakOFour/DunGen.git
<img width="792" height="238" alt="gitURL" src="https://github.com/user-attachments/assets/545f43d8-3be7-4945-9ae8-cf1cb4017c72" />

## How does it work?

First, an empty GameObject needs to exist in your scene with a DunGenerator script attached (as pictured below)
<img width="629" height="432" alt="Screenshot_2025-08-27_10 52 59" src="https://github.com/user-attachments/assets/9f475185-c478-4809-be06-7c524cccc908" />

The Starting Room and Dungeon Parts sections will need to be populated by prefabs with the DunGenRoom script
<img width="1920" height="647" alt="Screenshot_2025-08-27_11 13 36" src="https://github.com/user-attachments/assets/2ff03ab4-c97c-4c91-97e6-a622edccfd2a" />
The 'Bounds' property describes a box shape that should cover the whole of the prefab. The 'center' of the Bounds is it's location on the prefab, and it's 'extents' are the half sizes of each length of the box. A visualisation of the Bounds is drawn on the prefab as a red outline.

Each prefab also needs ConnectionPoints to be assigned.
<img width="1920" height="649" alt="Screenshot_2025-08-27_11 19 30" src="https://github.com/user-attachments/assets/00158c57-0421-4713-ae1a-eac46c2c069a" />
ConnectionPoints should be located where you want the prefab to connect to others i.e. at a door. The ConnectionPoints should also be on the edges of the bounds, and rotated so they face outwards from the prefab. The Bounds in this example do not cover the whole prefab and only extend halfway through the doorframe, as such, the ConnectionPoints are also placed halfway in the doorframe.

To check that a ConnectionPoint is facing outwards, make sure that the tool handle rotation is set to 'local', and that the red arrow points away from the prefab
<img width="172" height="77" alt="ToolRotation" src="https://github.com/user-attachments/assets/658ef5ca-9ed6-43a6-9334-5663ab1566ef" />

As another example, here is a T-Junction prefab with a ConnectionPoint selected in the editor<img width="711" height="563" alt="Screenshot_2025-08-27_11 29 35" src="https://github.com/user-attachments/assets/ca26c904-5b88-48dd-a78e-494b878141a6" />

With the prefabs set up, they can be drag and dropped into the DunGenerator from before. When populating the Dungeon Parts list, it is important to specify how many of each part you want to be placed into the Dungeon.
<img width="723" height="391" alt="Screenshot_2025-08-27_11 32 26" src="https://github.com/user-attachments/assets/6352fdc0-3043-4427-a4aa-c928137ad602" />

With that, all the prep is done and now you just need something to call the Generate() function in the DunGenerator
