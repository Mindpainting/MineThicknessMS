using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MineralThicknessMS.config
{
    public class MySQLHelper
    {
        public static string connStr = "Database=luojiadata;Data Source=localhost;User Id=root;Password=root;pooling=false;CharSet=utf8;port=3306";

        public static void ExecSql(string sqlStr, MySqlParameter[] parameters, MySqlTransaction transaction = null)
        {
            using (var connection = new MySqlConnection(connStr))
            {
                using (var command = new MySqlCommand(sqlStr, connection))
                {
                    command.Parameters.AddRange(parameters);

                    if (transaction != null)
                        command.Transaction = transaction;

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }


        public static bool ExecSql(string sqlStr, params MySqlParameter[] para)
        {

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {

                using (MySqlCommand command = new MySqlCommand(sqlStr, conn))
                {
                    try
                    {
                        conn.Open();
                        if (para != null)
                        {
                            foreach (MySqlParameter p in para)
                            {
                                command.Parameters.AddWithValue(p.ParameterName, p.Value);
                            }
                        }
                        return command.ExecuteNonQuery() > 0;
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
            }
        }

        public static DataSet ExecSqlQuery(string sqlStr, params MySqlParameter[] para)
        {
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                using (MySqlCommand command = new MySqlCommand(sqlStr, conn))
                {
                    DataSet ds = new DataSet();
                    try
                    {
                        if (para != null)
                        {
                            foreach (MySqlParameter p in para)
                            {
                                command.Parameters.AddWithValue(p.ParameterName, p.Value);
                            }
                        }
                        MySqlDataAdapter da = new MySqlDataAdapter(command);
                        da.Fill(ds);
                        return ds;
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
        }
    }
}
