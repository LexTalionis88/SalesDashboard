using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;

namespace SalesDashboard.Tests.Integration;

internal sealed class QueryCapture : DbCommandInterceptor
{
    public List<NpgsqlCommand> Commands { get; } = [];
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,
        CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
    {
        var copy = new NpgsqlCommand(command.CommandText);
        foreach (NpgsqlParameter parameter in command.Parameters)
            copy.Parameters.Add(new NpgsqlParameter(parameter.ParameterName, parameter.NpgsqlDbType) { Value = parameter.Value });
        Commands.Add(copy);
        return ValueTask.FromResult(result);
    }
}
