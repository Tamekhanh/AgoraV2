using System;
using System.Collections.Generic;
using System.Text;

namespace Agora.Application.Common;

public class PagedRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
}

