Provides a new style of menu/details navigation control.

Inspired by [this](http://dribbble.com/shots/1114754-Social-Feed-iOS7) post on Dribbble.

```csharp
using MonoTouch.Reveal;
...

RevealController revealViewController;

public override bool FinishedLaunching(UIApplication app, NSDictionary options)
{
    // Instantiate the controller
    revealController = new RevealController();
    
    // Assign the Menu view controller
    // If you don want to create your own menu, you can use the MenuViewController
    // I've included it to help creating the menu with some animations.
	var menu = new MenuViewController();
	menu.AddItem<UIViewController>("Label 1");
	menu.AddItem<UIViewController>("Label 2");
	menu.AddItem<UIViewController>("Label 3");   	
	menu.ResolveController = (label, type) =>
	{
		// Return a UIViewController of the required type
	};
	revealController.MenuView = menu;

    // Assign the Current view controller which is seen first before you touch the menu button.
    revealController.CurrentView = new UIViewController("firstviewnib", null);

    // Assign the controller to be displayed!
    window.RootViewController = revealController;     
}
```

*Some screenshots assembled with [PlaceIt](http://placeit.breezi.com/).*