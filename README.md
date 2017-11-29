# Download
Latest version [here](https://github.com/dlebansais/PgMoon/releases).
Files to enable support for Google Calendar [here](/Release/x64/GoogleAssemblies.x64.zip).

# PgMoon
Adds a little icon to the taskbar to show the moon phase in [Project: Gorgon](https://projectgorgon.com/) MMORPG.

## Current Moon Phase
The current moon phase is displayed in the main screen, as well as in the tooltip, with the remaining time to next phase.

## Full Moon
The remaining time before full moon is show.

## Calendar
All moon phases, past and future, can be seen in the calendar. To return to the current phase, double-click the calendar, or right-click to see the context menu and select "Display Current".

## Mushrooms
In Project: Gorgon, one can grow mushrooms in boxes, and all mushrooms grow differently depending on the moon phase. For each mushroom type, there are two preferred phase when they grow robustly. The main screen allows one to add the name of a mushroom (or use predefined names) and select two corresponding moon phases. One can also add comments, such as the best substrates to use, the growth time or anything else.

If one of the moon phase is current, its name is in bold characters.

In the top-left corner of the mushroom section, a little key icon is used to lock the list (click it and select "Lock"). The purpose of this feature is to avoid accidental changes, since this list is supposed to be edited only rarely. To unlock, click the list and select "Unlock" in the menu.

## Rahu Boat
In the city of Rahu, at docks, Sheyna offers passage on the boat with three possible destinations, depending on the current moon phase: Serbule, Kur Moutains or Sun Vale.

## Dark Chapel
The Dark Chapel dungeon can be entered by inserting a Gulagra's Key in the appropriate statue in Eltibule. The correct statue varies depending on the phases of the moon. The current one is show as a big black square (others as small white squares). Each square has a tooltip with a short description of where to find it.

## Loading at startup
The app can be configured to load at startup. There are two ways to do it:
* Launch the app as administrator, and select "Load at startup" in the menu.
* Follow instructions in the window that pops up when not running as administrator.
The same procedure can be used to stop loading at startup.
Note that when loaded at startup, the app doesn't run as administrator.

## Google Calendar
The app can be configured to post entries in a calendar. Currently, only Google Calendar is supported. You will need to follow these instructions:
* Download files from [here](/Release/x64/GoogleAssemblies.x64.zip) and copy them alongside the application.
* If you're not the owner of the calendar, make sure you have permission to create events.
* Follow step 1 of [these instructions](https://developers.google.com/google-apps/calendar/quickstart/dotnet/) to obtain your credential file. You don't need to perform other steps, just Step 1, and you don't need Visual Studio.
* In the application menu, select "Share the calendar.."
* Click Browse to select the credential file you have obtained. This should automatically list all calendars you have access to.
* Select the calendar on which to post events.
* In the information group, select what to put in the event description.
* Click Save, this will start posting events for the next 10 days, and more events as time goes.

# Screenshots

![Menu](/Screenshots/Menu.png?raw=true "The app menu")

![Main Screen](/Screenshots/MainScreen.png?raw=true "The app main screen")

# Certification

This program is digitally signed with a [CAcert](https://www.cacert.org/) certificate.
