Hello! Thank you for the purchase. Hope the assets work well.
If they don't, please contact me slsovest@gmail.com.

___Assembly________________________________________

All the parts may have containers for mounting other parts inside, their names start with "Mount_".
(In some cases they may be deep in bone hierarchy). 
The parts from the "Sides" folder can be mounted to "Mount_Side" container,
"Back_" to "Mount_Back" etc. The best way to get it is just to explore the demo units.
Drop the part in the corresponding container, and It should snap into place.

- Start with "Chassis_" part
- Drop the "Cockpit" or "Back" into their mounting points
- Search for the next mounting points inside the attached parts

Not all the combinations may look very well and plausible (the planes are especially tricky),
you can just start with the demo units and tweak those part by part.


___Scripts________________________________________

"Roll_Wheels.cs" script rolls the wheels according to the speed they move in space.
Should be assigned directly to the wheel geometry.


___Textures________________________________________

The source .PSD can be downloaded here:
https://drive.google.com/file/d/1bx5jMZW5RJ5WcopGUMo4Uvsbr9dBZ2cj/view?usp=sharing

For a quick repaint, adjust the layers in the "COLOR" folders. 
You can drop your decals and textures (camouflage, for example) in the folder as well. Just be careful with texture seams.
You can try using Color_ID and Grayscale textures to create a custom player color shader.


___Animations______________________________________

Some .fbx files contain multiple animations inside:


Fan_Prop:

Idle 			(frames 34-35)
Idle_to_Blurred 	(1-9)
Blurred 		(13-20)loop
Blurred_to_Idle 	(25-34)
Rotate_Blades 		(40-55)
Rotate_Blades_Blurred 	(60-75)


Fan_Quad:

Idle 			(34-35)
Idle_to_Blurred 	(1-9)
Blurred 		(13-20)
Blurred_to_Idle 	(25-34)
Rotate_Blades 		(40-55)


Top_Copter_Blades:

Idle 			(1-2)
Idle_to_Blurred 	(6-24)
Blurred 		(30-41)
Blurred_to_Idle 	(48-65)
Rotate 			(70-81)


Chassis_Plane:

Fold			(1-20)
Unfold			(30-50)


Container_Legs:

Idle			(1-2)
Fold			(5-25)
Idle_Folded		(25-26)
Unfold			(30-50)
Idle_Hanging		(55-115)


Side_Claw:

Pack			(1-13)
Idle_Packed		(13-14)
Unpack			(20-33)
Idle_Unpacked		(40-100)
Pinch			(110-133)
Grab			(140-159)
Ungrab			(160-185)
To_Landing		(190-213)
From_Landing		(220-242)


Weapon_TwinCannon:

Shoot			(10-20)
Shoot_Round		(1-5)


Weapon_QuadCannon:

Shoot			(1-15)
Shoot_Round		(20-30)



Version 2.0 (Sept 2020)
- added PBR textures
- added 96 new parts (mostly for spaceships and transporters)
- added 17 more units to the demo scene


Version 2.1 (Oct 2020)
- added second package (Unity 2017.4.1), which imports well into older Unity versions


Version 2.2 (Aug 2023)
- Equalized the texture brightness across all the Mech Constructor packages

