
using NodeMachine.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace NodeMachine.ViewModel.Tabs
{
    public class TabContent
        : INotifyPropertyChanged
    {
        private readonly UIElement _content;
        private readonly string _name;

        public TabContent(UIElement content, string name = null)
        {
            _content = content;
            _name = name;

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
                //Use the name explicitly set on this tab
                if (_name != null)
                    return _name;

                //Try to use the tab name explicitly set on this item
                var n = _content as ITabName;
                if (n != null && !string.IsNullOrWhiteSpace(n.TabName))
                    return n.TabName;

                //Otherwise just return the default
                return "Unnamed Tab";
            }
        }

        public UIElement Content
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
