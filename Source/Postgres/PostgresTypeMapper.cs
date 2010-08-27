using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AdFactum.Data.Internal;
using AdFactum.Data.Util;
using NpgsqlTypes;

namespace AdFactum.Data.Postgres
{
    public class PostgresTypeMapper : BaseTypeMapper
    {
        private SqlCasing sqlCasing = SqlCasing.Mixed;

        #region Keywords

        /// <summary>
        /// Postgres Keywords
        /// </summary>
        internal static readonly HashSet<string> Keywords = new HashSet<string>
        {
            "abort","abs","absolute","access","action","ada","add","admin","after","aggregate","alias","all","allocate","also",
            "alter","always","analyse","analyze","and","any","are","array","as","asc","asensitive","assertion","assignment","asymmetric","at",
            "atomic","attribute","attributes","authorization","avg","backward","before","begin","bernoulli","between","bigint","binary","bit",
            "bitvar","bit_length","blob","boolean","both","breadth","by","c","cache","call","called","cardinality","cascade","cascaded","case", 
            "cast","catalog","catalog_name","ceil","ceiling","chain","char","character","characteristics","characters","character_length","character_set_catalog",
            "character_set_name","character_set_schema","char_length","check","checked","checkpoint","class","class_origin","clob",
            "close","cluster","coalesce","cobol","collate","collation","collation_catalog","collation_name","collation_schema","collect",
            "column","column_name","command_function","command_function_code","comment","commit","committed","completion","condition","condition_number",
            "connect","connection","connection_name","constraint","constraints","constraint_catalog","constraint_name","constraint_schema",
            "constructor","contains","continue","conversion","convert","copy","corr","corresponding","count","covar_pop","covar_samp",
            "create","createdb","createrole","createuser","cross","csv","cube","cume_dist","current","current_date","current_default_transform_group",
            "current_path","current_role","current_time","current_timestamp","current_transform_group_for_type","current_user","cursor","cursor_name",
            "cycle","data","database","date","datetime_interval_code","datetime_interval_precision","day","deallocate","dec","decimal","declare",
            "default","defaults","deferrable","deferred","defined","definer","degree","delete","delimiter","delimiters","dense_rank",
            "depth","deref","derived","desc","describe","descriptor","destroy","destructor","deterministic","diagnostics","dictionary","disable",
            "disconnect","dispatch","distinct","do","domain","double","drop","dynamic","dynamic_function","dynamic_function_code",
            "each","element","else","enable","encoding","encrypted","end","end-exec","equals","escape","every","except","exception","exclude",
            "excluding","exclusive","exec","execute","existing","exists","exp","explain","external","extract","false","fetch","filter","final",
            "first","float","floor","following","for","force","foreign","fortran","forward","found","free","freeze","from","full","function","fusion",
            "general","generated","get","global","go","goto","grant","granted","greatest","group","grouping","handler","having","header","hierarchy",
            "hold","host","hour","identity","ignore","ilike","immediate","immutable","implementation","implicit","in","including","increment",
            "index","indicator","infix","inherit","inherits","initialize","initially","inner","inout","input","insensitive","insert","instance","instantiable",
            "instead","int","integer","intersect","intersection","interval","into","invoker","is","isnull","isolation","iterate","join",
            "key","key_member","key_type","lancompiler","language","large","last","lateral","leading","least","left","length","less","level","like",
            "limit","listen","ln","load","local","localtime","localtimestamp","location","locator","lock","login","lower","map","match","matched",
            "max","maxvalue","member","merge","message_length","message_octet_length","message_text","method","min","minute","minvalue",
            "mod","mode","modifies","modify","module","month","more","move","multiset","mumps","name","names","national","natural","nchar","nclob",
            "nesting","new","next","no","nocreatedb","nocreaterole","nocreateuser","noinherit","nologin","none","normalize","normalized","nosuperuser",
            "not","nothing","notify","notnull","nowait","null","nullable","nullif","nulls","number","numeric","object","octets","octet_length","of",
            "off","offset","oids","old","on","only","open","operation","operator","option","options","or","order","ordering","ordinality","others",
            "out","outer","output","over","overlaps","overlay","overriding","owner","pad","parameter","parameters","parameter_mode","parameter_name",
            "parameter_ordinal_position","parameter_specific_catalog","parameter_specific_name","parameter_specific_schema","partial",
            "partition","pascal","password","path","percentile_cont","percentile_disc","percent_rank","placing","pli","position","postfix","power",
            "preceding","precision","prefix","preorder","prepare","prepared","preserve","primary","prior","privileges","procedural","procedure",
            "public","quote","range","rank","read","reads","real","recheck","recursive","ref","references","referencing","regr_avgx","regr_avgy","regr_count",
            "regr_intercept","regr_r2","regr_slope","regr_sxx","regr_sxy","regr_syy","reindex","relative","release","rename","repeatable","replace",
            "reset","restart","restrict","result","return","returned_cardinality","returned_length","returned_octet_length","returned_sqlstate","returns",
            "revoke","right","role","rollback","rollup","routine","routine_catalog","routine_name","routine_schema","row","rows","row_count","row_number",
            "rule","savepoint","scale","schema","schema_name","scope","scope_catalog","scope_name","scope_schema","scroll","search","second","section",
            "security","select","self","sensitive","sequence","serializable","server_name","session","session_user","set","setof","sets","share","show",
            "similar","simple","size","smallint","some","source","space","specific","specifictype","specific_name","sql","sqlcode","sqlerror","sqlexception",
            "sqlstate","sqlwarning","sqrt","stable","start","state","statement","static","statistics","stddev_pop","stddev_samp","stdin","stdout","storage",
            "strict","structure","style","subclass_origin","sublist","submultiset","substring","sum","superuser","symmetric","sysid","system","system_user",
            "table","tablesample","tablespace","table_name","temp","template","temporary","terminate","than","then","ties","time","timestamp","timezone_hour",
            "timezone_minute","to","toast","top_level_count","trailing","transaction","transactions_committed","transactions_rolled_back","transaction_active",
            "transform","transforms","translate","translation","treat","trigger","trigger_catalog","trigger_name","trigger_schema","trim","true",
            "truncate","trusted","type","uescape","unbounded","uncommitted","under","unencrypted","union","unique","unknown","unlisten","unnamed","unnest",
            "until","update","upper","usage","user","user_defined_type_catalog","user_defined_type_code","user_defined_type_name","user_defined_type_schema",
            "using","vacuum","valid","validator","value","values","varchar","variable","varying","var_pop","var_samp","verbose","view","volatile",
            "when","whenever","where","width_bucket","window","with","within","without","work","write","year","zone"
        };

