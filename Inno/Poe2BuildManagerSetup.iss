[Setup]
AppName=Poe2 Build Manager
AppVersion=0.1.0
DefaultDirName={autopf}\Poe2 Build Manager
DefaultGroupName=Poe2 Build Manager
OutputBaseFilename=Poe2BuildManagerSetup

[Files]
Source: "C:\Users\Kenny\Documents\myApps\Poe2BuildManager\bin\Release\net9.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{group}\PoE2 Build Manager"; Filename: "{app}\Poe2BuildManager.exe"
Name: "{commondesktop}\PoE2 Build Manager"; Filename: "{app}\Poe2BuildManager.exe"