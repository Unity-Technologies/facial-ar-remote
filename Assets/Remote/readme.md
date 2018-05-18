#How to use

NOTE: Do not build the remote app from this (HD Render Loop) project! We include the client code, but the scene is broken.

1) Install the app on the phone via XCode (we can look into testflight if we need it on more phones)
2) Get the PC and the phone on the same network. Make sure this is one that allows nodes to listen on an arbitrary port (default: 9000). You can change the port if you need to in the phone app and in the editor. The best approach is to use a wifi router dedicated exclusively to the demo. Plug the desktop into port 1 (not the WAN port) of the router, and connect the phone to the router’s 5GHz SSID
3) Turn off Windows Defender Firewall on the PC. If this is too risky, allow the Unity Editor to listen on port 9000 or some other port, but just turning the whole thing off is easiest/best
4) Open the AR Demo project in Unity 2017.3.0f3
5) Open the scene Assets/EditorDemo/EditorDemo.unity
7) (optional) Adjust the port number on the Server object if needed.
8) Press Play in the editor. Note the ip addresses listed in the console. There may be a bunch due to virtual adapters. Ones like with `192.168` or `10.0`
9) Open the WindupDemo app on an iPhoneX. It should start tracking faces immediately to show you it’s working.
10) (optional) Adjust the port number to match the server. Should stay at 9000 in most cases
11) Enter the ip address you picked from the console.
12) Tap Connect. If it is gray (not black) the IP address has formatting errors
13) To reconnect, stop and start play mode in the editor, restart the app on the phone, and repeat steps 12 and 13

How to restart the iphone app: https://www.tomsguide.com/us/how-to-close-apps-iphone-x,review-4821.html
