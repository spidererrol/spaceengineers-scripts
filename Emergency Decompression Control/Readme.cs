/*
 Emergency Decompression Control
 ===============================

 This script will manage doors on your station to minimize the impace of
 decompression events.

 Quick Start
 -----------
 
 You will need a programming block on the same connected grid as you want to manage. (It will
 monitor accross rotors, pistons, and merge blocks but NOT connectors). Load this script into
 the programming block and it will automatically start and output some information to the monitor.

 Your ship or station must be divided into "zones". Each zone must have at least one vent and each
 zone must have doors between it and every zone it is connected to (ie so you can isolate a zone
 with a leak). Each zone must have a unique name made up of only letters, numbers or spaces.

 Adjust the name of each vent to contain "[#" + the zone name + "]" (Eg "[#Bridge]"). You can leave
 any other parts of the name intact (Eg "Air Vent 1 [#Bridge]").

 The each door needs to be similarly labeled with the name of *ALL* the zones it connects. Eg All
 doors betwen "Bridge" and "Upper Corridor" should be labeled with "[#Bridge] [#Upper Corridor]".

 You can also have lights which indicate pressure states for each zone. They should be labeled
 with the zone they are monitoring (Eg "[#Bridge]"). By default they will be turned on when the
 zone is depresurized and off when it is pressurized (set up colors and flashing as you like).

 That is it for a basic functional setup!

 There are also advanced configurations that you can do as detailed below.

 Advanced Configuration
 ----------------------

 You should read the Quick Start before this section as I will only cover additional functionality
 here.

 1. Changing the markers ("[#" and "]").

 If the markers get in the way of other scripts usage or are just to awkward to type, you can
 change them. You do need an opening and a closing marker and they should be selected so not
 to be confused with each other or other text in names.

 In the "Custom Data" for the programming block containing this script, there should be an
 "[EDC]" section (if not, make sure the script has compiled and run at least once). Edit the values
 for "Prefix" and "Suffix" to define new markers.

 Eg for "<%Room%>" instead of "[#Room]":
    [EDC]
    Prefix=<%
    Suffix=%>

 2. Hatch (normally closed door).

 To leave a door closed normally but still close it during decompression if it is open, mark it up
 with room names as usual and then edit "Custom Data" on the door and set "StayClosed" in the "EDC"
 section to "true":
    [EDC]
    StayClosed=true
 
 2b. Hatch (normally closed door) [DEPRECIATED]

 This is a legacy method for the above. It is depreciated as you have to keep all tags in sync and
 it may behave inconsistently if you don't.
 To activate this mode you must add a "!" before the closing tags on the door.
 Eg "[#Bridge!] [#Upper Corridor!]"

 3. Advanced Lighting control.

 You can make a light change colours for each mode or invert the on/off behaviour. This is done
 through a config section in the light's Custom Data.
 The default section is as follows:
    [EDC]
    SealedColor=Off
    LeakColor=On
 To invert operation simply change On to Off and Off to On:
    [EDC]
    SealedColor=On
    LeakColor=Off
 To change colors, specify the color for each mode. Colors are either a name from VRageMath.Color
 or an html-style "#" code.
    [EDC]
    SealedColor=#00ff00
    LeakColor=Red

 Further you can also control the blink mode of each light via the "SealedBlinkLength",
 "SealedBlinkIntervalSeconds", "SealedBlinkOffset", and the "Leak" versions of these. See the 
 "Blink" properties of IMyLightingBlock for the values of each.
    [EDC]
    SealedColor=Lime
    SealedBlinkLength=0.0001
    SealedBlinkIntervalSeconds=300
    SealedBlinkOffset=0.8
    LeakColor=Red
    LeakBlinkLength=0.01
    LeakBlinkIntervalSeconds=2
    LeakBlinkOffset=0

*/