using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WinSvc.Models
{
    /// <summary>
    /// Return data using result sets.
    /// </summary>
    [DataContract]
    public class SpResult
    {
        /// <summary>
        /// An integer value to indicate sp execution status.
        /// </summary>
        [DataMember]
        public int ReturnValue { get; set; }

        /// <summary>
        /// Parameter map for returning values to the caller of the procedure.
        /// </summary>
        [DataMember]
        public Dictionary<string, object> OutputParams { get; set; }

        /// <summary>
        /// Result set column names.
        /// </summary>
        [DataMember]
        public string[] Columns { get; set; }

        /// <summary>
        /// Result set.
        /// </summary>
        [DataMember]
        public object[][] DataSet { get; set; }
    }
}
