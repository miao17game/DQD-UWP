using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace DQD.Core.DataVirtualization {
    public interface IDataSource<T> {
        Task<T[]> fetchDataCallback(ItemIndexRange batch,CancellationToken token);
    }
}
