using Oracle.ManagedDataAccess.Client;

namespace BlazorCleanRelease.Components.Classes
{
    public class DatabaseProcessingClass
    {
        public int GetNextKey(OracleConnection connection, string scheme = "test")
        {
            using (OracleCommand command = connection.CreateCommand())
            {
                command.CommandText = $"SELECT S_INCKEY FROM {scheme}.SPR_SPEECH_TABLE WHERE S_INCKEY=(SELECT max(S_INCKEY) FROM {scheme}.SPR_SPEECH_TABLE)";
                var result = command.ExecuteScalar();
                return result != DBNull.Value ? Convert.ToInt32(result) + 1 : 1;
            }
        }
        public void InsertIntoCommentTable(OracleConnection connection, OracleTransaction transaction, string scheme, int key, byte[] comment)
        {
            //if (IsKeyExists(connection, transaction, scheme, key))
            {
                //Если комментарий для этой записи есть - устанавливается в {comment}
                using (OracleCommand updateCommand = connection.CreateCommand())
                {
                    updateCommand.Transaction = transaction;
                    updateCommand.CommandText = $"MERGE INTO {scheme}.SPR_SP_COMMENT_TABLE T1 " +
                            $"USING (SELECT S_INCKEY FROM {scheme}.SPR_SPEECH_TABLE WHERE S_INCKEY = :S_INCKEY) T2 " +
                            $"ON (T1.S_INCKEY = T2.S_INCKEY) " +
                            $"WHEN MATCHED THEN " +
                            $"UPDATE SET S_COMMENT = :S_COMMENT " +
                            $"WHEN NOT MATCHED THEN INSERT(S_INCKEY, S_COMMENT) VALUES(:S_INCKEY, :S_COMMENT)";

                    updateCommand.Parameters.Add(":S_INCKEY", OracleDbType.Int32).Value = key;
                    updateCommand.Parameters.Add(":S_COMMENT", OracleDbType.Blob).Value = comment;
                    updateCommand.ExecuteNonQuery();

                }
            }
            /*else
            {
                // Вставка новой записи
                using (OracleCommand insertCommand = connection.CreateCommand())
                {
                    insertCommand.Transaction = transaction;
                    insertCommand.CommandText = $"INSERT INTO {scheme}.SPR_SP_COMMENT_TABLE (S_INCKEY, S_COMMENT) VALUES (:S_INCKEY, :S_COMMENT)";
                    insertCommand.Parameters.Add(":S_INCKEY", OracleDbType.Int32).Value = key;
                    insertCommand.Parameters.Add(":S_COMMENT", OracleDbType.Blob).Value = comment;
                    insertCommand.ExecuteNonQuery();
                }
            }*/

        }
        public bool IsKeyExists(OracleConnection connection, OracleTransaction transaction, string scheme, int key)
        {
            using (OracleCommand selectCommand = connection.CreateCommand())
            {
                selectCommand.Transaction = transaction;
                selectCommand.CommandText = $"SELECT COUNT(*) FROM {scheme}.SPR_SP_COMMENT_TABLE WHERE S_INCKEY = :S_INCKEY";
                selectCommand.Parameters.Add(":S_INCKEY", OracleDbType.Int32).Value = key;

                int count = Convert.ToInt32(selectCommand.ExecuteScalar());
                return count > 0;
            }
        }
    }

}
