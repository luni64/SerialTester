using System.Collections.ObjectModel;
using System.Windows.Data;
using lunOptics.TeensySharp;
using System.Linq;
using System.Threading;
using System.Windows;

namespace ViewModel
{
    public class MainVM : BaseViewModel
    {
        public ObservableCollection<ITeensy> Teensies { get; }

        private static object _lock = new object();

        public MainVM()
        {
            TeensySharp.SynchronizationContext(SynchronizationContext.Current);

            var boards = TeensySharp.ConnectedBoards;

            Teensies = new ObservableCollection<ITeensy>(boards);


            //BindingOperations.EnableCollectionSynchronization(Teensies, _lock);

            TeensySharp.ConnectedBoardsChanged += Watcher_ConnectionChanged;

            var teensy = TeensySharp.ConnectedBoards.FirstOrDefault();
            //teensy.Reboot();
            //teensy.Reset();                                

        }

        private void Watcher_ConnectionChanged(object sender, ConnectedBoardsChangedArgs e)
        {
            if (e.changeType == ChangeType.add)
            {
                Teensies.Add(e.changedDevice);
            }
            else
            {
                if (Teensies.Contains(e.changedDevice)) Teensies.Remove(e.changedDevice);
            }
        }
    }
}
