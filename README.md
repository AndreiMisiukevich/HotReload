![logo](https://github.com/AndreiMisiukevich/HotReload/blob/master/logo/hotreload-logodesign-colored.png)

# HotReload
[![Twitter URL](https://img.shields.io/twitter/url/https/github.com/Andrik_Just4Fun/notification-feed.svg?style=social)](https://twitter.com/intent/tweet?url=https%3A%2F%2Fgithub.com%2FAndreiMisiukevich%2FHotReload&text=%23XamarinForms%20%23HotReload%0A%0AXamarin.Forms%20XAML%2FCSS%20hot%20reload%20%2F%20live%20reload)
 Xamarin.Forms XAML/CSS hot reload / live reload [![Build status](https://dev.azure.com/andreimisiukevich/HotReload/_apis/build/status/HotReload-.NET%20Desktop-CI)](https://dev.azure.com/andreimisiukevich/HotReload/_build/latest?definitionId=2)
Sample Video you can find here: https://twitter.com/i/status/1124314311151607809

![Sample GIF](https://github.com/AndreiMisiukevich/HotReload/blob/master/files/gf1.gif?raw=true)

# Setup

## Extension

### Visual Studio for **MAC**

* Visual Studio for Mac extension is available for downloading here http://addins.monodevelop.com/Project/Index/376 
Or by searching in Visual Studio's extension manager
![alt text](https://github.com/AndreiMisiukevich/HotReload/blob/master/files/mac_extension_manager.png)
* Header of "HotReload for Xamarin.Forms" extension will appear in Visual Studio's "Tools" dropdown menu.
 "Enable extension" menu item will appear as soon as solution is opened.
 ![alt text](https://github.com/AndreiMisiukevich/HotReload/blob/master/files/mac_extension_menu.png)

NOTE: Restart Visual Studio after installation (in case of menu didn't appear in toolbar).

### Visual Studio for **WINDOWS**

* You may use official HotReload's Visual Studio Extension https://marketplace.visualstudio.com/items?itemName=StanislavBavtovich.hotreloadxamarinforms. Also can be downloaded via extension manager 
 ![alt text](https://github.com/AndreiMisiukevich/HotReload/blob/master/files/win_extension_manager.png)
 
NOTE: Restart Visual Studio after installation.
* To make "HotReload for Xamarin.Forms" extension's toolbar visible turn it on in "Tools/Customize" window.
 ![alt text](https://github.com/AndreiMisiukevich/HotReload/blob/master/files/win_extension_enable.png)
 * As soon as solution is opened "Enable extension" button will appear on "Tabs panel".
 ![alt text](https://github.com/AndreiMisiukevich/HotReload/blob/master/files/win_extension_tab.png)
 
### JetBrains **Rider**

* In case if you use JetBrains Rider IDE you can download official HotReload's Rider plugin via setting menu (File -> Settings -> Plugins (pick "Marketplace" tab) -> search by "HotReload".
![alt text](https://github.com/AndreiMisiukevich/HotReload/blob/master/files/rider_extension_manager.png)
All previous versions are available via link: https://plugins.jetbrains.com/plugin/12534-hot-reload
* After Rider restart "Enable extension" button will appear on Navigation Bar.
![alt text](https://github.com/AndreiMisiukevich/HotReload/blob/master/files/rider_extension_tab.png)

### Other IDE/TextEditors (eg. NotePad++ | Sublime | VS Code etc.)

* Build observer project yourself (Release mode) and find **exe** file in bin/release folder https://github.com/AndreiMisiukevich/HotReload/tree/master/Observer/Xamarin.Forms.HotReload.Observer and put it in the root folder of your Xamarin.Forms NETSTANDARD/PCL project.
* Start Xamarin.Forms.HotReload.Observer.exe via terminal (for MAC) ```mono Xamarin.Forms.HotReload.Observer.exe``` or via command line (Windows) ```Xamarin.Forms.HotReload.Observer.exe``` etc.
* Optionaly you can set specific folder for observing files (if you didn't put observer.exe to the root folder) and specific device url for sending changes.
```mono Xamarin.Forms.HotReload.Observer.exe p=/Users/yourUser/SpecificFolder/ u=http://192.168.0.3```

## Reloader

### Base Setup

* Available on NuGet: [Xamarin.HotReload](http://www.nuget.org/packages/Xamarin.HotReload) [![NuGet](https://img.shields.io/nuget/v/Xamarin.HotReload.svg?label=NuGet)](https://www.nuget.org/packages/Xamarin.HotReload)
* Add nuget package to your Xamarin.Forms **NETSTANDARD**/**PCL** project and to all platform-specific projects **iOS**, **Android** etc. just in case (but adding to portable project should be enough)
* Setup Reloader
```csharp
using Xamarin.Forms;

namespace YourNamespace
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
#if DEBUG
            HotReloader.Current.Run(this); 
#endif
            MainPage = new NavigationPage(new MainPage());
        }
    }
}
```
**IMPORTANT:** i suggest to NOT use ```[Xaml.XamlCompilation(Xaml.XamlCompilationOptions.Compile)]``` with HotReload. It can cause errors. So, don't enable it for Debug or disable.

### C# Elements reloading
* **AVAILABLE ON ANDROID AND IOS SIMULATORS**.

* First of All add ```--enable-repl``` to iOS additional mtouch arguments for iPhone Simulator:

![alt text](https://github.com/AndreiMisiukevich/HotReload/blob/master/files/enable_repl.png)

```
**NOTE**: BindingContext will be copied automaticaly, but if your view constructor has any parameters, you will have to implement an interface - **ICsharpRestorable**:
```csharp
[HotReloader.CSharpVisual]
public class CodeContentPage : ContentPage
{
    private Color _backColor;

    public CodeContentPage(Color backColor)
    {
        _backColor = backColor;
        BackgroundColor = backColor;
    }

    public object[] ConstructorRestoringParameters => new object[] { _backColor }; //These arguments will be passed in case of reloading
}
```

* ViewModels (BindingContext) can be updated as well. No need to to mark them with any attribute.

### Additional Setup / Troubleshooting

0) If you want to disable HotReload for Release mode, follow instructions here https://github.com/AndreiMisiukevich/HotReload/issues/115#issuecomment-522475773

1) Your device/simulator/emulator will be discovered automatically. (**IMPORTANT**: 
Make sure your PC/Mac and device/emulator are in the same local network.)

2) If your device and PC/Laptop are in different subnets (or extension doesn't discover device), you should specify your Extension's IP during reloader setup

```csharp
HotReloader.Current.Run(this, new HotReloader.Configuration
{
    //optionaly you may specify EXTENSION's IP (ExtensionIpAddress) 
    //in case your PC/laptop and device are in different subnets
    //e.g. Laptop - 10.10.102.16 AND Device - 10.10.9.12
    ExtensionIpAddress = IPAddress.Parse("10.10.102.16") // use your PC/Laptop IP
});
```

3) Android Emulator IP autodiscovery:
**Windows:** Make sure that **adb** (usually located in C:\Program Files (x86)\Android\android-sdk\platform-tools) is added to PATH enviromnet variable in other case you will have to forward ports yourself. It it isn't added. Add it then restart visual studio or Rider

**BY DEFAULT EXTENSION TRIES TO FORWARD PORTS ITSELF (and you should skip this step) BUT** in case it is not working you should forward the port yourself (here is example with **DEVICE** port 8000 (*DeviceUrlPort* default value).

```
adb forward tcp:8000 tcp:8000
```

**keep in mind** that HotReload may change your DEVICE's port (it's edge case and shouldn't happen, but just keep in mind that it's possible).
So if *adb forward* doesn't help, open **APPLICATION OUTPUT** and look for ```$"### HOTRELOAD STARTED ON DEVICE's PORT: {devicePort} ###"```
And execute *adb forward*  with that value. Also you can get selected port and device IP's there:

```csharp
var info = HotReloader.Current.Run();
var port = info.SelectedDevicePort;
var addresses = info.IPAddresses;
```

4) If you want to make any initialization of your element after reloading, you should implement **IReloadable** interface. **OnLoaded** will be called each time when element is created (constructor called) AND element's Xaml updated. So, you needn't duplicate code in constructor and in **OnLoaded** method. Just use **OnLoaded** then.

```csharp
public partial class MainPage : ContentPage, IReloadable
{
    public MainPage()
    {
        InitializeComponent();
    }

    public void OnLoaded() // Add logic here
    {
        //label.Text = "I'm loaded again";
    }
}
```

5) **ViewCell** reloading: before starting app you **MUST** determine type of root view (e.g. StackLayout). It cannot be changed during app work (I mean, you still can change StackLayout props (e.g. BackgroundColor etc.), but you CANNOT change StackLayout to AbsoluteLayout e.g.). 

```xaml
<?xml version="1.0" encoding="UTF-8"?>
<ViewCell xmlns="http://xamarin.com/schemas/2014/forms" 
          x:Class="Xamarin.Forms.HotReload.Sample.Pages.Cells.ItemCell">
    <StackLayout>
    </StackLayout>
</ViewCell>
```

6) **Previewer** properties: if you want to use ```xmlns:d="http://xamarin.com/schemas/2014/forms/design"``` during your work with HotReload, you can achieve it by two approaches
- Global setting. Manage previewer propertis use via **Configuration**. Design properties will be used by default unless you disable them via Xaml.

```csharp
HotReloader.Current.Run(this, new HotReloader.Configuration
{
    PreviewerDefaultMode = HotReloader.PreviewerMode.On
});
```

- Local setting. Manage previewer propertis use via **XAML**. You can override default behavior for particular file with following markup:

```xaml
<?xml version="1.0" encoding="UTF-8"?>
<?hotReload preview.on?>
<ContentPage>
...
</ContentPage>
```

Use ```<?hotReload preview.on?>``` to allow design properties and ```<?hotReload preview.off?>``` to forbid them.

# Old Extensions (with mannual IP entering)
If you wish to enter device's IP mannualy, you may use these extensions (Make sure you disabled extensions autoupdate)
https://github.com/AndreiMisiukevich/HotReload/tree/master/old_extension_packages

# INFO

## Collaborators
- [AndreiMisiukevich (Andrei)](https://github.com/AndreiMisiukevich) [![Twitter URL](https://img.shields.io/twitter/url/https/twitter.com/Andrik_Just4Fun.svg?style=social&label=Follow%20%40Andrik_Just4Fun)](https://twitter.com/Andrik_Just4Fun)
- [stanbav (Stanislav)](https://github.com/stanbav) [![Twitter URL](https://img.shields.io/twitter/url/https/twitter.com/stasbavtovich.svg?style=social&label=Follow%20%40stasbavtovich)](https://twitter.com/stasbavtovich)
- [iBavtovich (Ignat)](https://github.com/iBavtovich) [![Twitter URL](https://img.shields.io/twitter/url/https/twitter.com/iBavtovich.svg?style=social&label=Follow%20%40iBavtovich)](https://twitter.com/iBavtovich)

Are there any questions? 🇧🇾 ***just ask us =)*** 🇧🇾

## License
The MIT License (MIT) see [License file](LICENSE)

## Contribution
Feel free to create issues and PRs 😃
