namespace WinSvc.Helpers
{
    internal static class QueryBuilder
    {
        internal static string ComposeGetSpParams(string spName)
        {
            var sql = $@"select [name] ParameterName, TYPE_NAME(user_type_id) DataType, max_length AS MaxLength, is_output IsOutput 
from sys.parameters 
where object_id = OBJECT_ID(N'{spName}', N'P')
order by parameter_id";

            return sql;
        }
    }
}
