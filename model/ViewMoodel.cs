using MeAgent.model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeAgent.global
{
    internal class ViewMoodel
    {
        public ObservableCollection<Server> servers;
        public ObservableCollection<Logitem> activeLogs;
        public LimitedSizeObservableCollection<Logitem> historyLogs;

        public LimitedSizeObservableCollection<Logitem> GetHistoryLogs()
        {
            if (historyLogs == null)
            {
                historyLogs = new LimitedSizeObservableCollection<Logitem>(1000);
                return historyLogs;
            }
            else
            {
                return historyLogs;
            }
        }

        public ObservableCollection<Logitem> GetActiveLogs()
        {
            if (activeLogs == null)
            {
                activeLogs = new ObservableCollection<Logitem>();
                return activeLogs;
            }
            else
            {
                return activeLogs;
            }
        }

        public class LimitedSizeObservableCollection<T> : ObservableCollection<T>
        {
            public int Capacity { get; }

            public LimitedSizeObservableCollection(int capacity)
            {
                Capacity = capacity;
            }

            public new void Add(T item)
            {
                if (Count >= Capacity)
                {
                    this.RemoveAt(0);
                }
                base.Add(item);
            }

            public new void InsertItem(int index, T item)
            {
                if (Count >= Capacity)
                {
                    this.RemoveAt(Count - 1);
                }
                base.Insert(index, item);
            }
        }

        private static ViewMoodel instance = null;

        public static ViewMoodel GetInstance()
        {
            if (instance == null)
            {
                instance = new ViewMoodel();
            }
            return instance;
        }
    }
}