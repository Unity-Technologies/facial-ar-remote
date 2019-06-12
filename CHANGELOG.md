# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2019-06-01

### This is the second major release of the package and the first release out of preview.


* Refactored to base on the new AR Foundation package to future-proof the project and access the latest features
* Now in Unity package format, making it easier to add the package to an existing project
* Redesigned UX and workflows for editor tools to make it much more simple and intuitive
* Redesigned UX for remote app
* Refactored code to separate editor tools and remote app
* Recording, playback, and clip baking is now a separate editor window
* Added new features
   * Driving multiple characters with a single source stream
   * Connecting multiple devices to drive multiple characters
* Blendshape mappings are separated from stream settings, meaning that multiple different rigs can be driven with the same stream settings, at the same time
