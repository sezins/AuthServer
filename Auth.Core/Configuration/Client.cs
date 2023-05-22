using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Core.Configuration
{
    public class Client
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public List<string> Audiences { get; set; }
    }
}
