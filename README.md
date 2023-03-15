# ~~SpellSlinger_Prototyping~~ That 1 Wizard Game
Includes some basic setups and experiments for the upcoming VR Project "That 1 Wizard Game"
## Saving Runes
Open the *Draw_Prototype_01* scene and hit the play button. 

On startup the necessary VR Software should automatically start (e.g. steamVR for index/vive).

If you draw a rune, the rune is automatically saved to a .txt file. The file can be changed in the Draw.cs script (constant *FILEPATH* at the top of the file).

Everytime a rune is drawn, several things happen:
 - A new GameObject called "Original Rune X" with a LineRenderer is created. "X" is the number of Runes (e.g. if there are already 5 Runes in the .txt file it would be "Original Rune 6").
 - A second GameObject called "Normalized Rune X" with a LineRenderer is created. This LineRenderer draws the rune as it was saved to the file. This can be helpful for debugging.

 After you drew a couple of runes click the *Pause Button* (do not Stop the game only Pause!). With the game stopped switch to the scene tab and click on each "Normalized Rune X". If for example *Normalized Rune 9* looks off (e.g. tracking glitch), you can just remove the 9th line in the file