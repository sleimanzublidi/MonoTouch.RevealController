using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Reveal;

namespace Sample
{
    [Register ("AppDelegate")]
    public partial class AppDelegate : UIApplicationDelegate
    {
        UIWindow window;
        RevealController revealViewController;
        MenuViewController menuViewController;
        HomeViewController homeViewController;

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            homeViewController = new HomeViewController();

            menuViewController = new MenuViewController();
            menuViewController.BackgroundView = new UIImageView(UIImage.FromFile("Wallpaper.png"));
            menuViewController.BackgroundView.Frame = UIScreen.MainScreen.Bounds;
            menuViewController.AddItem<HomeViewController>("Home");
            menuViewController.AddItem<UIViewController>("Stuff");
            menuViewController.AddItem<UIViewController>("Settings");

            menuViewController.ResolveController = (label, type) =>
            {
                if (type == typeof(HomeViewController))
                {
                    return homeViewController;
                }
                return new DummyController(label);
            }; 

            revealViewController = new RevealController();
            revealViewController.MenuView = menuViewController;
            revealViewController.CurrentView = homeViewController;

            window = new UIWindow(UIScreen.MainScreen.Bounds);
            window.RootViewController = revealViewController;
            window.MakeKeyAndVisible();

            return true;
        }
    }

    public class DummyController : UIViewController
    {
        private string title;

        public DummyController(string title) 
        {
            this.title = title;
            this.View.Frame = UIScreen.MainScreen.Bounds;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.BackgroundColor = UIColor.White;
            NavigationItem.Title = title;
        }
    }

    public class HomeViewController : DummyController
    {
        public HomeViewController() 
            : base("Home")
        {}
    }
}

