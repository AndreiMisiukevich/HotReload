# HotReload
Xamarin.Forms XAML hot reload, live reload, live xaml

![Sample GIF](https://github.com/AndreiMisiukevich/HotReload/blob/master/files/gf1.gif?raw=true)


## Setup
* Available on NuGet: [Xamarin.HotReload](http://www.nuget.org/packages/Xamarin.HotReload) [![NuGet](https://img.shields.io/nuget/v/Xamarin.HotReload.svg?label=NuGet)](https://www.nuget.org/packages/Xamarin.HotReload)
* Add nuget package to your Xamarin.Forms NETSTANDARD/PCL project.
* Setup App class like
```csharp

#if !DEBUG
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
#endif

namespace Xamarin.Forms.HotReload.Sample
{
    public partial class App : Application
    {
        public App()
        {
#if DEBUG
            HotReloader.Current.Start(); //by default you can pass specific url, if you want to run it on real device
#endif
            InitializeComponent();
            MainPage = new NavigationPage(new MainPage());
        }
    }
}
```
* ALL XAML partial classes (ContentPage, ViewCell etc.) MUST be setup like
```csharp
    using Xamarin.Forms.HotReload.Reloader;

    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
#if DEBUG
            this.InitializeElement();
#else
            InitializeComponent();
#endif
        }
    }
```
HotReloader runs http listener at http://127.0.0.1:8000 (by default) or at you passed url.

* Download fresh version of **observer.exe** https://github.com/AndreiMisiukevich/HotReload/blob/master/files/observer.exe and put it in the root folder of your Xamarin.Forms NETSTANDARD/PCL project.
* Start observer.exe via terminal (for MAC) or via command line (Windows)
``` mono observer.exe ``` FOR MAC
Optionaly you can set specific folder for observing files (if you didn't put observer.exe to the root folder).
```mono observer.exe p=/Users/andrei/SpecificFolder/```
Optionaly you can set specific device url for sending changes
```mono observer.exe u=http://192.168.0.3```
* Run your app and enjoy

Are there any questions? 🇧🇾 ***just ask me =)*** 🇧🇾

## License
The MIT License (MIT) see [License file](LICENSE)

## Contribution
Feel free to create issues and PRs 😃
