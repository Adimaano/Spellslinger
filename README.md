# ~~SpellSlinger_Prototyping~~ That 1 Wizard Game
Includes some basic setups and experiments for the upcoming VR Project "That 1 Wizard Game"
## Saving Runes
Open the *Scenes/DataCollection/Draw_Prototype_01* scene and hit the play button. 

On startup the necessary VR Software should automatically start (e.g. steamVR for index/vive).

If you draw a rune, the rune is automatically saved to a .txt file. The file can be changed in the Draw.cs script (constant *FILEPATH* at the top of the file).

Everytime a rune is drawn, several things happen:
 - A new GameObject called "Original Rune X" with a LineRenderer is created. "X" is the number of Runes (e.g. if there are already 5 Runes in the .txt file it would be "Original Rune 6").
 - A second GameObject called "Normalized Rune X" with a LineRenderer is created. This LineRenderer draws the rune as it was saved to the file. This can be helpful for debugging.

 After you drew a couple of runes click the *Pause Button* (do not Stop the game only Pause!). With the game stopped switch to the scene tab and click on each "Normalized Rune X". If for example *Normalized Rune 9* looks off (e.g. tracking glitch), you can just remove the 9th line in the file

## Project Structure
### Scripts Folder
Scripts folder is seen as one namespace for the project.

### ThirdParty Folder
Here go all third party assets, plugins, etc.

### Scenes Folder
Should be sorted in subfolders (Rune-Test, World-Test, etc.), except for the main scenes like menu and level.

## Coding Style
To create a consistent coding style, the Analyzer Tool [StyleCopAnalyzers](https://github.com/DotNetAnalyzers/StyleCopAnalyzers) can be used. It can be installed via the NuGet Package Manager.

### Adjusted Rules
This may change as the team makes progress throughout the project. Maybe some preferences change or new rules are added.
 - SA1500: Braces for multi-line statements must not share line -> **Disabled**

Reason/Example:
```csharp
// instead of this
if (true)
{
	// Do something
}

// you can use
if (true) {
	// Do something
}
```

 - SA1310: Field names must not contain underscore -> **Disabled**

Reason/Example:
```csharp
// instead of this
private const int THISISBASICALLYUNREADABLE = 0;

// you can use
private const int THIS_IS_NOW_READABLE = 0;
```

 - SA1202: 'public' members must appear before 'private' members -> **Disabled**

Reason/Example:
There are special functions in Unity that should be at the top of the class regardless of their access modifier (e.g. Awake(), Start(), Update(), ...)
```csharp
private void Awake() {
	// This function is called when the script instance is being loaded.
	// Let's put it at the top of the class for readability!
}
```

 - SA1600: Elements must be documented -> **Suggestion**

Reason/Example:

If an element is named in a way that it is self-explanatory, it does not need to be documented. However, if the name is not self-explanatory, it should be documented.

 - SA1134: Each attribute should be placed on its own line of code -> **Disabled**

Reason/Example:
If there is only one attribute, it is often more readable to put it on the same line as the declaration. Especially if there are multiple variables with the same attribute.
```csharp
// instead of this
[SerializeField] 
private bool isLit = false;
[SerializeField] 
private bool anotherBool = false;
[SerializeField] 
private bool andAnotherOne = false;

// you can use
[SerializeField] private bool isLit = false;
[SerializeField] private bool anotherBool = false;
[SerializeField] private bool andAnotherOne = false;
```

 - SA1602: Enumeration items should be documented -> **Suggestion**

Reason/Example:
If an enumeration item is named in a way that it is self-explanatory, it does not need to be documented. However, if the name is not self-explanatory, it should be documented.

 - SA1516: Elements should be separated by blank line -> **Suggestion**

Reason/Example:
If multiple elements are closely related, it is often more readable to put them next to each other. Especially if there are multiple variables with the same attribute. It is in the responsibility of the programmer to make sure that the code is readable.
```csharp
// no space because the variables are closely related
public System.Action Action1 { get; internal set; }
public System.Action Action2 { get; internal set; }
public System.Action Action3 { get; internal set; }

// space because the variables are not closely related
public void Method1() {
	// ...
}

// space because the variables are not closely related
public void Method2() {
	// ...
}
```
