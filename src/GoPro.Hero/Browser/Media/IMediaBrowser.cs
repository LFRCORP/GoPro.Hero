﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoPro.Hero.Browser.Media
{
    public interface IMediaBrowser:IGeneralBrowser
    {
        Task<IMedia> ContentAsync(string name);
        IMedia Content(string name);

        Task<IEnumerable<IMedia>> ContentsAsync();
        IEnumerable<IMedia> Contents();
    }
}
