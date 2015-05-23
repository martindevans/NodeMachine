using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NodeMachine.Annotations;
using NodeMachine.Model;
using System;
using System.Collections.Generic;

namespace NodeMachine.ViewModel.Nodes
{
    public class ProceduralNodeViewModel
        : INotifyPropertyChanged
    {
        private readonly ProceduralNode _node;

        public Guid Guid
        {
            get
            {
                return _node.Guid;
            }
        }

        public string TypeName
        {
            get
            {
                return _node.TypeName;
            }
        }

        public SubdivisionState State
        {
            get
            {
                return _node.State;
            }
        }

        public BoundingBox Bounds
        {
            get
            {
                return _node.Bounds;
            }
        }

        public IEnumerable<PropertyValue> HierarchicalProperties
        {
            get
            {
                return _node.HierarchicalProperties
                            .GroupBy(a => a.Key)
                            .Select(a => new PropertyValue(a.Key, a.Last().Value));
            }
        }

        private readonly List<ProceduralNodeViewModel> _children; 
        public List<ProceduralNodeViewModel> Children
        {
            get
            {
                return _children.Where(c => c.IsFiltered).ToList();
            }
        }

        public IEnumerable<PropertyValue> Metadata
        {
            get
            {
                return _node.Metadata;
            }
        }

        private bool _filtered = true;
        public bool IsFiltered
        {
            get
            {
                return _filtered;
            }
            set
            {
                _filtered = value;
                OnPropertyChanged();
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (value == _isSelected)
                    return;
                _isSelected = value;
                OnPropertyChanged();

                //todo:highlight this element in game
            }
        }

        public ProceduralNodeViewModel(ProceduralNode node)
        {
            _node = node;
            _children = node.Children.Select(a => new ProceduralNodeViewModel(a)).ToList();
        }

        public async Task<int> Filter(string filterString)
        {
            return await Filter(Filters.Parse(filterString));
        }

        private class Filters
        {
            public readonly string[] Strings;

            private Filters(string[] strings)
            {
                Strings = strings;
            }

            public static Filters Parse(string filterString)
            {
                if (string.IsNullOrWhiteSpace(filterString))
                    return null;

                return new Filters(filterString.Split(new [] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        private async Task<int> Filter(Filters filters)
        {
            int filtered = 0;

            //Filter children
            foreach (var child in _children)
                filtered += await child.Filter(filters);

            if (filters == null)
            {
                IsFiltered = true;
            }
            else
            {
                //Filter self (skip entirely if a child is visible)
                IsFiltered = Children.Any(a => a.IsFiltered)
                             || filters.Strings.Any(s => TypeName.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0)
                             || filters.Strings.Any(s => Metadata.Any(a => a.Key.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0))
                             || filters.Strings.Any(s => HierarchicalProperties.Any(a => a.Key.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            OnPropertyChanged("Children");

            return filtered + (IsFiltered ? 1 : 0);
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
