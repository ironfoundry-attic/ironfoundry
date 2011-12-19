
﻿namespace CloudFoundry.Net.VsExtension.Ui.Controls.ViewModel
{
    using System.Linq;
    using CloudFoundry.Net.Extensions;
    using GalaSoft.MvvmLight;

    public class TreeViewItemViewModel : ViewModelBase
    {
        private static readonly TreeViewItemViewModel placeholderChild = new TreeViewItemViewModel();
        private readonly SafeObservableCollection<TreeViewItemViewModel> children;
        private readonly TreeViewItemViewModel parent;
        private bool isExpanded;
        private bool isSelected;

        protected TreeViewItemViewModel(TreeViewItemViewModel parent, bool lazyLoadChildren)
        {
            this.parent = parent;
            this.children = new SafeObservableCollection<TreeViewItemViewModel>();
            if (lazyLoadChildren)
                this.children.Add(placeholderChild);
        }

        private TreeViewItemViewModel()
        {
        }        

        public bool HasNotBeenPopulated
        {
            get { return this.children.Count == 1 && this.children.First() == placeholderChild; }
        }

        public bool IsExpanded
        {
            get { return this.isExpanded; }
            set
            {
                if (value != this.isExpanded)
                {
                    this.isExpanded = value;
                    RaisePropertyChanged("IsExpanded");
                }

                if (this.isExpanded && this.parent != null)
                    parent.isExpanded = true;

                if (this.HasNotBeenPopulated)
                {
                    this.Children.Remove(placeholderChild);
                    this.LoadChildren();
                }
            }
        }

        public bool IsSelected
        {
            get { return this.isSelected; }
            set
            {
                if (value != this.isSelected)
                {
                    this.isSelected = value;
                    RaisePropertyChanged("IsSelected");
                }
            }
        }

        public TreeViewItemViewModel Parent
        {
            get { return this.parent; }
        }

        public SafeObservableCollection<TreeViewItemViewModel> Children
        {
            get { return this.children; }
        }

        public virtual void LoadChildren()
        {

        }
    }
}
