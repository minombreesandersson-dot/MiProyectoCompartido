using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;

namespace __NOMBRE__.Data;

// ADO.NET puro: conexión -> comando -> lector, sin Entity Framework.
// Con SQL Server es igual cambiando SqliteConnection por SqlConnection (paquete Microsoft.Data.SqlClient).
public class AdoService(IConfiguration config)
{
    private readonly string _cadena = config.GetConnectionString("DefaultConnection")!;

    // Lista las tablas de la base (sin las internas de SQLite y de EF)
    public List<string> Tablas()
    {
        var tablas = new List<string>();
        using var cn = new SqliteConnection(_cadena);
        cn.Open();
        using var cmd = cn.CreateCommand();
        cmd.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' " +
                          "AND name NOT LIKE 'sqlite_%' AND name NOT LIKE '\\_\\_%' ESCAPE '\\' ORDER BY name";
        using var lector = cmd.ExecuteReader();
        while (lector.Read()) tablas.Add(lector.GetString(0));
        return tablas;
    }

    // SELECT con parámetros -> DataTable (los parámetros evitan la inyección SQL)
    public DataTable Consultar(string sql, params (string Nombre, object Valor)[] parametros)
    {
        using var cn = new SqliteConnection(_cadena);
        cn.Open();
        using var cmd = cn.CreateCommand();
        cmd.CommandText = sql;
        foreach (var (nombre, valor) in parametros) cmd.Parameters.AddWithValue(nombre, valor);
        var tabla = new DataTable();
        using var lector = cmd.ExecuteReader();
        tabla.Load(lector);
        return tabla;
    }

    // INSERT / UPDATE / DELETE -> filas afectadas
    public int Ejecutar(string sql, params (string Nombre, object Valor)[] parametros)
    {
        using var cn = new SqliteConnection(_cadena);
        cn.Open();
        using var cmd = cn.CreateCommand();
        cmd.CommandText = sql;
        foreach (var (nombre, valor) in parametros) cmd.Parameters.AddWithValue(nombre, valor);
        return cmd.ExecuteNonQuery();
    }

    // Lo mismo con Dapper: una línea por consulta
    public int ContarFilas(string tabla)
    {
        using var cn = new SqliteConnection(_cadena);
        return cn.ExecuteScalar<int>($"SELECT COUNT(*) FROM \"{tabla}\"");
    }
}
