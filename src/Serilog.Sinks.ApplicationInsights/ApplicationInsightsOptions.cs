using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serilog.Sinks.ApplicationInsights
{
    public class ApplicationInsightsOptions
    {
        public bool UseJsonFormatter { get; set; } = false;

        public bool IncludeLogLevel { get; set; } = false;

        public bool IncludeRenderedMessage { get; set; } = false;

        public bool IncludeMessageTemplate { get; set; } = false;


    }
}
