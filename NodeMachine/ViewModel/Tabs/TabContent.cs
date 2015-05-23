﻿
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using NodeMachine.Annotations;

namespace NodeMachine.ViewModel.Tabs
{
    public class TabContent
        : INotifyPropertyChanged
    {
        private readonly Control _content;

        public TabContent(Control content)
        {
            _content = content;

            var n = content as ITabName;
            if (n != null)
                n.PropertyChanged += (sender, args) => {
                    if (args.PropertyName == "TabName")
                        OnPropertyChanged("TabName");
                };
        }

        public string TabName
        {
            get
            {
                //Try to use the tab name explicitly set on this item
                var n = _content as ITabName;
                if (n != null && !string.IsNullOrWhiteSpace(n.TabName))
                    return n.TabName;

                //Otherwise just return the default
                return "Unnamed Tab";
            }
        }

        public Control Content
        {
            get
            {
                return _content;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
