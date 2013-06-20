using System;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;

namespace MonoTouch.Reveal
{
    public class MenuViewController : UIViewController
    {
        private UITableView tableView;

        public MenuViewController()
            : base()
        {
            TextAttributes = new MenuItemTextAttributes
            {
                TextColor = UIColor.White,
                HighlightedTextColor = UIColor.LightGray,
                Font = UIFont.FromName("HelveticaNeue-Light", 21f)
            };
        }

        #region Properties

        private UIView backgroundView = null;
        public UIView BackgroundView
        {
            get { return backgroundView; }
            set
            { 
                backgroundView = value;
            }
        }

        public MenuItemTextAttributes TextAttributes { get; set; }

        #endregion

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = UIColor.Black;

            if (NavigationController != null)
                NavigationController.NavigationBarHidden = true;

            if (BackgroundView != null)
                View.AddSubview(BackgroundView);             

            tableView = CreateTableView(View.Bounds, UITableViewStyle.Plain);
            tableView.BackgroundView = null;
            tableView.BackgroundColor = UIColor.Clear;
            tableView.Source = new TableSource(this);
            tableView.Alpha = 0;
            View.AddSubview(tableView);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            if (tableView == null) return;

            if (animated == true)
            {
                tableView.Transform = CGAffineTransform.MakeScale(0.9f, 0.9f);
                tableView.Alpha = 0;

                UIView.Animate(0.5, delegate 
                {
                    tableView.Transform = CGAffineTransform.MakeIdentity();
                });
                UIView.Animate(0.6, delegate 
                {
                    tableView.Alpha = 1;
                });
            }
            else
            {
                tableView.Alpha = 1;
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (tableView == null) return;

            if (animated == true)
            {
                UIView.Animate(0.2, delegate 
                {
                    tableView.Transform = CGAffineTransform.MakeScale(0.7f, 0.7f);
                    tableView.Alpha = 0;
                });
            }
            else
            {
                tableView.Alpha = 0;
            }
        }

        public virtual UITableView CreateTableView(RectangleF bounds, UITableViewStyle style)
        {
            RectangleF frame  = new RectangleF(22f, 44f, bounds.Width - 44f, bounds.Height - 44f);

            UITableView tv = new UITableView(frame, style);
            tv.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin;
            tv.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            tv.AutosizesSubviews = true;
            return tv;
        }

        public void Reload()
        {
            if (tableView != null)
            {
                tableView.ReloadData();
            }
        }

        #region Items

        public readonly List<MenuItem> Items = new List<MenuItem>();

        public void AddItem<T>(string label)
            where T : UIViewController
        {
            Items.Add(MenuItem.Create<T>(label));
        }

        public void AddItem(string label)
        {
            AddItem<UIViewController>(label);
        }

        #endregion

        #region Events

        public event EventHandler<MenuItemSelectedEventArgs> ItemSelected;

        protected void OnItemSelected(MenuItem item)
        {
            if (item == null || item.ControllerType == null)
                return;

            if (ResolveController != null)
            {
                UIViewController controller = ResolveController(item.Label, item.ControllerType);
                if (controller != null && NavigationController != null)
                {
                    NavigationController.PopToRootViewController(false);
                    controller.RemoveFromParentViewController();
                    NavigationController.PushViewController(controller, false);
                }
            }

            if (ItemSelected != null)
            {
                ItemSelected(this, new MenuItemSelectedEventArgs(item));
            }
        }

        #endregion

        public ResolveController ResolveController;

        #region UITableViewSource

        private class TableSource : UITableViewSource
        {
            private MenuViewController controller = null;

            public TableSource(MenuViewController controller)
            {
                this.controller = controller;
            }

            public override int NumberOfSections(UITableView tableView)
            {
                return 1;
            }

            public override int RowsInSection(UITableView tableview, int section)
            {
                return controller.Items.Count;
            }

            #region Header

            public override float GetHeightForHeader(UITableView tableView, int section)
            {
                return 0;
            }

            public override UIView GetViewForHeader(UITableView tableView, int section)
            {
                return new UIView();
            }

            #endregion

            private static readonly NSString CellKey = new NSString("MenuItemCellKey");

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.DequeueReusableCell(CellKey);
                if (cell == null)
                {
                    cell = new UITableViewCell(UITableViewCellStyle.Default, CellKey);
                    cell.BackgroundColor = UIColor.Clear;
                    cell.SelectionStyle = UITableViewCellSelectionStyle.None;

                    if (controller.TextAttributes != null)
                    {
                        cell.TextLabel.Font = controller.TextAttributes.Font ?? cell.TextLabel.Font;
                        cell.TextLabel.TextColor = controller.TextAttributes.TextColor;
                        cell.TextLabel.HighlightedTextColor = controller.TextAttributes.HighlightedTextColor;
                        if (controller.TextAttributes.TextShadowColor != null)
                        {
                            cell.TextLabel.ShadowColor = controller.TextAttributes.TextShadowColor;
                            cell.TextLabel.ShadowOffset = new SizeF(controller.TextAttributes.TextShadowOffset.Horizontal, controller.TextAttributes.TextShadowOffset.Vertical);
                        }
                    }
                }

                MenuItem menuItem = controller.Items[indexPath.Row];

                cell.TextLabel.Text = menuItem.Label;
                return cell;
            }

            public override void WillDisplay(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
            {

            }

            public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
            {
                tableView.DeselectRow(indexPath, false);

                MenuItem menuItem = controller.Items[indexPath.Row];
                controller.OnItemSelected(menuItem);
            }
        }
    
        #endregion
    }

    public delegate UIViewController ResolveController(string label, Type controllerType);

    public class MenuItemTextAttributes : UITextAttributes
    {
        public UIColor HighlightedTextColor;
    }

    public class MenuItem
    {
        public MenuItem(string label, Type controllerType)
        {
            Label = label;
            ControllerType = controllerType;
        }

        public static MenuItem Create<T>(string label)
            where T : UIViewController
        {
            return new MenuItem(label, typeof(T));
        }

        public string Label { get; set; }
        public Type ControllerType { get; set; }

        private Guid id = Guid.NewGuid();
        public override bool Equals(object obj)
        {
            if (obj is MenuItem)
            {
                return (obj as MenuItem).id.Equals(id);
            }
            return false;
        }
    }

    public class MenuItemSelectedEventArgs : EventArgs
    {
        public MenuItemSelectedEventArgs(MenuItem item)
        {
            Item = item;
        }

        public MenuItem Item { get; private set; }
    }
}

