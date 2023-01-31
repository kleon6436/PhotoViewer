using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Kchary.PhotoViewer.Helpers
{
    /// <summary>
    /// ObservableCollectionの拡張クラス
    /// </summary>
    /// <remarks>
    /// AddRangeで一度にアイテムに追加後に更新イベントを出すようにする
    /// </remarks>
    public class ObservableCollectionEx<T> : ObservableCollection<T>
    {
        public void AddRange(IEnumerable<T> addItems)
        {
            if (addItems == null)
            {
                throw new ArgumentNullException(nameof(addItems));
            }

            foreach (var item in addItems)
            {
                Items.Add(item);
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
