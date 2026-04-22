using System;
using System.Collections.Generic;
using backend.Parser;

namespace backend.Search
{
    public class LcaBinaryLifting
    {
        private int _timer;
        private readonly int _l;
        private readonly Dictionary<HtmlNode, int> _tin;
        private readonly Dictionary<HtmlNode, int> _tout;
        private readonly Dictionary<HtmlNode, HtmlNode[]> _up;

        public LcaBinaryLifting(HtmlNode root, int nodeCount)
        {
            _tin = new Dictionary<HtmlNode, int>();
            _tout = new Dictionary<HtmlNode, int>();
            _up = new Dictionary<HtmlNode, HtmlNode[]>();
            _timer = 0;
            
            _l = (int)Math.Ceiling(Math.Log2(nodeCount > 0 ? nodeCount : 1));
            
            Dfs(root, root);
        }

        private void Dfs(HtmlNode v, HtmlNode p)
        {
            _tin[v] = ++_timer;
            
            _up[v] = new HtmlNode[_l + 1];
            _up[v][0] = p;
            
            for (int i = 1; i <= _l; i++)
            {
                var halfAncestor = _up[v][i - 1];
                _up[v][i] = _up[halfAncestor][i - 1];
            }

            foreach (var u in v.Children)
            {
                if (u != p)
                {
                    Dfs(u, v);
                }
            }

            _tout[v] = ++_timer;
        }

        private bool IsAncestor(HtmlNode u, HtmlNode v)
        {
            return _tin[u] <= _tin[v] && _tout[u] >= _tout[v];
        }

        public HtmlNode FindLCA(HtmlNode u, HtmlNode v)
        {
            if (IsAncestor(u, v))
                return u;
            
            if (IsAncestor(v, u))
                return v;

            for (int i = _l; i >= 0; i--)
            {
                if (!IsAncestor(_up[u][i], v))
                {
                    u = _up[u][i];
                }
            }

            return _up[u][0];
        }
    }
}