![logo](https://github.com/AndreiMisiukevich/HotReload/blob/master/logo/hotreload-logodesign-colored.png)

# HotReload
Xamarin.Forms XAML hot reload, live reload, live xaml

Sample Video you can find here: https://twitter.com/i/status/1124314311151607809

![Sample GIF](https://github.com/AndreiMisiukevich/HotReload/blob/master/files/gf1.gif?raw=true)


## Setup
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
            HotReloader.Current.Start(this); 
            //optionally you may set device's port / scheme and extension's port for auto discovery
#endif
            MainPage = new NavigationPage(new MainPage());
        }
    }
}
```
**IMPORTANT:** i suggest to NOT use ```[Xaml.XamlCompilation(Xaml.XamlCompilationOptions.Compile)]``` with HotReload. It can cause errors. So, don't enable it for Debug or disable please.

#### Mac

 * Visual Studio for Mac extension is available for downloading here http://addins.monodevelop.com/Project/Index/376 
Or by searching in Visual Studio's extension manager
![alt text](https://github.com/AndreiMisiukevich/HotReload/blob/master/files/mac_extension_manager.png)
 * Header of "HotReload for Xamarin.Forms" extension will appear in Visual Studio's "Tools" dropdown menu.
 "Enable extension" menu item will appear as soon as solution is opened.
 ![alt text](https://github.com/AndreiMisiukevich/HotReload/blob/master/files/mac_extension_menu.png)

##### WINDOWS 

* You may use official HotReload's Visual Studio Extension https://marketplace.visualstudio.com/items?itemName=StanislavBavtovich.hotreloadxamarinforms. Also can be downloaded via extension manager 
 ![alt text](https://github.com/AndreiMisiukevich/HotReload/blob/master/files/win_extension_manager.png)
 NOTE: Restart Visual Studio after installation.
* To make "HotReload for Xamarin.Forms" extension's toolbar visible turn it on in "Tools/Customize" window.
 ![alt text](https://github.com/AndreiMisiukevich/HotReload/blob/master/files/win_extension_enable.png)
 * As soon as solution is opened "Enable extension" button will appear on "Tabs panel".
 ![alt text](https://github.com/AndreiMisiukevich/HotReload/blob/master/files/win_extension_tab.png)
 
##### Other IDE e.g. (like Rider etc.)

* Build observer project yourself (Release mode) and find **exe** file in bin/release folder https://github.com/AndreiMisiukevich/HotReload/tree/master/Observer/Xamarin.Forms.HotReload.Observer and put it in the root folder of your Xamarin.Forms NETSTANDARD/PCL project.
* Start Xamarin.Forms.HotReload.Observer.exe via terminal (for MAC) ```mono Xamarin.Forms.HotReload.Observer.exe``` or via command line (Windows) ```Xamarin.Forms.HotReload.Observer.exe``` etc.
* Optionaly you can set specific folder for observing files (if you didn't put observer.exe to the root folder) and specific device url for sending changes.
```mono Xamarin.Forms.HotReload.Observer.exe p=/Users/yourUser/SpecificFolder/ u=http://192.168.0.3```

### Run your app and start developing with **HotReload**!

0) Your device will be discovered automatically.

1) **IMPORTANT**: 
Make sure you your PC/Mac and device/emulator are in the same local network.

2) If you run several instances of VS, probably, you will have to specify EXTENSION'S port during HotReload setup. When you enable extension, it shows you a port in message box. If port ISN'T **15000**, you should pass actual value to HotReload.

```csharp
HotReloader.Current.Start(this, extensionPort: 15001); // 15001 is actual extenstion port value (from message box).
```

3) If you want to make any initialization of your element after reloading, you should implement **IReloadable** interface. **OnLoaded** will be called each time when element is created (constructor called) AND element's Xaml updated. So, you needn't duplicate code in constructor and in **OnLoaded** method. Just use **OnLoaded** then.

```csharp
public partial class MainPage : ContentPage, IReloadable
{
    public MainPage()
    {
        InitializeComponent();
    }

    public void OnLoaded() // Add logic here
    {
        //
    }
}
```

4) **ViewCell** reloading: before starting app you MUST determine type of root view (e.g. StackLayout). It cannot be changed during app work (I mean, you still can change StackLayout props (e.g. BackgroundColor etc.), but you CANNOT change StackLayout to AbsoluteLayout e.g.). 

```xaml
<?xml version="1.0" encoding="UTF-8"?>
<ViewCell xmlns="http://xamarin.com/schemas/2014/forms" 
          x:Class="Xamarin.Forms.HotReload.Sample.Pages.Cells.ItemCell">
    <StackLayout>
    </StackLayout>
</ViewCell>
```

## Android Emulator
In case `VS Extension` detects `xaml` changes but doesn't update in the emulator, you may need to forward the port to your ip (here is example with **DEVICE** port 8000. DON'T USE EXTENSION'S PORT (usually extension port is 15000)):
```
adb forward tcp:8000 tcp:8000
```

## Collaborators
- [AndreiMisiukevich (Andrei)](https://github.com/AndreiMisiukevich)
- [stanbav (Stanislav)](https://github.com/stanbav)

Are there any questions? 🇧🇾 ***just ask us =)*** 🇧🇾

## License
The MIT License (MIT) see [License file](LICENSE)

## Contribution
Feel free to create issues and PRs 😃
