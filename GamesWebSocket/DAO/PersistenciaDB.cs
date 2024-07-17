using GamesWebSocket;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Data.Common;
using System.ServiceModel.Channels;

namespace GamesWebSocket.DAO;

public class QueryExecution<TParameter>
{
    public string QueryString { get; } = string.Empty;
    public IEnumerable<TParameter>? Parameters { get; } = null;
    public bool ExpectNoReturnEffect { get; } = true;

    private QueryExecution(string Query)
    {
        QueryString = Query;
    }
    private QueryExecution(string Query, bool expectEffect)
    {
        QueryString = Query;
        ExpectNoReturnEffect = expectEffect;
    }
    private QueryExecution(string Query, IEnumerable<TParameter> parameters)
    {
        QueryString = Query;
        Parameters = parameters.ToArray();
    }

    private QueryExecution(string Query, IEnumerable<TParameter> parameters, bool expectEffect)
    {
        QueryString = Query;
        Parameters = parameters.ToArray();
        ExpectNoReturnEffect = expectEffect;
    }

    public static QueryExecution<TParameter> CreateQuery(string query) => new(query);
    public static QueryExecution<TParameter> CreateQuery(string query, bool expectEffect) => new(query, expectEffect);
    public static QueryExecution<TParameter> CreateQuery(string query, IEnumerable<TParameter> parameters) => new(query, parameters);
    public static QueryExecution<TParameter> CreateQuery(string query, IEnumerable<TParameter> parameters, bool expectEffect) => new(query, parameters, expectEffect);
}
public class QueryExecution
{
    public string QueryString { get; } = string.Empty;
    public IEnumerable<OracleParameter>? Parameters { get; } = null;
    public bool ExpectNoReturnEffect { get; } = true;

    private QueryExecution(QueryExecution<OracleParameter> qeExecution)
    {
        QueryString = qeExecution.QueryString;
        Parameters = qeExecution.Parameters;
        ExpectNoReturnEffect = qeExecution.ExpectNoReturnEffect;
    }

    public static QueryExecution CreateQuery(string query) =>
        new(QueryExecution<OracleParameter>.CreateQuery(query));

    public static QueryExecution CreateQuery(string query, bool expectEffect) =>
        new(QueryExecution<OracleParameter>.CreateQuery(query, expectEffect));

    public static QueryExecution CreateQuery(string query, IEnumerable<OracleParameter> parameters) =>
        new(QueryExecution<OracleParameter>.CreateQuery(query, parameters));

    public static QueryExecution CreateQuery(string query, IEnumerable<OracleParameter> parameters, bool expectEffect) =>
        new(QueryExecution<OracleParameter>.CreateQuery(query, parameters, expectEffect));
}

public interface IPersistenciaDB
{
    DataTable ExecuteQuery(QueryExecution queryExecution, bool withTransaction = true);
    Task<DataTable> ExecuteQueryAsync(QueryExecution queryExecution, bool withTransaction = true);
    IEnumerable<DataTable> ExecuteAllQueries(IEnumerable<QueryExecution> queryExecution, bool withTransaction = true);
    Task<IEnumerable<DataTable>> ExecuteAllQueriesAsync(IEnumerable<QueryExecution> queryExecution, bool withTransaction = true);
    IDictionary<string, DataTable> ExecuteAllQueries(IDictionary<string, QueryExecution> queryExecution, bool withTransaction = true);

    bool ExecuteNoReturnQuery(QueryExecution queryExecution, bool withTransaction = true);
    Task<bool> ExecuteNoReturnQueryAsync(QueryExecution queryExecution, bool withTransaction = true);
    IEnumerable<bool> ExecuteAllNoReturnQueries(IEnumerable<QueryExecution> queryExecution, bool withTransaction = true);
    Task<IEnumerable<bool>> ExecuteAllNoReturnQueriesAsync(IEnumerable<QueryExecution> queryExecution, bool withTransaction = true);
    IDictionary<string, bool> ExecuteAllNoReturnQueries(IDictionary<string, QueryExecution> queryExecution, bool withTransaction = true);

}