        #endregion

        public PostgresTypeMapper()
        {
            DbMappingTable.Add(typeof(Boolean), "boolean");
            DbMappingTable.Add(typeof(Byte), "smallint");
            DbMappingTable.Add(typeof(DateTime), "timestamp");
            DbMappingTable.Add(typeof(Decimal), "real");
            DbMappingTable.Add(typeof(Double), "double precision");
            DbMappingTable.Add(typeof(Guid), "uuid");
            DbMappingTable.Add(typeof(Int16), "smallint");
            DbMappingTable.Add(typeof(Int32), "integer");
            DbMappingTable.Add(typeof(Int64), "bigint");
            DbMappingTable.Add(typeof(Single), "real");
            DbMappingTable.Add(typeof(String), "varchar({0})");
            DbMappingTable.Add(typeof(TimeSpan), "interval");
            DbMappingTable.Add(typeof(Enum), "enum");
            DbMappingTable.Add(typeof(Stream), "bytea");
            DbMappingTable.Add(typeof(Byte[]), "bytea");
            DbMappingTable.Add(typeof(Char), "char(1)");

            SqlMappingTable.Add(typeof(Boolean), (int)NpgsqlDbType.Boolean);
            SqlMappingTable.Add(typeof(Byte), (int)NpgsqlDbType.Smallint);
            SqlMappingTable.Add(typeof(DateTime), (int)NpgsqlDbType.Timestamp);
            SqlMappingTable.Add(typeof(Decimal), (int)NpgsqlDbType.Real);
            SqlMappingTable.Add(typeof(Double), (int)NpgsqlDbType.Double);
            SqlMappingTable.Add(typeof(Guid), (int)NpgsqlDbType.Uuid);
            SqlMappingTable.Add(typeof(Int16), (int)NpgsqlDbType.Smallint);
            SqlMappingTable.Add(typeof(Int32), (int)NpgsqlDbType.Integer);
            SqlMappingTable.Add(typeof(Int64), (int)NpgsqlDbType.Bigint);
            SqlMappingTable.Add(typeof(Single), (int)NpgsqlDbType.Real);
            SqlMappingTable.Add(typeof(String), (int)NpgsqlDbType.Varchar);
            SqlMappingTable.Add(typeof(TimeSpan), (int)NpgsqlDbType.Interval);
            SqlMappingTable.Add(typeof(Enum), (int)NpgsqlDbType.Varchar);
            SqlMappingTable.Add(typeof(Stream), (int)NpgsqlDbType.Bytea);
            SqlMappingTable.Add(typeof(Byte[]), (int)NpgsqlDbType.Bytea);
            SqlMappingTable.Add(typeof(Char), (int)NpgsqlDbType.Char);
        }

