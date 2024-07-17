
//using GamesWebSocket.DAO;
//using GamesWebSocket.Models.Auth;
//using GamesWebSocket.Models.HttpIn;
//using GamesWebSocket.Models.HttpOut;
//using Microsoft.IdentityModel.Tokens;
//using Newtonsoft.Json;
//using Oracle.ManagedDataAccess.Client;
//using System.Data;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;

//namespace GamesWebSocket.ApplicationServices.Auth;

//public interface IAuthApplicationServices
//{
//    DataResponse GetToken(User_ConfidentialData_ModelDTO userConfidential);

//    User_ConfidentialData_ModelDTO? Resgatar_DadosAuth(string usuario);
//    bool Verificar_DadosAuth(LoginDTO login, User_ConfidentialData_ModelDTO userConfidential);
//    bool ResgatarEVerificar_DadosAuth(LoginDTO login);
//}

//public class AuthApplicationServices : IAuthApplicationServices
//{
//    private readonly IPersistenciaDB _persistenciaDB;

//    public AuthApplicationServices(IPersistenciaDB persistenciaDB)
//    {
//        _persistenciaDB = persistenciaDB;
//    }

//    /// <summary>
//    /// Função que pega o usuário e senha e verifica se eles correspondem.
//    /// Se o Login for válido, cria um token e retorna ele.
//    /// </summary>
//    /// <param name="usuario">Login do usuário</param>
//    /// <param name="senha">Senha do usuário</param>
//    /// <returns>Token de conexão</returns>
//    //public DataResponse GetToken(User_ConfidentialData_ModelDTO userConfidential)
//    //{
//    //    try
//    //    {
//    //        var chaveSimetrica = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("0123456789abcdef0123456789abcdef"));
//    //        var assinatura = new SigningCredentials(chaveSimetrica, SecurityAlgorithms.HmacSha256Signature); // Para testes
//    //        if (Settings.config != null)
//    //        {
//    //            string chave = Settings.config!.GetSection("Token:SecretKey").Value;
//    //            chaveSimetrica = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chave));
//    //            assinatura = new SigningCredentials(chaveSimetrica, SecurityAlgorithms.HmacSha256Signature);
//    //        }

//    //        string role = "CONSULTOR";
//    //        var claims = new List<Claim>
//    //        {
//    //            new("Usuario", userConfidential.USUARIO),
//    //            new("Nome", userConfidential.NOMECOMPLETO),
//    //            new(ClaimTypes.Email, userConfidential.ENDERECOEMAIL),
//    //            new(ClaimTypes.Role, role)
//    //        };

//    //        Authentic Dados_autenticados = new()
//    //        {
//    //            Usuario = userConfidential.USUARIO,
//    //            Token = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
//    //                issuer: "http://www.avipam.com.br",
//    //                audience: "usuario",
//    //                expires: DateTime.Now.AddHours(6),
//    //                signingCredentials: assinatura,
//    //                claims: claims
//    //            ))
//    //        };

//    //        return DataResponse.ResultadoValido(Dados_autenticados);

//    //    }
//    //    catch (Exception ex)
//    //    {
//    //        return DataResponse.ResultadoInvalido(400, ex.Message);
//    //    }
//    //}

//    //public User_ConfidentialData_ModelDTO? Resgatar_DadosAuth(string usuario)
//    //{
//    //    DataTable dtUser = _persistenciaDB.ExecuteQuery(GetUserDataQuery(usuario));
//    //    if (dtUser.Rows.Count != 1) return null;

//    //    User_ConfidentialData_ModelDTO[]? userConfidential = JsonConvert.DeserializeObject<User_ConfidentialData_ModelDTO[]>(JsonConvert.SerializeObject(dtUser));

//    //    if (userConfidential == null) return null;

//    //    return userConfidential.FirstOrDefault();
//    //}

//    ///// <summary>
//    ///// Função que verifica se o usuario e a senha correspondem com aquele que está guardado no banco de dados.
//    ///// </summary>
//    ///// <param name="Usuario"></param>
//    ///// <param name="Senha"></param>
//    ///// <returns>True se a senha estiver correspondida. Se não, false.</returns>
//    //public bool Verificar_DadosAuth(LoginDTO login, User_ConfidentialData_ModelDTO userConfidential)
//    //{
//    //    if (userConfidential.SENHA != login.Password) return false;
//    //    if (userConfidential.ACESSO_SIGH != "S") return false;

//    //    return true;
//    //}

//    //public bool ResgatarEVerificar_DadosAuth(LoginDTO login)
//    //{
//    //    User_ConfidentialData_ModelDTO? userConfidential = Resgatar_DadosAuth(login.User);
//    //    if (userConfidential == null) return false;

//    //    return Verificar_DadosAuth(login, userConfidential);
//    //}

//    internal static QueryExecution GetUserDataQuery(string usuario)
//    {
//        #region QueryInformation e SQL
//        string sql = $@"
//SELECT 
//    L.USUARIO,
//    L.SENHA,
//    L.NOMECOMPLETO,
//    L.ENDERECOEMAIL,
//    LA.DIRETORIORFQ AS ACESSO_SIGH
//FROM
//    INTRANET.LOGIN L,
//    INTRANET.LOGIN_ACESSOS LA
//WHERE
//    L.USUARIO = LA.USUARIO
//AND L.USUARIO = :USUARIO
//";
//        var vlUsuario = new OracleParameter("USUARIO", OracleDbType.Varchar2, 200) { Value = usuario };

//        OracleParameter[] param = new OracleParameter[] {
//            vlUsuario
//        };
//        #endregion

//        return QueryExecution.CreateQuery(sql, param);
//    }
//}
