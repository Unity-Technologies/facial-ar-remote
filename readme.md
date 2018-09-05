# Facial AR Remote (Preview)

## About

Facial AR Remote is a tool that allows you to capture blendshape animations directly from your iPhone X into Unity 3d by use of an app on your phone.

### Experimental Status

This repository is tested against the latest stable version of Unity and requires the user to build their own iOS app to use as a remote. It is presented on an experimental basis - there is no formal support.

## How To Use/Quick Start Guide  

Project built using Unity 2018+, [TextMesh Pro Package Manager](https://docs.unity3d.com/Packages/com.unity.textmeshpro@1.2/manual/index.html), and [ARKit plugin](https://assetstore.unity.com/packages/essentials/tutorial-projects/unity-arkit-plugin-92515). 
*Note* ARKit plugin is only required for iOS build of remote. For your convenience, you may want to build the remote from a separate project. For best results use Bitbucket tip of [ARKit plugin](https://bitbucket.org/Unity-Technologies/unity-arkit-plugin)

This repository uses [Git LFS](https://git-lfs.github.com/) so make sure you have LFS installed to get all the files. Unfortunately this means that the large files are also not included in the "Download ZIP" option on Github, and the example head model, among other assets, will be missing.

### iOS Build Setup

1. Setup a new project either from the ARKit plugin project from BitBucket or a new project with the ARKit plugin from the asset store.

2. (Unity 2018.1) Add `TextMesh-Pro` to the project from `Window > Package Manager`. The package is added automatically in Unity 2018.2 and above.

3. Add this repo to the project and set the build target to iOS.

4. Setup the iOS build settings for the remote. In `Other Settings > Camera Usage Description` be sure you add "AR Face Tracking" or something to that effect to the field. 
*Note* You may need to set the `Target Minimum iOS Version` to `11.3` or higher. You may also need to enable `Requires ARKit Support`
*Note* To use ARkit 2.0 you will need to set `ARKIT_2_0` in `Other Settings > Scripting Define Symbols*` this will be required for any platform you want to use ARKit 2.0 features with.

5. Open `Client.scene` and on the `Client` gameobject, set the correct `Stream Settings` on the `Client` component for your version of ARKit.

6. When prompted, import TMP Essential Resources for TextMesh Pro

7. Enable "ARKit Uses Facetracking" on UnityARKitPlugin > Resources > UnityARKitPlugIn > ARKitSettings

8. Set `Client.scene` as your build scene and build the Xcode project.

### Editor Animation Setup

#### Install and Connection Testing

1. Add `TextMesh-Pro` to your main project or new project from `Window > Package Manager`.

2. Add this repo to the project.
*Note* You should not need the ARKit plugin to capture animation.

3. To test your connection to the remote, start by opening `../Examples/Scenes/SlothBlendShapes.scene`.

4. Be sure your device and editor are on the same network. Launch the app on your device and press play in the editor.

5. Set the `Port` number on the device to the same `Port` listed on the `Stream Reader` component of the `Stream Reader` game object.

6. Set the `IP` of the device to one listed in the console debug log.

7. Press `Connect` on the device. If your face is in view you should now see your expressions driving the character on screen.
*Note* You need to be on the same network and you may have to disable any active VPNs and/or disable firewall(s) on the ports you are using. This may be necessary on your computer and/or on the network.
*Note* Our internal setup was using a dedicated wireless router attached to the editor computer or lighting port to ethernet adaptor.


### Known Issues

1. Character Rig Controller does not support Humanoid Avatar for bone animation.

2. Animation Baking does not support Humanoid Avatar for avatar bone animation.

3. Stream source can only connect to a single stream reader.

4. Some network setups cause an issue with DNS lookup for getting IP addresses of the server computer.
