using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GEngine.Game;

namespace GEngine.Engine
{
    public class GameObjectCollection : ICollection<GameObject>
    {
        private ICollection<GameObject> _data;

        public GameObjectCollection()
        {
            _data = new List<GameObject>();
        }

        public int Count => _data.Count;

        public bool IsReadOnly => _data.IsReadOnly;

        public void Add(GameObject item)
        {
            _data.Add(item);
        }

        public void Clear()
        {
            _data.Clear();
        }

        public bool Contains(GameObject item)
        {
            return _data.Contains(item);
        }

        public void CopyTo(GameObject[] array, int arrayIndex)
        {
            _data.CopyTo(array, arrayIndex);
        }

        public IEnumerator<GameObject> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        public bool Remove(GameObject item)
        {
            return _data.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }
    }
    public class ResourceCollection : ICollection<ResourceBase>
    {
        private ICollection<ResourceBase> _data;

        public ResourceCollection()
        {
            _data = new List<ResourceBase>();
        }

        public int Count => _data.Count;

        public bool IsReadOnly => _data.IsReadOnly;

        public void Add(ResourceBase item)
        {
            _data.Add(item);
        }

        public void Clear()
        {
            _data.Clear();
        }

        public bool Contains(ResourceBase item)
        {
            return _data.Contains(item);
        }

        public bool Contains(string resourceName)
        {
            foreach (var data in _data)
            {
                if (data.ResourceName == resourceName)
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(ResourceBase[] array, int arrayIndex)
        {
            _data.CopyTo(array, arrayIndex);
        }

        public IEnumerator<ResourceBase> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        public bool Remove(ResourceBase item)
        {
            return _data.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }
    }
}
