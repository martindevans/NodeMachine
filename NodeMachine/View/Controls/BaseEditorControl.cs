using NodeMachine.Annotations;
using NodeMachine.Connection;
using NodeMachine.Model.Project;
using NodeMachine.ViewModel.Tabs;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace NodeMachine.View.Controls
{
    public abstract class BaseEditorControl<TModel>
        : UserControl, ITabName
        where TModel : class, INotifyPropertyChanged, new()
    {
        private TModel _value;
        public TModel Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (_value != null)
                    _value.PropertyChanged -= FacadePropertyChanged;
                _value = value;
                if (_value != null)
                {
                    _value.PropertyChanged += FacadePropertyChanged;
                    Unsaved = !ProjectDataModelCollection.Contains(_value);
                }

                OnPropertyChanged();
                OnPropertyChanged("TabName");
            }
        }

        private bool _unsaved;
        public bool Unsaved
        {
            get
            {
                return _unsaved;
            }
            set
            {
                _unsaved = value;
                OnPropertyChanged();
            }
        }

        public IProjectManager ProjectManager { get; private set; }
        public IGameConnection Connection { get; private set; }

        protected abstract ObservableCollection<TModel> ProjectDataModelCollection { get; }
        protected abstract string ValueName { get; }

        public string TabName
        {
            get
            {
                return _value == null ? (typeof(TModel).Name + " Editor") : string.Format("{0}.{1}", ValueName, typeof(TModel).Name);
            }
        }

        protected BaseEditorControl(IProjectManager manager, IGameConnection connection, TModel value)
        {
            ProjectManager = manager;
            Connection = connection;

            Value = value ?? new TModel();

            DataContext = this;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            ProjectDataModelCollection.CollectionChanged += (sender, args) => {
                if (args.OldItems != null && args.OldItems.Contains(Value))
                    Unsaved = true;
            };
        }

        private void FacadePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name")
                OnPropertyChanged("TabName");
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

        protected void DeleteFromProject(object sender, RoutedEventArgs e)
        {
            ProjectDataModelCollection.Remove(Value);
        }

        protected void AddToProject(object sender, RoutedEventArgs e)
        {
            if (!ProjectDataModelCollection.Contains(Value))
                ProjectDataModelCollection.Add(Value);
            Unsaved = false;
        }
    }
}
