using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using NodeMachine.Annotations;

namespace NodeMachine.View.Controls
{
    /// <summary>
    /// Interaction logic for StatusControl.xaml
    /// </summary>
    public partial class StatusControl : UserControl, INotifyPropertyChanged
    {
        public Process Process
        {
            get
            {
                return Process.GetCurrentProcess();
            }
        }

        public StatusControl()
        {
            InitializeComponent();

            Pump();
        }

        private async void Pump()
        {
            while (true)
            {
                OnPropertyChanged("Process");
                await Task.Delay(250);
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
