MonoTouch.RevealController
====================================

Description
-----------

Provides a new style of menu/details navigation control. 

Inspired by [this](http://dribbble.com/shots/1114754-Social-Feed-iOS7) post on Dribbble.

You can see it in action [here](http://youtu.be/_yq3nHdQ190).


Requirements
------------

* Xamarin Monotouch

Usage
-----

    // Instantiate the controller
    var revealController = new RevealController();
    
    // Assign the Menu view controller
    revealController.MenuView = new UIViewController("Menu", null);

    // Assign the Current view controller which is seen first before you touch the menu button.
    revealController.CurrentView = new UIViewController("firstviewnib", null);

    // Assign the controller to be displayed!
    window.RootViewController = revealController;
    

	// I've included a class called MenuViewController to help creating the menu with some animations.

	var menu = new MenuViewController();
	menu.AddItem<UIViewController>("Label 1");
	menu.AddItem<UIViewController>("Label 2");
	menu.AddItem<UIViewController>("Label 3");   	
	menu.ResolveController = (label, type) =>
	{
		// Return a UIViewController of the required type
	};
	revealController.MenuView = menu;

Licensing
---------

Copyright 2013 Sleiman Zublidi

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.