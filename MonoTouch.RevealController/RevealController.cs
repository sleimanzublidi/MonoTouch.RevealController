using System;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;

namespace MonoTouch.Reveal
{
    public class RevealController : UIViewController
    {
        #region Fields

        private readonly ProxyNavigationController internalMenuView;

        private UINavigationController internalTopNavigation;
        private readonly UIViewController internalTopView;

        private UIViewController externalContentView;
        private UIViewController externalMenuView;
        private readonly UITapGestureRecognizer tapGesture;

        private UIImageView snapshot;
        private UIImageView snapshotTrail;
        private bool isVisible;
        private SizeF originalSize;
        private bool showTrail = false;

        #endregion

        public RevealController()
        {
            AnimationSpeed = 0.2f;
            HideStatusBar = true;

            internalMenuView = new ProxyNavigationController
            {
                ParentController = this,
                View = { AutoresizingMask = UIViewAutoresizing.FlexibleHeight }
            };

            internalTopView = new UIViewController { View = { UserInteractionEnabled = true } };
            internalTopView.View.Layer.MasksToBounds = false;

            tapGesture = new UITapGestureRecognizer();
            tapGesture.AddTarget(Hide);
            tapGesture.NumberOfTapsRequired = 1;
        }

        #region Properties

        private bool isMenuEnabled = true;
        /// <summary>
        /// Gets or sets a value indicating whether this menu enabled.
        /// If this is true then you can reach the menu. If false then all hooks to get to the menu view will be disabled.
        /// This is only necessary when you don't want the user to get to the menu.
        /// </summary>
        public bool MenuEnabled
        {
            get { return isMenuEnabled; }
            set
            {
                if (value == isMenuEnabled) return;
                if (!value) Hide();
                if (internalTopNavigation != null && internalTopNavigation.ViewControllers.Length > 0)
                {
                    var view = internalTopNavigation.ViewControllers[0];
                    view.NavigationItem.LeftBarButtonItem = value ? CreateMenuButton() : null;
                }
                isMenuEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets the menu view.
        /// </summary>
        public UIViewController MenuView
        {
            get { return externalMenuView; }
            set
            {
                if (externalMenuView == value) return;
                internalMenuView.SetController(value);
                externalMenuView = value;
            }
        }

        /// <summary>
        /// Gets or sets the current view.
        /// </summary>
        public UIViewController CurrentView
        {
            get { return externalContentView; }
            set
            {
                if (externalContentView == value) return;
                SelectView(value);
            }
        }

        /// <summary>
        /// Gets or sets the animation speed.
        /// </summary>
        public float AnimationSpeed { get; set; }

        
        public bool HideStatusBar { get; set; }

        #endregion  

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            //Add the Menu View
            internalMenuView.View.Frame = new RectangleF(0, 0, View.Frame.Width, View.Frame.Height);
            AddChildViewController(internalMenuView);
            View.AddSubview(internalMenuView.View);

            //Add the Top view
            internalTopView.View.Frame = new RectangleF(0, 0, View.Frame.Width, View.Frame.Height);
            AddChildViewController(internalTopView);
            View.AddSubview(internalTopView.View);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            if (NavigationController != null)
            {
                NavigationController.SetNavigationBarHidden(true, true);
            }                
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (NavigationController != null)
            {
                NavigationController.SetNavigationBarHidden(false, true);
            }                
        }

        public virtual void WillShowMenu(bool animated)
        {
            if (internalMenuView != null)
                internalMenuView.ViewWillAppear(animated);
        }

        public virtual void WillHideMenu(bool animated)
        {
            if (internalMenuView != null)
                internalMenuView.ViewWillDisappear(animated);
        }

        /// <summary>
        /// Creates the menu button.
        /// </summary>
        protected virtual UIBarButtonItem CreateMenuButton()
        {
            return new UIBarButtonItem("Menu", UIBarButtonItemStyle.Plain, (s, e) => Show());
        }

        /// <summary>
        /// Selects the view.
        /// </summary>
        public void SelectView(UIViewController view)
        {
            if (internalTopNavigation != null)
            {
                internalTopNavigation.RemoveFromParentViewController();
                internalTopNavigation.View.RemoveFromSuperview();
                internalTopNavigation.Dispose();
            }

            internalTopNavigation = new UINavigationController(view)
            {
                View =
                {
                    Frame = new RectangleF(0, 0, internalTopView.View.Frame.Width, internalTopView.View.Frame.Height)
                }
            };
            internalTopView.AddChildViewController(internalTopNavigation);
            internalTopView.View.AddSubview(internalTopNavigation.View);

            if (MenuEnabled)
            {
                view.NavigationItem.LeftBarButtonItem = CreateMenuButton();
            }                

            externalContentView = view;
            Hide();
        }

        /// <summary>
        /// Show Menu.
        /// </summary>
        public void Show()
        {
            if (isVisible) return;
            isVisible = true;

            WillShowMenu(true);

            if (HideStatusBar)
                UIApplication.SharedApplication.SetStatusBarHidden(true, false);

            originalSize = internalTopView.View.Frame.Size;

            SizeF size = new SizeF(originalSize.Width / 2f, originalSize.Height / 2f);
            float x = (View.Frame.Width - (size.Width / 2));
            float y = (View.Frame.Height - size.Height) / 2f;

            if (showTrail)
            {
                snapshotTrail = new UIImageView(TakeSnapshot(internalTopView.View));
                snapshotTrail.Frame = new RectangleF(0, 0, originalSize.Width, originalSize.Height);
                snapshotTrail.Alpha = 0.2f;
                View.AddSubview(snapshotTrail);
            }

            snapshot = new UIImageView(TakeSnapshot(internalTopView.View));
            snapshot.Frame = new RectangleF(0, 0, originalSize.Width, originalSize.Height);
            snapshot.UserInteractionEnabled = true;
            View.AddSubview(snapshot);

            internalTopView.View.Hidden = true;

            UIView.Animate(AnimationSpeed, 0, UIViewAnimationOptions.CurveEaseOut, 
            delegate 
            { 
                snapshot.Frame = new RectangleF(x, y, size.Width, size.Height); 
            },
            delegate
            {
                snapshot.AddGestureRecognizer(tapGesture);
            });

            if (showTrail)
            {
                UIView.Animate(AnimationSpeed, 0.1, UIViewAnimationOptions.CurveEaseOut, delegate 
                { 
                    snapshotTrail.Frame = new RectangleF(x, y, size.Width, size.Height); 
                }, 
                null);
            }
        }

        /// <summary>
        /// Hide Menu.
        /// </summary>
        public void Hide()
        {
            if (!isVisible) return;
            WillHideMenu(true);

            NSAction finished = delegate 
            {
                if (HideStatusBar)
                    UIApplication.SharedApplication.SetStatusBarHidden(false, false);

                internalTopView.View.Hidden = false;

                if (snapshotTrail != null)
                {
                    snapshotTrail.RemoveFromSuperview();
                    snapshotTrail.Dispose();
                    snapshotTrail = null;
                }

                if (snapshot != null)
                {
                    snapshot.UserInteractionEnabled = false;
                    snapshot.RemoveGestureRecognizer(tapGesture);
                    snapshot.RemoveFromSuperview();
                    snapshot.Dispose();
                    snapshot = null;
                }

                isVisible = false;
            };

            UIView.Animate(AnimationSpeed, 0, UIViewAnimationOptions.CurveEaseOut, delegate
            {
                if (snapshot != null)
                {
                    snapshot.Frame = new RectangleF(0, 0, originalSize.Width, originalSize.Height); 
                }
            }, finished);
        }

        private static UIImage TakeSnapshot(UIView view)
        {
            UIGraphics.BeginImageContextWithOptions(view.Bounds.Size, false, UIScreen.MainScreen.Scale);
            view.Layer.RenderInContext (UIGraphics.GetCurrentContext());           
            UIImage image = UIGraphics.GetImageFromCurrentImageContext();          
            UIGraphics.EndImageContext(); 
            return image; 
        }

        #region ProxyNavigationController

        // From https://github.com/thedillonb/MonoTouch.SlideoutNavigation/

        ///<summary>
        /// A proxy class for the navigation controller.
        /// This allows the menu view to make requests to the navigation controller
        /// and have them forwarded to the topview.
        ///</summary>
        private class ProxyNavigationController : UINavigationController
        {
            /// <summary>
            /// Gets or sets the parent controller.
            /// </summary>
            /// <value>
            /// The parent controller.
            /// </value>
            public RevealController ParentController { get; set; }

            /// <summary>
            /// Sets the controller.
            /// </summary>
            /// <param name='viewController'>
            /// View controller.
            /// </param>
            public void SetController(UIViewController viewController)
            {
                base.PopToRootViewController(false);
                base.PushViewController(viewController, false);
            }

            /// <Docs>
            /// To be added.
            /// </Docs>
            /// <summary>
            /// To be added.
            /// </summary>
            /// <param name='viewController'>
            /// View controller.
            /// </param>
            /// <param name='animated'>
            /// Animated.
            /// </param>
            public override void PushViewController(UIViewController viewController, bool animated)
            {
                ParentController.SelectView(viewController);
            }
        }

        #endregion
    }
}