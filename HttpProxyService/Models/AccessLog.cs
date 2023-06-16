using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpProxyService.Models
{
    [TableAttribute(name: "accesslogs", Schema = "public")]
    public partial class AccessLog : object
    {
        [KeyAttribute, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AccessLogId { get; set; } = default!;

        [RequiredAttribute]
        public string LogData { get; set; } = default!;

        [MaxLength(100, ErrorMessage = "Неверно указан код запроса")]
        public string LogName { get; set; } = default!;

        public DateTime AccessTime { get; set; } = default!;

        [RequiredAttribute]
        public int MethodInfoId { get; set; } = default!;

        [ForeignKeyAttribute(name: "MethodInfoId")]
        public MethodInfo MethodInfo { get; set; } = default!;
    }
}
