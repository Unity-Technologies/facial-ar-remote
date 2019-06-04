# AR Face Capture

## About

Facial AR Remote is a tool that allows you to capture blend shape animations directly from an iOS device with TrueDepth support (iPhone X or above) into Unity by use of an app built with the [remote package](https://github.com/Unity-Technologies/com.unity.xr.ar-face-capture-remote).

##### Experimental Status

This repository is tested against the latest stable version of Unity and requires the user to build their own iOS app to use as a remote. It is presented on an experimental basis - there is no formal support.

## How To Use/Quick Start Guide  

This repository uses [Git LFS](https://git-lfs.github.com/) so make sure you have LFS installed to get all the files. Unfortunately this means that the large files are also not included in the "Download ZIP" option on Github, and the example head model, among other assets, will be missing.

### Editor Setup

#### Install and Connection Testing

1. Download an install Unity 2018.4

2. Open the AR Face Capture window from `Window > AR Face Capture` or by clicking the `Open AR Face Capture Window` on a `Stream Reader` component on a game object.

3. To test your connection to the remote, start by opening `../Samples/SlothSample/Scenes/SlothBlendShapes.scene` by dragging the scene from the package into the hierarchy and removing the previously loaded scene (currently this is the only way to open scenes from packages).

4. Be sure your device and editor are on the same network. Launch the app on your device and press play in the editor.

5. Set the `Port` number on the device to the same `Port` listed on the `Stream Reader` component of the `Stream Reader` game object.

6. Set the `IP` of the device to one listed in the console debug log.

7. Press `Connect` on the device. If your face is in view you should now see your expressions driving the character on screen.

**Note** You need to be on the same network and you may have to disable any active VPNs and/or disable firewall(s) on the ports you are using. This may be necessary on your computer and/or on the network.

**Warning** Modifying or using a different version of the `Stream Settings` asset (**Stream Settings ARKit2-0**) will cause the remote app from the App Store to no longer function with your package. The `Stream Settings` used by the `Network Stream` component must be the same as the remote app. If you need to change the file, you must build the remote app yourself. See below for building the remote app.

#### Driving multiple character rigs
The StreamReader prefab is only designed for a single device/single character rig setup. To drive multiple characters from a single device or playback stream:

Foreach character rig:
1) Create a new game object and add a `Stream Reader` component
2) On the `Stream Reader`, add the game object containing the `Blend Shape Controller` and `Character Rig Controller` components (the character rig) to the `Character` field
3) Create a new game object and add a `Network Stream` and `Playback Stream` component
4) In the `Stream Reader` component, add the game object containing the Network and Playback Stream components to `Stream Source Overrides` list

**Note** You can place the `Network Stream` and `Playback Stream` components on separate game objects, but then you must add both of those game objects to the `Stream Source Overrides` list

### Building the remote app
To build the remote app yourself, you will need to install the [AR Face Capture Remote](https://github.com/Unity-Technologies/com.unity.xr.ar-face-capture-remote) package. See the readme for instructions.

# How it works
## Networking
The remote is made up of a client/remote iOS app. The client is a lightweight app that’s able to make use of the latest additions to ARKit and send that data over the network to the `Network Stream` source. Using a simple TCP/IP socket and fixed-size byte stream, we send every frame of blendshape, camera and head pose data from the device to the editor. The editor then decodes the stream and to updates one or more rigged characters in real-time. 

## Jitter Reduction
To smooth out some jitter due to network latency, the `Stream Reader` keeps a tunable buffer of historic frames for when the editor inevitably lags behind the phone. We found this to be a crucial feature for preserving a smooth look on the preview character while staying as close as possible the real actor’s current pose. In poor network conditions, the preview will sometimes drop frames to catch up, but **all data is still recorded with the original timestamps from the device.**

## How data is ingested from the remote app
On the editor side, we use the stream data to drive the character for preview as well as baking animation clips. Since we save the raw stream from the phone to disk, we can continue to play back this data on a character as we refine the blend shapes. And since the save data is just a raw stream from the phone, **you can even re-target the motion to different characters**. 

## Baking streamed data to an Animation Clip
Once you have a stream you’re happy with captured, you can bake the stream to an animation clip. This is great since they can use that clip that you have authored like any other animation in Unity to drive a character in Mecanim, Timeline or any of the other ways animation is used. **Note that the animation clip is specific to the particular character rig that was used when baking the clip.**

# Components 
Several key components act as the hub for using the **AR Face Capture** package in the editor. They are responsible for processing the stream data from the stream source(s) to be used by the connected controllers responsible for driving face movement on a character rig.

There is also a `AR Face Capture Window` that is used to control the device connection, and record & playback captured streams to a character. You are also able to “bake” a stream to a character by creating an animation clip that can be used in the animator or timeline.

## Stream Sources

Both the `Network Stream` and `Playback Stream` components are stream sources and responsible for feeding stream data to a `Stream Reader`.

**Important notes:**
* You can switch between recording and playback without needing to stop any of the internal stream updating
* Streams can only be recorded in play mode and are not available for immediate playback. The recorded streams are processed and saved to the playback buffer when exiting play mode.

### Network Stream
A network-based stream source. Sets up a listen server on the given port to which `Client`s connect.

### Playback Stream
Reads tracking data from a `Playback Data` asset and updates a `Stream Reader`.

## Stream Reader

This is the core component responsible for processing the stream data from the stream source(s) to be used by connected controllers, such as the `Blend Shape Controller` and the `Character Rig Controller`.

## Components for Driving Character Animation
### Blend Shapes Controller
Updates blend shape values from the `Stream Reader` to the skinned mesh renderers referenced in this script.

### Character Rig Controller
Applies pose values from the `Stream Reader` to transforms controlling the head, neck, and eyes.

## Scriptable Objects
### Stream Settings
Holds the data needed to process facial data to and from a byte stream. This data needs to match on the server and client side.

### Playback Data
Asset for storing recorded sessions.

*** 
Why are these separate components? The core reason is to make all this more modular and separate out the functions more. Some of the idea was possibly needing wanting to switch network streams easier for implementing multiple devices. In having both the `Network Stream` and `Playback Stream` be providers of the stream data conceptually makes it a little cleaner to know what is going on. You are also able to switch between recording and playback without needing to stop any of the internal stream updating. I also was working on driving multiple characters from the same stream provider. This was to better test a control capture across several characters or versions of the same character all together. Also, the idea is that you could extend just the stream source or stream reader without having to reinvent/extend one master class

### Known Issues

1. `Character Rig Controller` does not support Humanoid Avatar for bone animation.

2. Animation Baking does not support Humanoid Avatar for avatar bone animation.

3. Some network setups cause an issue with DNS lookup for getting IP addresses of the server computer.

Note: History edits were made on 10/29/2018. If you cloned this repository before that date, please rebase before submitting a Pull Request.
