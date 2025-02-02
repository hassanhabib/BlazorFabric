# BlazorFabric
Simple port of Office Fabric React components and style to Blazor

## Demo
https://blazorfabric.azurewebsites.net/

## Release Notes
- v1.3.2
	- Added `Spinner`
	- Added `Persona`
	- Added `Image`
	- Added `Tooltip`
	- Added `Stack` <- *awesome abstraction of flexbox... the Fabric guys don't give this one enough credit*
	- Added `ResponsiveLayout` <- *not a Fabric control, but made so you don't have to use CSS media queries!*
	- Changed the demo's layout to use `ResponsiveLayout` and `Stack`, slowly removing CSS!
- v1.2.6 *(aspnetcore-3.0 out of preview!)*
	- Fixed `Overlay` so that it blocks body scrolling
	- Added `Panel`
	- Added `FocusTrapZone` (and added it to `Modal` to make focus interactions work better)
	- disregard previous fix!  Current guidelines are that devs must manually add js and css files to your html files.  See the sample for a copy/paste opportunity.
- v1.2.2-preview9
	- Fixed new components so they would automatically add their css/js to your index.html in client-side blazor
	  (This is temporary.  You're supposed to add them in manually anyways.  This will stop working in a future version of Blazor)
- v1.2.1-preview9
	- Fixed wrong thread problem with `ContextualMenuItem` (using a timer)
- v1.2.0-preview9
	- Breaking changes: `ContextualMenu` attached to all buttons requires a new way to create menu items.  Use the built-in `ContextualMenuItem` class or create your own with the `IContextualMenuItem` interface.
	- Added: `CommandBar`, `ResizeGroup`, `OverflowSet` and fixed `ContextualMenu` click/dismiss problems.
- v1.1.1-preview9
	- Fixed Button contextmenu icon
- v1.1.0-preview9 
    - Breaking changes: Changed all namespaces to `BlazorFabric`.  No more sub-namespaces using the control's name.

## Status of Controls
- Label -done
- DefaultButton, PrimaryButton, ActionButton, IconButton, CommandButton -working
- Checkbox -done, except for icons
- List -supports `INotifyCollectionChanged`, works!
- TextField -done, except for icons and masking
- Icon, only MS icons 
- Nav -done!
- ContextualMenu - done! (except for Callout positioning bug)
- Callout (part of ContextMenu) -working, not positioning perfectly
- Layer (part of Callout) -done?  only layers at root window right now.
- Dropdown -done? working well, but affected by Callout positioning bug
- Modal -done!, no modeless version
- CommandBar - done!
- ResizeGroup - done!
- OverflowSet - done!
- FocusTrapZone - done!
- Panel - done!
- Spinner - done!
- Tooltip - limited functionality.  will show, but can't interact with it yet, doesn't respond to overflow yet.
- Persona - done?
- Image - done!
- Spinner - done!

## Info
There are no MergeStyles in this port.  It's just each control packaged into its own project so you can limit what gets added to your Blazor project. 

## To use
1. Install NuGet package for the control you want.  _BlazorFabric.*_  (be sure to select preview packages)
2. The Blazor team has been inconsistent with how static files from component libraries are added to projects in the past.  Going forward, you'll need to **add all javascript and CSS assets from the component packages manually**.  You can just copy/paste the section from the test app's index.html.
You can also use my helper VSIX extension: https://marketplace.visualstudio.com/items?itemName=LeeMcPherson.BlazorLibraryAssetHelper&ssr=false#overview
3. Optionally, add Microsoft's assets package to your index.html or \_Hosts.cshtml file.

`<link rel="stylesheet" href="https://static2.sharepointonline.com/files/fabric/office-ui-fabric-core/10.0.0/css/fabric.min.css" />`

(Remember that the assets package has a more restrictive license.  You are required to use it with/for some type of Microsoft product.  However, one of their engineers said that using it hosted on Azure would be enough... but I'm not a lawyer, so use caution.)

4. If you're using any component that requires a `Layer` as part of its inner-workings (i.e. `Modal`, `Callout`, etc... anything that pops up over already drawn stuff), you need to wrap the `Router` with a `LayerHost`.
```
<BlazorFabric.Layer.LayerHost Style="display:flex; flex-direction: row;width:100vw">
    <Router AppAssembly="typeof(Startup).Assembly" />
</BlazorFabric.Layer.LayerHost>
```

