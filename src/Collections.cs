using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GEngine.Game;

namespace GEngine.Engine
{
    public struct GameObjectInfoPair
    {
        public GameObject GameObject;
        public Coord Position;
    }
    public class GameObjectInfoCollection : ICollection<GameObjectInfoPair>
    {
        private ICollection<GameObjectInfoPair> _data;

        public GameObjectInfoCollection()
        {
            _data = new List<GameObjectInfoPair>();
        }

        public int Count => _data.Count;

        public bool IsReadOnly => _data.IsReadOnly;

        public void Add(GameObject obj, int x, int y)
        {
            _data.Add(new GameObjectInfoPair()
            {
                GameObject = obj,
                Position = new Coord(x, y)
            });
        }
        public void Add(GameObject obj, Coord position)
        {
            _data.Add(new GameObjectInfoPair()
            {
                GameObject = obj,
                Position = position
            });
        }

        public void Add(GameObjectInfoPair item)
        {
            _data.Add(item);
        }

        public void Clear()
        {
            _data.Clear();
        }

        public bool Contains(GameObjectInfoPair item)
        {
            return _data.Contains(item);
        }

        public void CopyTo(GameObjectInfoPair[] array, int arrayIndex)
        {
            _data.CopyTo(array, arrayIndex);
        }

        public IEnumerator<GameObjectInfoPair> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        public bool Remove(GameObjectInfoPair item)
        {
            return _data.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        public GameObjectInfoPair Get(string name)
        {
            foreach (GameObjectInfoPair go in _data)
            {
                if (go.GameObject.ObjectName == name) return go;
            }
            throw new EngineException($"GameObject with name '{name}' not found in collection.", "GameObjectInfoCollection.Get()");
        }

        public GameObjectInfoPair Get(GameObject obj)
        {
            foreach (GameObjectInfoPair go in _data)
            {
                if (go.GameObject == obj) return go;
            }
            throw new EngineException($"GameObject with object '{obj}' not found in collection.", "GameObjectInfoCollection.Get()");
        }
    }
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

        public GameObject Get(string name)
        {
            foreach (GameObject go in _data)
            {
                if (go.ObjectName == name) return go;
            }
            throw new EngineException($"GameObject with name '{name}' not found in collection.", "GameObjectCollection.Get()");
        }
    }
    public class InstanceCollection : ICollection<Instance>
    {
        private ICollection<Instance> _data;

        public InstanceCollection()
        {
            _data = new List<Instance>();
        }

        public Instance this[int i]
        {
            get
            {
                return this[i];
            }
        }
        public bool IsSorted
        {
            get
            {
                return true;
            }
        }

        public int Count => _data.Count;

        public bool IsReadOnly => _data.IsReadOnly;

        public void Add(Instance item)
        {
            _data.Add(item);
            //add quicksort implementation on instance depth
            //ascending order where top is the least greatest
        }

        public void Clear()
        {
            _data.Clear();
        }

        public bool Contains(Instance item)
        {
            return _data.Contains(item);
        }

        public void CopyTo(Instance[] array, int arrayIndex)
        {
            _data.CopyTo(array, arrayIndex);
        }

        public IEnumerator<Instance> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        public bool Remove(Instance item)
        {
            return _data.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        public Instance Get(string name)
        {
            foreach (Instance go in _data)
            {
                if (go.Reference.ObjectName == name) return go;
            }
            throw new EngineException($"Instance of GameObject '{name}' not found in collection.", "InstanceCollection.Get()");
        }

        public Instance Get(Guid hash)
        {
            foreach (Instance go in _data)
            {
                if (go.Hash == hash) return go;
            }
            throw new EngineException($"Instance with hash '{hash}' not found in collection.", "InstanceCollection.Get()");
        }

        public void SortByDepth()
        {
            if (IsSorted) return;

            Instance[] instances = new Instance[this.Count];

            this.CopyTo(instances, 0);

            this.Clear();

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

        public ResourceBase Get(string name)
        {
            foreach (ResourceBase res in _data)
            {
                if (res.ResourceName == name) return res;
            }
            throw new EngineException($"Resource with name '{name}' not found in collection.", "ResourceCollection.Get()");
        }
    }
}
