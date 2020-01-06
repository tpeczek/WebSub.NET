# WebSub.NET

WebSub.NET is a set of libraries which aim at providing [WebSub](https://www.w3.org/TR/websub/) (a common mechanism for communication between publishers and subscribers of Web content) support for .NET platform.

## WebSub.NET Subscriber Preview

WebSub.NET Subscriber is currently in preview.

- The WebSub subscriber preview package is available on NuGet and MyGet.
- The ASP.NET WebHooks WebSub subscriber receiver is available on NuGet and MyGet.
- The ASP.NET Core WebHooks WebSub subscriber receiver is available on MyGet.

In order to use packages from MyGget you need to add a NuGet.Config to your app with the following content:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <packageSources>
        <clear />
        <add key="dotnet-core.myget.org" value="https://dotnet.myget.org/F/dotnet-core/api/v3/index.json" />
        <add key="websubdotnet.myget.org" value="https://www.myget.org/F/websubdotnet/api/v3/index.json" />
        <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    </packageSources>
</configuration>
```

## Donating

My blog and open source projects are result of my passion for software development, but they require a fair amount of my personal time. If you got value from any of the content I create, then I would appreciate your support by [buying me a coffee](https://www.buymeacoffee.com/tpeczek).

<a href="https://www.buymeacoffee.com/tpeczek"><img src="https://www.buymeacoffee.com/assets/img/custom_images/black_img.png" style="height: 41px !important;width: 174px !important;box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;-webkit-box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;"  target="_blank"></a>

## Copyright and License

Copyright © 2018 - 2020 Tomasz Pęczek

Licensed under the [MIT License](https://github.com/tpeczek/WebSub.NET/blob/master/LICENSE.md)