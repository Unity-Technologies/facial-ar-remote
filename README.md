# Facial AR Remote (Preview)

## About

Facial AR Remote is a tool that allows you to capture blendshape animations directly from a compatible iOS device to the Unity Editor. Download the [Facial AR Remote Integration Project](https://github.com/Unity-Technologies/Facial-AR-Remote-Project) if you want a Unity project with all dependencies built in.

### Experimental Status

This repository is tested against the latest stable version of Unity and requires the user to build the iOS app to use as a remote. It is presented on an experimental basis - there is no formal support.

## Download
Install the package through the Package Manager [using the Git URL](https://docs.unity3d.com/Manual/upm-ui-giturl.html).

## How To Use/Quick Start Guide  

This repository uses [Git LFS](https://git-lfs.github.com/) so make sure you have LFS installed to get all the files. Unfortunately this means that the large files are also not included in the "Download ZIP" option on GitHub, and the example head model, among other assets, will be missing.

### iOS Build Setup

1. Set your build target to iOS
1. In `Project Settings > Player Settings` go to `Other Settings > Camera Usage Description` and type a description of why you are requesting camera access. This will be presented when you first open the app.
1. Set the `Client.scene` as your build scene in the Build Settings and build the Xcode project.

### Editor Animation Setup

#### Install and Connection Testing

1. (Optional) install the Sloth Example from the Package Manager by selecting the ARKit Facial Remote package and installing the sample

1. Be sure your device and editor are on the same network. Launch the app on your device and press play in the editor.

1. Set the `Port` number on the device to the same `Port` listed on the `Stream Reader` component of the `Stream Reader` game object.

1. Set the `IP` of the device to one listed in the console debug log.

1. Press `Connect` on the device. If your face is in view you should now see your expressions driving the character on screen.
*Note* You need to be on the same network and you may have to disable any active VPNs and/or disable firewall(s) on the ports you are using. This may be necessary on your computer and/or on the network.
*Note* Our internal setup was using a dedicated wireless router attached to the editor computer or lighting port to ethernet adaptor.


### Known Issues

1. Character Rig Controller does not support Humanoid Avatar for bone animation.

1. Animation Baking does not support Humanoid Avatar for avatar bone animation.

1. Stream source can only connect to a single stream reader.

1. Some network setups cause an issue with DNS lookup for getting IP addresses of the server computer.