public class PersistenciaDB<TConnection>
    : IPersistenciaDB
    where TConnection : IDbConnection, new()
{

    private IDbCommand CreateCommandFromQueryExecution(QueryExecution queryExecution, IDbConnection connection)
    {
        IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = queryExecution.QueryString;
        cmd.Connection = connection;

        if (queryExecution.Parameters is not null)
        {
            foreach (var param in queryExecution.Parameters)
            {
                cmd.Parameters.Add(param.Clone());
            }
        }
        return cmd;
    }

    // ==================== EXECUTE QUERY - DATATABLE ====================

    /// <summary>
    /// Função que executa uma Query junto de seus parâmetros.
    /// </summary>
    /// <param name="queryExecution">Objeto que contêm a String da Query e os Parâmetros dela</param>
    /// <returns>DataTable das informações da query</returns>
    public DataTable ExecuteQuery(QueryExecution queryExecution, bool withTransaction = true)
    {
        TConnection _connection = new();
        if (Settings.config is null) throw new Exception("Algum erro ocorreu com a conexão ao servidor.");
        _connection.ConnectionString = Settings.config.GetSection("API:CONNECTION_STRING").Value;

        IDataReader dr;
        DataTable dt = new();

        IDbCommand cmd = CreateCommandFromQueryExecution(queryExecution, _connection);

        if (withTransaction)
        {
            IDbTransaction t = _connection.BeginTransaction();
            cmd.Transaction = t;
        }

        _connection.Open();

        try
        {
            dr = cmd.ExecuteReader();

            dt.Load(dr);

            dr.Close();

            if (withTransaction && cmd.Transaction != null) cmd.Transaction.Commit();
        }
        catch (Exception ex)
        {
            if (withTransaction && cmd.Transaction != null) cmd.Transaction.Rollback();
            throw new Exception("Algum erro acontenceu na execução da query. " + ex.Message);
        }

        if (_connection != null && _connection.State == ConnectionState.Open)
        {
            _connection.Close();
            _connection.Dispose();
        }

        return dt;
    }

    /// <summary>
    /// Assíncrono -
    /// Função que executa uma Query junto de seus parâmetros.
    /// </summary>
    /// <param name="queryExecution">Objeto que contêm a String da Query e os Parâmetros dela</param>
    /// <returns>DataTable das informações da query</returns>
    public async Task<DataTable> ExecuteQueryAsync(QueryExecution queryExecution, bool withTransaction = true)
    {
        TConnection _connection = new();
        if (Settings.config is null) throw new Exception("Algum erro ocorreu com a conexão ao servidor.");
        _connection.ConnectionString = Settings.config.GetSection("API:CONNECTION_STRING").Value;

        IDataReader dr;
        DataTable dt = new();

        IDbCommand cmd = CreateCommandFromQueryExecution(queryExecution, _connection);

        if (withTransaction)
        {
            IDbTransaction t = _connection.BeginTransaction();
            cmd.Transaction = t;
        }

        _connection.Open();

        try
        {
            dr = await ((DbCommand)cmd).ExecuteReaderAsync();

            dt.Load(dr);

            dr.Close();

            if (withTransaction && cmd.Transaction != null) cmd.Transaction.Commit();
        }
        catch (Exception ex)
        {
            if (withTransaction && cmd.Transaction != null) cmd.Transaction.Rollback();
            throw new Exception("Algum erro acontenceu na execução da query. " + ex.Message);
        }

        if (_connection != null && _connection.State == ConnectionState.Open)
        {
            _connection.Close();
            _connection.Dispose();
        }

        return dt;
    }

    /// <summary>
    /// Função que executa uma Lista de Queries junto de seus parâmetros.
    /// Tem a mesma funcionalidade do 'ExecuteDataQuery'. A diferença é que ela executa uma lista de 
    /// queries antes da conexão ser fechada.
    /// Dessa maneira, uma Lista de DataTables será retornado.
    /// </summary>
    /// <param name="queryExecution">Uma lista de Objetos que contêm a String da Query e os Parâmetros dela</param>
    /// <returns>Lista de DataTables de mesmo tamanho da entrada.</returns>
    public IEnumerable<DataTable> ExecuteAllQueries(IEnumerable<QueryExecution> queryExecution, bool withTransaction = true)
    {
        TConnection _connection = new();
        if (Settings.config is null) throw new Exception("Algum erro ocorreu com a conexão ao servidor.");
        _connection.ConnectionString = Settings.config.GetSection("API:CONNECTION_STRING").Value;

        List<IDbCommand> cmdList = new() { };
        List<DataTable> dt = new();

        _connection.Open();

        if (withTransaction)
        {
            using IDbTransaction t = _connection.BeginTransaction();
            foreach (QueryExecution queryInfo in queryExecution)
            {
                IDbCommand cmd = CreateCommandFromQueryExecution(queryInfo, _connection);
                cmd.Transaction = t;

                cmdList.Add(cmd);
            }
            try
            {
                foreach (IDbCommand dbCommand in cmdList)
                {
                    IDataReader dr_temp = dbCommand.ExecuteReader();
                    DataTable dt_temp = new();

                    dt_temp.Load(dr_temp);

                    dr_temp.Close();
                    dt.Add(dt_temp);
                }
                t.Commit();
            }
            catch (Exception ex)
            {
                t.Rollback();
                throw new Exception("Algum erro acontenceu na execução da query. " + ex.Message);
            }

        }
        else
        {
            foreach (QueryExecution queryInfo in queryExecution)
            {
                IDbCommand cmd = CreateCommandFromQueryExecution(queryInfo, _connection);
                cmdList.Add(cmd);
            }
            try
            {
                foreach (IDbCommand dbCommand in cmdList)
                {
                    IDataReader dr_temp = dbCommand.ExecuteReader();
                    DataTable dt_temp = new();

                    dt_temp.Load(dr_temp);

                    dr_temp.Close();
                    dt.Add(dt_temp);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Algum erro acontenceu na execução da query. " + ex.Message);
            }
        }


        if (_connection != null && _connection.State == ConnectionState.Open)
        {
            _connection.Close();
            _connection.Dispose();
        }

        return dt;
    }

    /// <summary>
    /// Assíncrono -
    /// Função que executa uma Lista de Queries junto de seus parâmetros.
    /// Tem a mesma funcionalidade do 'ExecuteDataQuery'. A diferença é que ela executa uma lista de 
    /// queries antes da conexão ser fechada.
    /// Dessa maneira, uma Lista de DataTables será retornado.
    /// </summary>
    /// <param name="queryExecution">Uma lista de Objetos que contêm a String da Query e os Parâmetros dela</param>
    /// <returns>Lista de DataTables de mesmo tamanho da entrada.</returns>
    public async Task<IEnumerable<DataTable>> ExecuteAllQueriesAsync(IEnumerable<QueryExecution> queryExecution, bool withTransaction = true)
    {
        TConnection _connection = new();
        if (Settings.config is null) throw new Exception("Algum erro ocorreu com a conexão ao servidor.");
        _connection.ConnectionString = Settings.config.GetSection("API:CONNECTION_STRING").Value;

        List<IDbCommand> cmdList = new() { };
        List<DataTable> dt = new();

        _connection.Open();

        if (withTransaction)
        {
            using IDbTransaction t = _connection.BeginTransaction();
            foreach (QueryExecution queryInfo in queryExecution)
            {
                IDbCommand cmd = CreateCommandFromQueryExecution(queryInfo, _connection);
                cmd.Transaction = t;

                cmdList.Add(cmd);
            }
            try
            {
                foreach (IDbCommand dbCommand in cmdList)
                {
                    IDataReader dr_temp = await ((DbCommand)dbCommand).ExecuteReaderAsync(); // dbCommand.ExecuteReader();
                    DataTable dt_temp = new();

                    dt_temp.Load(dr_temp);

                    dr_temp.Close();
                    dt.Add(dt_temp);
                }
                t.Commit();
            }
            catch (Exception ex)
            {
                t.Rollback();
                throw new Exception("Algum erro acontenceu na execução da query. " + ex.Message);
            }
        }
        else
        {
            foreach (QueryExecution queryInfo in queryExecution)
            {
                IDbCommand cmd = CreateCommandFromQueryExecution(queryInfo, _connection);
                cmdList.Add(cmd);
            }
            try
            {
                foreach (IDbCommand dbCommand in cmdList)
                {
                    IDataReader dr_temp = await ((DbCommand)dbCommand).ExecuteReaderAsync(); // dbCommand.ExecuteReader();
                    DataTable dt_temp = new();

                    dt_temp.Load(dr_temp);

                    dr_temp.Close();
                    dt.Add(dt_temp);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Algum erro acontenceu na execução da query. " + ex.Message);
            }
        }

        if (_connection != null && _connection.State == ConnectionState.Open)
        {
            _connection.Close();
            _connection.Dispose();
        }

        return dt;
    }

    /// <summary>
    /// Função que executa uma Lista de Queries junto de seus parâmetros.
    /// Tem a mesma funcionalidade do 'ExecuteDataQuery'. A diferença é que ela executa uma lista de 
    /// queries antes da conexão ser fechada.
    /// Dessa maneira, uma Lista de DataTables será retornado.
    /// </summary>
    /// <param name="queryExecution">Uma lista de Objetos que contêm a String da Query e os Parâmetros dela</param>
    /// <returns>Lista de DataTables de mesmo tamanho da entrada.</returns>
    public IDictionary<string, DataTable> ExecuteAllQueries(IDictionary<string, QueryExecution> queryExecution, bool withTransaction = true)
    {
        TConnection _connection = new();
        if (Settings.config is null) throw new Exception("Algum erro ocorreu com a conexão ao servidor.");
        _connection.ConnectionString = Settings.config.GetSection("API:CONNECTION_STRING").Value;

        Dictionary<string, IDbCommand> cmdDict = new();
        Dictionary<string, DataTable> dt = new();

        _connection.Open();

        if (withTransaction)
        {
            using IDbTransaction t = _connection.BeginTransaction();
            foreach (var (key, queryExec) in queryExecution)
            {
                IDbCommand cmd = CreateCommandFromQueryExecution(queryExec, _connection);
                cmd.Transaction = t;

                cmdDict.Add(key, cmd);
            }
            try
            {
                foreach (var (key, oracleCommand) in cmdDict)
                {
                    IDataReader dr_temp = oracleCommand.ExecuteReader();
                    DataTable dt_temp = new();

                    dt_temp.Load(dr_temp);

                    dr_temp.Close();
                    dt.Add(key, dt_temp);
                }

                t.Commit();
            }
            catch (Exception ex)
            {
                t.Rollback();
                throw new Exception("Algum erro acontenceu na execução da query. " + ex.Message);
            }
        }
        else
        {
            foreach (var (key, queryExec) in queryExecution)
            {
                IDbCommand cmd = CreateCommandFromQueryExecution(queryExec, _connection);
                cmdDict.Add(key, cmd);
            }
            try
            {
                foreach (var (key, oracleCommand) in cmdDict)
                {
                    IDataReader dr_temp = oracleCommand.ExecuteReader();
                    DataTable dt_temp = new();

                    dt_temp.Load(dr_temp);

                    dr_temp.Close();
                    dt.Add(key, dt_temp);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Algum erro acontenceu na execução da query. " + ex.Message);
            }
        }

        if (_connection != null && _connection.State == ConnectionState.Open)
        {
            _connection.Close();
            _connection.Dispose();
        }

        return dt;
    }

    // ==================== EXECUTE NO RETURN QUERY - BOOLEAN ====================

    /// <summary>
    /// Função que executa uma Query junto de seus parâmetros.
    /// Sua principal diferença é que ela verifica se a query afetou alguma linha e retorna um boolean.
    /// Se as linhas afetados foram > 0. Então a função retorna true.
    /// </summary>
    /// <param name="queryExecution">Objeto que contêm a String da Query e os Parâmetros dela</param>
    /// <returns>Boolean. True se linhas afetadas > 0</returns>
    public bool ExecuteNoReturnQuery(QueryExecution queryExecution, bool withTransaction = true)
    {
        TConnection _connection = new();
        if (Settings.config is null) throw new Exception("Algum erro ocorreu com a conexão ao servidor.");
        _connection.ConnectionString = Settings.config.GetSection("API:CONNECTION_STRING").Value;

        bool dt = true;

        IDbCommand cmd = CreateCommandFromQueryExecution(queryExecution, _connection);

        if (withTransaction)
        {
            IDbTransaction t = _connection.BeginTransaction();
            cmd.Transaction = t;
        }

        _connection.Open();

        try
        {
            int rowsAffected = cmd.ExecuteNonQuery();

            if (queryExecution.ExpectNoReturnEffect == true && rowsAffected == 0) dt = false;

            if (withTransaction && cmd.Transaction != null) cmd.Transaction.Commit();
        }
        catch (Exception ex)
        {
            if (withTransaction && cmd.Transaction != null) cmd.Transaction.Rollback();
            throw new Exception("Algum erro acontenceu na execução da query. " + ex.Message);
        }

        if (_connection != null && _connection.State == ConnectionState.Open)
        {
            _connection.Close();
            _connection.Dispose();
        }

        return dt;
    }
    /// <summary>
    /// Assíncrono -
    /// Função que executa uma Query junto de seus parâmetros.
    /// Sua principal diferença é que ela verifica se a query afetou alguma linha e retorna um boolean.
    /// Se as linhas afetados foram > 0. Então a função retorna true.
    /// </summary>
    /// <param name="queryExecution">Objeto que contêm a String da Query e os Parâmetros dela</param>
    /// <returns>Boolean. True se linhas afetadas > 0</returns>
    public async Task<bool> ExecuteNoReturnQueryAsync(QueryExecution queryExecution, bool withTransaction = true)
    {
        TConnection _connection = new();
        if (Settings.config is null) throw new Exception("Algum erro ocorreu com a conexão ao servidor.");
        _connection.ConnectionString = Settings.config.GetSection("API:CONNECTION_STRING").Value;

        bool dt = true;

        IDbCommand cmd = CreateCommandFromQueryExecution(queryExecution, _connection);

        if (withTransaction)
        {
            IDbTransaction t = _connection.BeginTransaction();
            cmd.Transaction = t;
        }

        _connection.Open();

        try
        {
            int rowsAffected = await ((DbCommand)cmd).ExecuteNonQueryAsync();

            if (queryExecution.ExpectNoReturnEffect == true && rowsAffected == 0) dt = false;

            if (withTransaction && cmd.Transaction != null) cmd.Transaction.Commit();
        }
        catch (Exception ex)
        {
            if (withTransaction && cmd.Transaction != null) cmd.Transaction.Rollback();
            throw new Exception("Algum erro acontenceu na execução da query. " + ex.Message);
        }

        if (_connection != null && _connection.State == ConnectionState.Open)
        {
            _connection.Close();
            _connection.Dispose();
        }

        return dt;
    }

    /// <summary>
    /// Função que executa uma Lista de Queries junto de seus parâmetros.
    /// Tem a mesma funcionalidade do 'ExecuteNoReturnQuery'. A diferença é que ela executa uma lista de 
    /// queries antes da conexão ser fechada.
    /// Dessa maneira, uma Lista de Booleanos será retornado. A mesma regra do 'ExecuteNoReturnQuery' se aplica.
    /// </summary>
    /// <param name="queryExecution">Uma lista de Objetos que contêm a String da Query e os Parâmetros dela</param>
    /// <returns>Lista de Boolean de mesmo tamanho da entrada. True se linhas afetadas > 0</returns>
    public IEnumerable<bool> ExecuteAllNoReturnQueries(IEnumerable<QueryExecution> queryExecution, bool withTransaction = true)
    {
        TConnection _connection = new();
        if (Settings.config is null) throw new Exception("Algum erro ocorreu com a conexão ao servidor.");
        _connection.ConnectionString = Settings.config.GetSection("API:CONNECTION_STRING").Value;

        List<IDbCommand> cmdList = new();
        List<bool> dbDataReaders = new();

        _connection.Open();
        if (withTransaction)
        {
            using IDbTransaction t = _connection.BeginTransaction();
            foreach (QueryExecution queryInfo in queryExecution)
            {
                IDbCommand cmd = CreateCommandFromQueryExecution(queryInfo, _connection);
                cmd.Transaction = t;

                cmdList.Add(cmd);
            }
            try
            {
                for (int i = 0; i < cmdList.Count; i++)
                {
                    IDbCommand dbCommand = cmdList[i];
                    int rowsAffected = dbCommand.ExecuteNonQuery();

                    bool result = true;

                    if (queryExecution.ToList()[i].ExpectNoReturnEffect == true && rowsAffected == 0) result = false;

                    dbDataReaders.Add(result);
                }
                t.Commit();
            }
            catch (Exception ex)
            {
                t.Rollback();
                throw new Exception("Algum erro acontenceu na execução da query. " + ex.Message);
            }
        }
        else
        {
            foreach (QueryExecution queryInfo in queryExecution)
            {
                IDbCommand cmd = CreateCommandFromQueryExecution(queryInfo, _connection);
                cmdList.Add(cmd);
            }
            try
            {
                for (int i = 0; i < cmdList.Count; i++)
                {
                    IDbCommand dbCommand = cmdList[i];
                    int rowsAffected = dbCommand.ExecuteNonQuery();

                    bool result = true;

                    if (queryExecution.ToList()[i].ExpectNoReturnEffect == true && rowsAffected == 0) result = false;

                    dbDataReaders.Add(result);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Algum erro acontenceu na execução da query. " + ex.Message);
            }
        }

        if (_connection != null && _connection.State == ConnectionState.Open)
        {
            _connection.Close();
            _connection.Dispose();
        }

        return dbDataReaders;
    }

    /// <summary>
    /// Assíncrono -
    /// Função que executa uma Lista de Queries junto de seus parâmetros.
    /// Tem a mesma funcionalidade do 'ExecuteNoReturnQuery'. A diferença é que ela executa uma lista de 
    /// queries antes da conexão ser fechada.
    /// Dessa maneira, uma Lista de Booleanos será retornado. A mesma regra do 'ExecuteNoReturnQuery' se aplica.
    /// </summary>
    /// <param name="queryExecution">Uma lista de Objetos que contêm a String da Query e os Parâmetros dela</param>
    /// <returns>Lista de Boolean de mesmo tamanho da entrada. True se linhas afetadas > 0</returns>
    public async Task<IEnumerable<bool>> ExecuteAllNoReturnQueriesAsync(IEnumerable<QueryExecution> queryExecution, bool withTransaction = true)
    {
        TConnection _connection = new();
        if (Settings.config is null) throw new Exception("Algum erro ocorreu com a conexão ao servidor.");
        _connection.ConnectionString = Settings.config.GetSection("API:CONNECTION_STRING").Value;

        List<IDbCommand> cmdList = new();
        List<bool> dbDataReaders = new();

        _connection.Open();

        if (withTransaction)
        {
            using IDbTransaction t = _connection.BeginTransaction();
            foreach (QueryExecution queryInfo in queryExecution)
            {
                IDbCommand cmd = CreateCommandFromQueryExecution(queryInfo, _connection);
                cmd.Transaction = t;

                cmdList.Add(cmd);
            }
            try
            {
                for (int i = 0; i < cmdList.Count; i++)
                {
                    IDbCommand dbCommand = cmdList[i];
                    int rowsAffected = await ((DbCommand)dbCommand).ExecuteNonQueryAsync();

                    bool result = true;

                    if (queryExecution.ToList()[i].ExpectNoReturnEffect == true && rowsAffected == 0) result = false;

                    dbDataReaders.Add(result);
                }
                if (dbDataReaders.Any(el => el == false)) throw new Exception("Uma das queries não foi executada corretamente.");
                t.Commit();
            }
            catch (Exception ex)
            {
                t.Rollback();
                throw new Exception("Algum erro acontenceu na execução da query. " + ex.Message);
            }
        }
        else
        {
            foreach (QueryExecution queryInfo in queryExecution)
            {
                IDbCommand cmd = CreateCommandFromQueryExecution(queryInfo, _connection);
                cmdList.Add(cmd);
            }
            try
            {
                for (int i = 0; i < cmdList.Count; i++)
                {
                    IDbCommand dbCommand = cmdList[i];
                    int rowsAffected = await ((DbCommand)dbCommand).ExecuteNonQueryAsync();

                    bool result = true;

                    if (queryExecution.ToList()[i].ExpectNoReturnEffect == true && rowsAffected == 0) result = false;

                    dbDataReaders.Add(result);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Algum erro acontenceu na execução da query. " + ex.Message);
            }
        }

        if (_connection != null && _connection.State == ConnectionState.Open)
        {
            _connection.Close();
            _connection.Dispose();
        }

        return dbDataReaders;
    }

    /// <summary>
    /// Função que executa uma Lista de Queries junto de seus parâmetros.
    /// Tem a mesma funcionalidade do 'ExecuteNoReturnQuery'. A diferença é que ela executa uma lista de 
    /// queries antes da conexão ser fechada.
    /// Dessa maneira, uma Lista de Booleanos será retornado. A mesma regra do 'ExecuteNoReturnQuery' se aplica.
    /// </summary>
    /// <param name="queryExecution">Uma lista de Objetos que contêm a String da Query e os Parâmetros dela</param>
    /// <returns>Lista de Boolean de mesmo tamanho da entrada. True se linhas afetadas > 0</returns>
    public IDictionary<string, bool> ExecuteAllNoReturnQueries(IDictionary<string, QueryExecution> queryExecution, bool withTransaction = true)
    {
        TConnection _connection = new();
        if (Settings.config is null) throw new Exception("Algum erro ocorreu com a conexão ao servidor.");
        _connection.ConnectionString = Settings.config.GetSection("API:CONNECTION_STRING").Value;

        Dictionary<string, IDbCommand> cmdDict = new();
        Dictionary<string, bool> oracleDataReaders = new();

        _connection.Open();

        if (withTransaction)
        {
            using IDbTransaction t = _connection.BeginTransaction();
            foreach (var (key, queryInfo) in queryExecution)
            {
                IDbCommand cmd = CreateCommandFromQueryExecution(queryInfo, _connection);
                cmd.Transaction = t;

                cmdDict.Add(key, cmd);
            }
            try
            {
                foreach (var (key, oracleCommand) in cmdDict)
                {
                    QueryExecution? qeUsed;
                    if (queryExecution.TryGetValue(key, out qeUsed)) throw new Exception("Uma das queries não foi executada corretamente."); ;
                    if (qeUsed == null) throw new Exception("Uma das queries não foi executada corretamente."); ;

                    int rowsAffected = oracleCommand.ExecuteNonQuery();

                    bool result = true;

                    if (qeUsed.ExpectNoReturnEffect == true && rowsAffected == 0) result = false;

                    oracleDataReaders.Add(key, result);
                }
                t.Commit();
            }
            catch (Exception ex)
            {
                t.Rollback();
                throw new Exception("Algum erro acontenceu na execução da query. " + ex.Message);
            }
        }
        else
        {
            foreach (var (key, queryInfo) in queryExecution)
            {
                IDbCommand cmd = CreateCommandFromQueryExecution(queryInfo, _connection);
                cmdDict.Add(key, cmd);
            }
            try
            {
                foreach (var (key, oracleCommand) in cmdDict)
                {
                    QueryExecution? qeUsed;
                    if (queryExecution.TryGetValue(key, out qeUsed)) throw new Exception("Uma das queries não foi executada corretamente."); ;
                    if (qeUsed == null) throw new Exception("Uma das queries não foi executada corretamente."); ;

                    int rowsAffected = oracleCommand.ExecuteNonQuery();

                    bool result = true;

                    if (qeUsed.ExpectNoReturnEffect == true && rowsAffected == 0) result = false;

                    oracleDataReaders.Add(key, result);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Algum erro acontenceu na execução da query. " + ex.Message);
            }
        }

        if (_connection != null && _connection.State == ConnectionState.Open)
        {
            _connection.Close();
            _connection.Dispose();
        }

        return oracleDataReaders;
    }
}
