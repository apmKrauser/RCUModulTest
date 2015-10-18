using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace ModulTest
{
    public class ObservableList<T> : ObservableCollection<T>
    {
        public void AddRange(IEnumerable<T> collection)
        {
            foreach (var el in collection)
                Items.Add(el);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
