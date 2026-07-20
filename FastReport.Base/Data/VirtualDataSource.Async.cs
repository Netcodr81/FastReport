using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace FastReport.Data;


internal partial class VirtualDataSource
{
    public override Task InitSchemaAsync(CancellationToken cancellationToken = default)
    {
        InitSchema();
        return Task.CompletedTask;
    }

    public override Task LoadDataAsync(ArrayList rows, CancellationToken cancellationToken = default)
    {
        LoadData(rows);
        return Task.CompletedTask;
    }
}