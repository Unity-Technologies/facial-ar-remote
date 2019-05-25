# Facial AR Remote (Preview)

## About

Facial AR Remote is a tool that allows you to capture blend shape animations directly from an iOS device with TrueDepth support (iPhone X or above) into Unity by use of an app built with the [remote package](https://github.com/Unity-Technologies/com.unity.xr.ar-face-capture-remote).

### Experimental Status

This repository is tested against the latest stable version of Unity and requires the user to build their own iOS app to use as a remote. It is presented on an experimental basis - there is no formal support.

## How To Use/Quick Start Guide  

This repository uses [Git LFS](https://git-lfs.github.com/) so make sure you have LFS installed to get all the files. Unfortunately this means that the large files are also not included in the "Download ZIP" option on Github, and the example head model, among other assets, will be missing.

### Editor Animation Setup

#### Install and Connection Testing

1. Download an install Unity 2018.4 and then open a project with a rig set up with the corrent blendshapes

2. To test your connection to the remote, start by opening `../Examples/Scenes/SlothBlendShapes.scene`.

3. Be sure your device and editor are on the same network. Launch the app on your device and press play in the editor.

4. Set the `Port` number on the device to the same `Port` listed on the `Stream Reader` component of the `Stream Reader` game object.

5. Set the `IP` of the device to one listed in the console debug log.

6. Press `Connect` on the device. If your face is in view you should now see your expressions driving the character on screen.
*Note* You need to be on the same network and you may have to disable any active VPNs and/or disable firewall(s) on the ports you are using. This may be necessary on your computer and/or on the network.
*Note* Our internal setup was using a dedicated wireless router attached to the editor computer or lighting port to ethernet adaptor.


### Known Issues

1. Character Rig Controller does not support Humanoid Avatar for bone animation.

2. Animation Baking does not support Humanoid Avatar for avatar bone animation.

3. Stream source can only connect to a single stream reader.

4. Some network setups cause an issue with DNS lookup for getting IP addresses of the server computer.

Note: History edits were made on 10/29/2018. If you cloned this repository before that date, please rebase before submitting a Pull Request.
