using System;
using System.Collections.Generic;
using System.Data;
using AdFactum.Data.Internal;
using AdFactum.Data.Repository;

namespace AdFactum.Data.SqlServer
{
    /// <summary>
    /// That's a abstract class for all microsoft based persisters
    /// </summary>
    public abstract class MicrosoftBasedPersister : BasePersister
    {

        /// <summary>
        /// Retrieves the last auto increment Id
        /// </summary>
        /// <returns></returns>
        protected override int SelectLastAutoId(string tableName)
        {
            int autoId = -1;
            IDbCommand command = CreateCommand();
            command.CommandText = "SELECT @@IDENTITY";

            IDataReader reader = ExecuteReader(command);
            if (reader.Read())
            {
                object lastId = reader.GetValue(0);
                if (lastId != DBNull.Value)
                    autoId = (int)ConvertSourceToTargetType(reader.GetValue(0), typeof(Int32));
            }
            reader.Close();
            command.Dispose();

            return autoId;
        }

    }
}