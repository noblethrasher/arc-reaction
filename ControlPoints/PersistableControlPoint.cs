using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prelude;
using System.Data.SqlClient;
using System.Data;

namespace ArcReaction.ControlPoints
{
    abstract class PersistableControlPoint
    {
        static PersistableControlPoint()
        {
            if (!((PrivledgedSql)"select table_name from information_schema.tables where table_name = 'PersistedControlPoint'").Any())
                new BackTableCheck().Execute();
        }

        sealed class BackTableCheck : Prelude.PrivledgedProc<int>
        {
            new const string sql = "create table PersistedControlPoint ( id varchar(50) primary key, created as getdate() )";

            public BackTableCheck() : base(sql) { }
            
            protected override int _Execute(SqlCommand command)
            {
                return command.ExecuteNonQuery();
            }
        }

        readonly string id;

        public PersistableControlPoint(IDataReader rdr)
        {
            id = rdr.GetString("id");
        }


    }
}
