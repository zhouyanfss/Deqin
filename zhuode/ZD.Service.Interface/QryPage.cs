using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ZD.Service.Interface
{
    [DataContract]
    public class QryPage
    {
        /// <summary>
        /// in parameter
        /// How many items for each page
        /// </summary>
        [DataMember]
        public int PerPageSize;

        /// <summary>
        /// in parameter
        /// the number of page to query
        /// </summary>
        [DataMember]
        public int PageNumber;

        /// <summary>
        /// how many pages for the query
        /// out parameter
        /// </summary>
        [DataMember]
        public int PageCount;

        /// <summary>
        /// out parameter
        /// total items of the query result
        /// </summary>
        [DataMember]
        public int TotalCount;
    }

    [DataContract]
    public class QrySort
    {
        [DataMember]
        public string Field { get; set; }

        [DataMember]
        public bool Asc { get; set; }
    }

    [DataContract]
    public class QueryResult<TEntity>
    {
        [DataMember]
        public IList<TEntity> Entities;
        [DataMember]
        public QryPage PageHint;
    }
}
