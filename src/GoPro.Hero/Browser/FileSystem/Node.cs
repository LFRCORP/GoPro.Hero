﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GoPro.Hero.Browser.FileSystem
{
    public class Node
    {
        private readonly IFileSystemBrowser _browser;

        private Node(ICamera camera, Uri address, IFileSystemBrowser browser)
            : this(camera, address, NodeType.Root, string.Empty, browser)
        {
            Type = address.AbsolutePath == "/"
                       ? NodeType.Root
                       : _browser.IsFile(address)
                             ? NodeType.File
                             : NodeType.Folder;
        }

        internal Node(ICamera camera, Uri address, NodeType type, string size, IFileSystemBrowser browser)
        {
            _browser = browser;
            Camera = camera;
            Path = address;
            Type = type;
            Size = size;

            var segments = Path.AbsolutePath.Split('/');
            Name = string.IsNullOrEmpty(segments.Last()) ? segments[segments.Length - 2] : segments.Last();
        }

        public ICamera Camera { get; private set; }
        public string Name { get; private set; }
        public Uri Path { get; private set; }
        public NodeType Type { get; private set; }
        public string Size { get; private set; }

        public Node this[string name]
        {
            get { return Child(name); }
        }

        public long SizeAsBytes()
        {
            var type = Size[Size.Length - 1];
            double value = -1;

            var res = double.TryParse(Size.Substring(0, Size.Length - 1),out value);
            if (!res)
                return -1;

            switch (type)
            {
                case 'M':
                    return (long)(value * 1024 * 1024);
                case 'K':
                    return (long)(value * 1024);
                default:
                    return -1;
            }
        }

        public string Extension()
        {
            return Name.Split('.').LastOrDefault();
        }

        public string NameWithoutExtension()
        {
            return Name.Split('.').First();
        }

        public IEnumerable<Node> Children(string name)
        {
            return ChildrenAsync(name).Result;
        }

        public async Task<IEnumerable<Node>> ChildrenAsync(string name)
        {
            return (await NodesAsync()).Where(n => n.Name == name);
        }

        public Node Child(string name)
        {
            return ChildAsync(name).Result;
        }

        public async Task<Node> ChildAsync(string name)
        {
            return (await this.ChildrenAsync(name)).FirstOrDefault();
        }

        public IEnumerable<Node> Nodes()
        {
            return _browser.Nodes(this);
        }

        public async Task<IEnumerable<Node>> NodesAsync()
        {
            return await _browser.NodesAsync(this);
        }

        public IEnumerable<Node> Folders()
        {
            return FoldersAsync().Result;
        }

        public async Task<IEnumerable<Node>> FoldersAsync()
        {
            return (await _browser.NodesAsync(this)).Where(node => node.Type == NodeType.Folder);
        }

        public IEnumerable<Node> Files()
        {
            return FilesAsync().Result;
        }

        public async Task<IEnumerable<Node>> FilesAsync()
        {
            return (await _browser.NodesAsync(this)).Where(node => node.Type == NodeType.File);
        }

        public WebResponse DownloadContent()
        {
            return _browser.DownloadContent(this);
        }

        public async Task<WebResponse> DownloadContentAsync()
        {
            return await _browser.DownloadContentAsync(this);
        }

        public override string ToString()
        {
            return Name;
        }

        public static Node Create<TB>(ICamera camera, Uri address) where TB : IFileSystemBrowser
        {
            var browser = Activator.CreateInstance<TB>();
            browser.Initialize(camera, address);

            var node = new Node(camera, address, browser);
            return node;
        }

        public static Node Create(IFileSystemBrowser browser)
        {
            var node = new Node(browser.Camera, browser.Address, browser);
            return node;
        }
    }
}