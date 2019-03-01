# HotReload
Xamarin.Forms XAML hot reload, live reload, live xaml

![Sample GIF](https://github.com/AndreiMisiukevich/HotReload/blob/master/files/gf1.gif?raw=true)


## Setup
* Available on NuGet: [Xamarin.HotReload](http://www.nuget.org/packages/Xamarin.HotReload) [![NuGet](https://img.shields.io/nuget/v/Xamarin.HotReload.svg?label=NuGet)](https://www.nuget.org/packages/Xamarin.HotReload)
* Add nuget package to your Xamarin.Forms **NETSTANDARD**/**PCL** project.
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
#endif
            MainPage = new NavigationPage(new MainPage());
        }
    }
}
```

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

* **IMPORTANT**: 
- Make sure you use proper device IP in Extension. Check application output for more info about device IP.
- Also keep in mind, that your PC/Mac and device/emulator must be in the same local network.

* If you want to initialize your element after reloading, you should implement **IReloadable** interface. **OnLoaded** will be called each time when element is created (constructor called) AND element's Xaml updated. So, you needn't duplicate code in constructor and in **OnLoaded** method. Just use **OnLoaded**.

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

## Android Emulator
In case `VS Extension` detects `xaml` changes but doesn't update in the emulator, you may need to forward the port to your `127.0.0.1`:

- You also need to forward the port to your pc/mac using `adb`
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
