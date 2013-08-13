EndingXNA
=========

XNA conversion of the roguelike puzzle game **http://robotacid.com/flash/ending/**

There are 3 projects in the solution: **Windows**, **Windows Phone** and **XBox 360**. 

I've tried to preserve the structure of the original Action Script project as much as possible during the conversion - obviously code has been changed to conform to C# syntax but there are some stub / wrapper objects that mimic Action Script classes that I utilised to aid conversion and reduce the requirement to rewrite game logic.

Windows
-------
Pretty much all of the features of the original source have been converted. You can use JSON or XML as a file format for the levels.

Windows Phone
-------------
As above, although I think the level editor may need a tweak or two here and there.

XBox 360
--------
Currently limited to a Title Safe viewport for rendering and fixed to Gamepad 1 for input (*minimal* work required to read all pads).
I hooked the mousepointer to the right analogue stick and spoofed LMB with the 'A' button.