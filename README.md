# SettingsBox
A library used to interact with WorldBox's in-game settings.

### How do I use SettingsBox?

To create a setting you must first define a Setting variable and initialize it with the method bindSetting from SettingsManager. This will allow you to customize the name, description, default value, etc. See Example Below:

```csharp
private static Setting settingExample;

void Awake() {
    settingFireRain = SettingsManager.bindSetting("My Setting Name", // Setting name
        "My Setting Description", // Setting description 
        false, // Default Value (can be bool or int)
        "ui/Icons/iconGrenade"); // Icon path
}
```

The value of the setting is the setting.Value property. Note: you must cast the type (bool, int) and you can set this value to change it in game. See Example Below:

```csharp
private static Setting settingExample;

void Update() {
    if ((bool)settingExample.Value) { // Check the value of the setting
        settingExample.Value = false; // Set the value of the setting
    }
}
```

### Bind Setting
SettingsManager.bindSetting has two headers one for a toggle bool and another for an int slider. Each has optional parameters which includes an action that is run whenever the setting is changed.

```
public static Setting bindSetting(string pName, string pDescription, bool pDefaultValue, string pIcon = null, Action<bool> pAction = null)
```
```
public static Setting bindSetting(string pName, string pDescription, int pDefaultValue, int pMax = 100, int pMin = 0, bool pPercent = false, string pIcon = null, Action<int> pAction = null)
````

For more examples and use cases see the [Example Mod](https://github.com/ApexNite/SettingsExample)
