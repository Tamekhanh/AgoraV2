using System;
using System.Collections.Generic;
using System.Text;

namespace Agora.Application.Common;

public class PagedResult<T>
{
    public int Total { get; set; }
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
}

