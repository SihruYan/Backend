using System.Data;
using Backend.Model;
using Dapper;
using Npgsql;

namespace Backend.Data;

public class DapperDb
{
    private readonly string _connStr;

    public DapperDb(IConfiguration config)
    {
        _connStr = Environment.GetEnvironmentVariable("SUPABASE_CONN")
                   ?? config.GetConnectionString("DefaultConnection");
    }

    private IDbConnection CreateConnection() => new NpgsqlConnection(_connStr);

    public async Task<IEnumerable<Product>> GetProductsAsync()
    {
        using var conn = CreateConnection();
        var sql = "SELECT * FROM product";
        return await conn.QueryAsync<Product>(sql);
    }

    public async Task<int> InsertProductAsync(Product product)
    {
        using var conn = CreateConnection();
        var sql = @"INSERT INTO product (""Id"",""Name"", ""Description"")
                    VALUES (@Id,@Name, @Description)";
        return await conn.ExecuteAsync(sql, product);
    }
}