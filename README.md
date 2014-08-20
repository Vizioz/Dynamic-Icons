Dynamic-Icons - ALPHA RELEASE
=============================
Dynamic Icon generator for ASP.NET Webforms &amp; MVC websites.

The objective of this component is to automatically return all Icon files used by various devices such as Apple-Touch-Icon files, Android Icons, Favicons etc. It will also provide web form and MVC helpers to render all the required META tags.

**This is still in it's early stages and I would not advise anyone to use this in a production environment**

# Current Plans for further development

Add support for:

- Android Icons
- FavIcons
- allow these dynamic icons to be over written when you have a custom icon for a specific file name
- MVC Helpers e.g. @Html.RenderDynamicIconsMetaData()
- Webforms Helpers
- NuGet Packaging

# Setup Required

For this to work you will need to add the following to your Web.config file:

## Key

The following key defines where your source Icon file is, this should be a large version of your logo, say 500x500. The image location is relative to your website root and you can of course put the image where ever you like.

`<add key="dynamicIcon" value="/images/now-logo.png" />`

## HttpHandler

```
<remove name="DynamicIconsHandler" />
<add name="DynamicIconsHandler" preCondition="integratedMode" verb="GET" path="apple-touch*.png" type="DynamicIcons.HttpHandler, DynamicIcons " />
```

