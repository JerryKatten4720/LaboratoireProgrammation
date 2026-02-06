using System.Data;
using Dapper;
namespace LaboratoireProgrammation.Utils;
public class SqlUtils {
    
   private readonly Func<IDbConnection> _connectionFactory;
   
   public SqlUtils(Func<IDbConnection> connectionFactory) { _connectionFactory = connectionFactory; }
   public IDbConnection GetConnection() => _connectionFactory();
   public bool IsAvailable() {
       try {
           using var c = GetConnection();
           c.Open();
           return true;
       } catch { return false; }
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
       try { action(c, t); t.Commit(); }
       catch { t.Rollback(); throw; }
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
       var props = typeof(T).GetProperties();
       var cols = string.Join(", ", props.Select(p => p.Name));
       var vals = string.Join(", ", props.Select(p => "@" + p.Name));
       using var c = GetConnection();
       c.Execute($"INSERT INTO {table} ({cols}) VALUES ({vals})", entity);
   }
   
   public void Update<T>(string table, T entity, string where) {
       var props = typeof(T).GetProperties().Select(p => $"{p.Name} = @{p.Name}");
       var sets = string.Join(", ", props);
       using var c = GetConnection();
       c.Execute($"UPDATE {table} SET {sets} WHERE {where}", entity);
   }
   
   public void Delete(string table, string cond, object param = null) {
       using var c = GetConnection();
       c.Execute($"DELETE FROM {table} WHERE {cond}", param);
   }
   
   public long GetCount(string table) => ExecuteScalar<long>($"SELECT COUNT(*) FROM {table}");
   
   public void BulkInsert<T>(string table, IEnumerable<T> entities) {
       ExecuteTransaction((c, t) =>
       {
           var props = typeof(T).GetProperties();
           var sql = $"INSERT INTO {table} ({string.Join(",", props.Select(p => p.Name))}) VALUES ({string.Join(",", props.Select(p => "@" + p.Name))})";
           c.Execute(sql, entities, t);
       });
   }
   
   public void CreateTable(string table, string schema) => ExecuteScalar<int>($"CREATE TABLE IF NOT EXISTS {table} ({schema});");
   
}