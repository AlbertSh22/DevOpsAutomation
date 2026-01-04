using System.Runtime.Serialization;

namespace WinSvc.Models
{
    /// <summary>
    /// Info about each parameter of the stored procedure.
    /// </summary>
    [DataContract]
    public class SysParam
    {
        /// <summary>
        /// Name of the parameter.
        /// </summary>
        [DataMember]
        public string ParameterName { get; set; }

        /// <summary>
        /// Name of the type, of the parameter as defined by the user.
        /// </summary>
        [DataMember]
        public string DataType { get; set; }

        /// <summary>
        /// Maximum length of the parameter, in bytes.
        /// </summary>
        [DataMember]
        public short MaxLength { get; set; }

        /// <summary>
        /// 1 = Parameter is OUTPUT or RETURN; otherwise, 0.
        /// </summary>
        [DataMember]
        public bool IsOutput { get; set; }

        /// <summary>
        /// ID of the parameter. Param identification number.
        /// </summary>
        [DataMember]
        public int ParameterId { get; set; }
    }
}
