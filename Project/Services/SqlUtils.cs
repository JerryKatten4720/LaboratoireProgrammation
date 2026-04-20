using System.Data;
using Dapper;

namespace LaboratoireProgrammation.Project.Services;

public class SqlUtils {
    private readonly Func<IDbConnection> _connectionFactory;

    public SqlUtils(Func<IDbConnection> connectionFactory) {
        _connectionFactory = connectionFactory;
    }

    public IDbConnection GetConnection() {
        return _connectionFactory();
    }


    private bool IsSqlServer(IDbConnection connection) {
        return connection.GetType().Name.Contains("SqlConnection");
    }

    private string EscapeColumn(string columnName, IDbConnection connection) {
        if (IsSqlServer(connection)) return $"[{columnName}]";


        return columnName;
    }


    public bool IsAvailable() {
        try {
            using var c = GetConnection();
            c.Open();
            return true;
        }
        catch {
            return false;
        }
    }

    public void AssertStatus() {
        if (!IsAvailable()) throw new Exception("Database connection failed.");
    }

    public async Task ExecuteAsync(string sql, object param = null) {
        using var c = GetConnection();
        await c.ExecuteAsync(sql, param);
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null) {
        using var c = GetConnection();
        return await c.QueryAsync<T>(sql, param);
    }

    public void ExecuteTransaction(Action<IDbConnection, IDbTransaction> action) {
        using var c = GetConnection();
        c.Open();
        using var t = c.BeginTransaction();
        try {
            action(c, t);
            t.Commit();
        }
        catch {
            t.Rollback();
            throw;
        }
    }

    public bool Exists(string table, string cond, object param = null) {
        using var c = GetConnection();
        return c.ExecuteScalar<int>($"SELECT COUNT(1) FROM {table} WHERE {cond}", param) > 0;
    }

    public T ExecuteScalar<T>(string sql, object param = null) {
        using var c = GetConnection();
        return c.ExecuteScalar<T>(sql, param);
    }


    public void Insert<T>(string table, T entity) {
        using var c = GetConnection();
        var props = typeof(T).GetProperties();
        var cols = string.Join(", ", props.Select(p => EscapeColumn(p.Name, c)));
        var vals = string.Join(", ", props.Select(p => "@" + p.Name));

        c.Execute($"INSERT INTO {table} ({cols}) VALUES ({vals})", entity);
    }

    public void Update<T>(string table, T entity, string where) {
        using var c = GetConnection();
        var props = typeof(T).GetProperties().Select(p => $"{EscapeColumn(p.Name, c)} = @{p.Name}");
        var sets = string.Join(", ", props);

        c.Execute($"UPDATE {table} SET {sets} WHERE {where}", entity);
    }

    public void Delete(string table, string cond, object param = null) {
        using var c = GetConnection();
        c.Execute($"DELETE FROM {table} WHERE {cond}", param);
    }

    public long GetCount(string table) {
        return ExecuteScalar<long>($"SELECT COUNT(*) FROM {table}");
    }

    public void BulkInsert<T>(string table, IEnumerable<T> entities) {
        ExecuteTransaction((c, t) => {
            var props = typeof(T).GetProperties();
            var cols = string.Join(",", props.Select(p => EscapeColumn(p.Name, c)));
            var vals = string.Join(",", props.Select(p => "@" + p.Name));
            var sql = $"INSERT INTO {table} ({cols}) VALUES ({vals})";

            c.Execute(sql, entities, t);
        });
    }


    public void CreateTable(string table, string schema) {
        using var c = GetConnection();
        string sql;

        if (IsSqlServer(c))
            sql = $@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{table}') AND type in (N'U'))
                BEGIN
                    CREATE TABLE {table} ({schema});
                END";
        else
            sql = $"CREATE TABLE IF NOT EXISTS {table} ({schema});";

        c.Execute(sql);
    }
}