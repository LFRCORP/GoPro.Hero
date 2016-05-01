﻿using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace GoPro.Hero.Browser.Media
{
    public class TimeLapsedImage:Media<TimeLapsedImageParameters>,IEnumerable<WebResponse>
    {
        public int Group { get; private set; }
        public int Start { get; private set; }
        public int End { get; private set; }
        public int Count { get { return this.End + 1 - this.Start; } }

        public string IndexName(int index)
        {
            var name = string.Format("G{0:000}{1:0000}.JPG", Group, index);
            return name;
        }

        public async Task<WebResponse> DownloadAsync(int index)
        {
            var indexName = IndexName(index);
            return await DownloadAsync(indexName);
        }

        protected sealed override void Initiaize(TimeLapsedImageParameters token, IMediaBrowser browser)
        {
            base.Initiaize(token, browser);

            Start = token.Start;
            End = token.End;
            Group = token.Group;
        }

        public override string ToString()
        {
            return string.Format("timelapsed:{0}, count:{1}", base.Name,Count);
        }

        public IEnumerator<WebResponse> GetEnumerator()
        {
            return new TimeLapsedImageEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        class TimeLapsedImageEnumerator : IEnumerator<WebResponse>
        {
            public int CurrentIndex { get; private set; }
            public TimeLapsedImage Owner { get; private set; }
            public WebResponse Current{get;private set;}
            object IEnumerator.Current{get { return Current; }}

            public bool MoveNext()
            {
                CurrentIndex++;

                if (CurrentIndex > Owner.End)
                    return false;

                Current = Owner.DownloadAsync(CurrentIndex).Result;

                return true;
            }

            public void Reset()
            {
                CurrentIndex = Owner.Start;
            }

            public void Dispose()
            {

            }

            public TimeLapsedImageEnumerator(TimeLapsedImage owner)
            {
                Owner = owner;
                CurrentIndex = Owner.Start -1;
            }
        }
    }
}
