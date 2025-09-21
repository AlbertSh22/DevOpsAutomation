namespace WinSvc.Models
{
    internal class SysParam
    {
        public string ParameterName { get; set; }

        public string DataType { get; set; }

        public short MaxLength { get; set; }

        public bool IsOutput { get; set; }
    }
}
