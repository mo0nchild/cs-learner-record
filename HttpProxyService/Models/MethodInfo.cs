using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HttpProxyService.Models
{
    [TableAttribute("methodinfo", Schema = "public")]
    public partial class MethodInfo : object
    {
        [KeyAttribute, Required]
        public int MethodInfoId { get; set; } = default!;

        [MaxLengthAttribute(100, ErrorMessage = "Неверно указанный путь метода")]
        public string RequestPath { get; set; } = default!;

        [MaxLengthAttribute(50, ErrorMessage = "Неверное указаное имя метода")]
        public string Name { get; set; } = default!;

        public List<AccessLog> AccessLogs { get; set; } = new();
    }
}
