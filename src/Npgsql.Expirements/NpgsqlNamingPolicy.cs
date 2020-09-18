using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Npgsql.Expirements
{
    public abstract class NpgsqlNamingPolicy
    {
        public abstract string ConvertName(string name);
    }
}