        /// <summary>
        /// Gets the string for DDL.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        public override string GetStringForDDL(FieldDescription field)
        {
            String result = "bytea";

            Type type = PreTransformTypeForDDL(field);
            int length = field.CustomProperty.MetaInfo.Length;

            /*
             * Strings that are greater than 4000 characters are handled as MEMOS
             */
            if (type.Equals(typeof(string)) && length > 4000)
                return "text";

            /*
             * Enum Types are supported by Postgres, so why don't use it ;)
             */
            if (type.IsEnum)
                return AddDefaultToDDL(field, Quote(type.Name));

            result = base.GetStringForDDL(field, type, length) ?? result;
            return result;
        }

        /// <summary>
        /// Gets the enum for database.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="isUnicode">if set to <c>true</c> [is unicode].</param>
        /// <returns></returns>
        public override Enum GetEnumForDatabase(Type type, bool isUnicode)
        {
            NpgsqlDbType result = NpgsqlDbType.Bytea;
            type = TypeHelper.GetBaseType(type);

            if (SqlMappingTable.ContainsKey(type))
                result = (NpgsqlDbType)SqlMappingTable[type];
            else
            {
                /*
                 * Wenn keine direkte Zuordnung möglich ist, dann nach Implementierungen suchen
                 */
                foreach (var dict in SqlMappingTable)
                {
                    if (type.IsDerivedFrom(dict.Key))
                    {
                        result = (NpgsqlDbType)dict.Value;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the auto increment identifier.
        /// </summary>
        /// <value>The auto increment identifier.</value>
        public override string AutoIncrementIdentifier
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Convert every quoted identifier to lower
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public override string Quote(string expression)
        {
            string[] parts = expression.Split(',');
            string result = string.Empty;
            for (int x = 0; x < parts.Length; x++)
            {
                string trim = parts[x].Trim();

                var lowerInvariant = expression.ToLowerInvariant();
                bool isKeyWord = lowerInvariant.IndexOfAny(new[] { ' ', '#' }) >= 0 || Keywords.Contains(lowerInvariant);
                bool containsUpperCase = lowerInvariant != expression;

                result = string.Concat(result, (x > 0 ? "," : ""), isKeyWord || containsUpperCase ? string.Concat("\"", trim, "\"") : trim);
            }

            return result;
        }

        /// <summary>
        /// Returns the SQL Casing of the used Database.
        /// </summary>
        public override SqlCasing SqlCasing
        {
            get { return sqlCasing; }
        }

        /// <summary>
        /// Sets the SQL casing.
        /// </summary>
        /// <param name="casing">The casing.</param>
        public void SetSqlCasing(SqlCasing casing)
        {
            sqlCasing = casing;
        }

        /// <summary>
        /// Gets a value indicating whether [parameter duplication].
        /// </summary>
        /// <value><c>true</c> if [parameter duplication]; otherwise, <c>false</c>.</value>
        public override bool ParameterDuplication
        {
            get { return true;}
        }


        /// <summary>
        /// Convert a paramter value to the valid string format
        /// </summary>
        public override string GetParamValueAsSQLString(object parameterValue)
        {
            // Boolean values can be returned as a string (not 0 or 1)
            if (parameterValue is bool)
                return (bool)parameterValue ? "true" : "false";

            // Enums can be evaluated directly
            if (parameterValue != null && parameterValue.GetType().IsEnum)
                return parameterValue.ToString();

            return base.GetParamValueAsSQLString(parameterValue);
        }

        /// <summary>
        /// Converts a type to the typed used from the current database implementation
        /// </summary>
        /// <param name="checkType">Type of the check.</param>
        /// <returns></returns>
        public override Type GetTypeForDatabase(Type checkType)
        {
            if (checkType == typeof(byte))
                return typeof (Int16);

            if (checkType == typeof(decimal))
                return typeof(Single);

            if (checkType == typeof(TimeSpan))
                return typeof (NpgsqlInterval);

            return base.GetTypeForDatabase(checkType);
        }

        /// <summary>
        /// Converts the source to a database specific type and returns it.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public override object ConvertValueToDbType(object value)
        {
            if (value is TimeSpan)
                return new NpgsqlInterval((TimeSpan)value);

            if (value is DateTime)
            {
                var dt = (DateTime)value;
                return new NpgsqlTimeStamp(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
            }

            if (value is Enum) 
                return value;

            return base.ConvertValueToDbType(value);
        }

        /// <summary>
        /// Convert the given value to the specific type.
        /// </summary>
        /// <param name="returnType">Type of the return.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public override object ConvertToType(Type returnType, object value)
        {
            if (returnType.Equals(typeof(TimeSpan)))
            {
                if (value is NpgsqlInterval)
                    return ((NpgsqlInterval)value).Time;
            }                

            return base.ConvertToType(returnType, value);
        }
    }
}
