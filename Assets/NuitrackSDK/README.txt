*FIRST OF ALL*
To use Nuitrack on any device (Windows, IOS, Android, Linux, etc.), you need to install the appropriate software https://github.com/3DiVi/nuitrack-sdk/blob/master/doc/Install.md

*QUICKSTART*
Run AllModulesScene. If you see some errors then follow the on-screen instructions. 
If nothing help go to https://github.com/3DiVi/nuitrack-sdk/blob/master/doc/Troubleshooting.md.
If the problem persists and you decide to contact support, attach the Unity Editor.log from %LOCALAPPDATA%/Unity/Editor

*Prefabs*
In NuitracSDK you can find much useful prefabs. They are sorted by functionality and are located in the corresponding folders

*Nuitrack Scripts Prefab*
It's a base prefab. He must be on stage at the time of using Nuitrack.
Tip:If your project has several scenes, it is recommended to place it at the very beginning on a separate scene. 
This is a DontDestroyOnLoad-prefab, and therefore it will be possible to use one instance of this prefab, it will move between scenes

Modules (https://github.com/3DiVi/nuitrack-sdk/blob/master/doc/Architecture.md):
- Depth - data from depth frame
- Color - data from RGB frame
- User Tracker - Finds users in the frame. User Mask
- Skeleton Tracker - tracks skeletons. Can track 19 joint on 6 users
- Gestures Recognizer - Tracks gestures made with your hands. Waving, Swipe Left, Swipe Right, Swipe Up, Swipe Down, Push
- Hands Tracker - Tracks the hands in some virtual area in front of the person. You can use it to control some user interface

Options:
- WiFi connect - You can receive data via Wi-Fi from TVico and VicoVR (No longer available). At the moment, only the skeleton data is available (https://github.com/3DiVi/nuitrack-sdk/blob/master/doc/TVico_User_Guide.md#setting-up-tvico)
- Run in background - There is no stopping Nuitrack when the window\application is minimized
- Async init - Nuitrack starts in a separate thread. This removes a slight hang when starting work. But then you need to make sure that no scripts will try to get data from Nuitrack before initializing Nuitrack.
Tip: You can disable the necessary prefab, and enable it after initialization by placing it in the Init Event field (you can see an example on the AllModulesScene scene)
- Init Event - The event that is called after Nuitrack is initialized. You can put here any actions that should be called after initialization
- use file record - Insert this path of your *.oni or *.bag file and you can use the recording instead of the sensor (~3 min). If you want to stop using it, uncheck the box. (You may also need to restart the editor)

[Sets values in Nuitrack\nuitrack\nuitrack\data\nuitrack.config so that you don't have to manually edit this file. 
If you have previously edited the values in nuitrack.config, they will have a higher priority than the checkboxes on the prefab. (it is recommended not to edit nuitrack.config at all)]
Config parameters:
- Depth 2 Color Registration - Aligns the data from the RGB frame and the depth frame. Accuracy increases, but performance on weak devices may also decrease.
- Use Face Tracking - Enables face tracking. Face rectangle, Emotions, Age, Gender, Years. (Works only with the skeleton!)
- Nuitrack AI - Enables Nuitrack Ai algoritms. Skeleton and Object trackings. (PC ONLY!) More: https://github.com/3DiVi/nuitrack-sdk/blob/master/doc/Nuitrack_AI.md
- Mirror - Mirrors sensor data (rgb, depth, skeleton, etc.)

*First scene*
1. Open new scene
2. Click "Prepare the scene" in the Nuitrack menu item in the menu bar at the top
3. Drag to the Scene "NuitrackSDK\Frame\Prefabs\Sensor Frame Canvas"
4. Run the scene

*Tutorials*
To open the list of tutorials in the main menu, click: Nuitrack -> Help -> Open tutorials list

*Troubleshooting*
Q:After unpacking, the editor interface broke. 
A:Restart the editor everything should be fixed

Q:Where can I view tutorials
A:Check the Nuitrack menu item. There are a lot of interesting things there.