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
    public class InstanceCollectionEventArgs : EventArgs
    {
        public InstanceCollectionEventType Type { get; set; }
        public Instance Reference { get; set; }
    }
    public enum InstanceCollectionEventType
    {
        AddObject,
        RemoveObject,
        AccessedObject,
        Misc
    }
    public class InstanceCollection : ICollection<Instance>
    {
        private ICollection<Instance> _data;
        private bool _allowEvent = true;
        public delegate void InstanceCollectionEventHandler(InstanceCollectionEventArgs eventArgs);
        public event InstanceCollectionEventHandler OnObjectAdd;
        public event InstanceCollectionEventHandler OnObjectRemove;
        public event InstanceCollectionEventHandler OnObjectAccess;
        public event InstanceCollectionEventHandler OnGeneralizedEvent;

        private void EventAdd(Instance inst)
        {
            if (!_allowEvent) return;
            var eArg = new InstanceCollectionEventArgs()
            {
                Reference = inst,
                Type = InstanceCollectionEventType.AddObject
            };
            OnObjectAdd?.Invoke(eArg);
            OnGeneralizedEvent?.Invoke(eArg);
        }
        private void EventRemove(Instance inst)
        {
            var eArg = new InstanceCollectionEventArgs()
            {
                Reference = inst,
                Type = InstanceCollectionEventType.RemoveObject
            };
            OnObjectRemove?.Invoke(eArg);
            OnGeneralizedEvent?.Invoke(eArg);
        }
        private void EventAccess(Instance inst)
        {
            var eArg = new InstanceCollectionEventArgs()
            {
                Reference = inst,
                Type = InstanceCollectionEventType.AccessedObject
            };
            OnObjectAccess?.Invoke(eArg);
            OnGeneralizedEvent?.Invoke(eArg);
        }

        public InstanceCollection()
        {
            _data = new List<Instance>();
        }

        public Instance this[int i]
        {
            get
            {
                return _data.ElementAt(i);
            }
        }

        public Instance this[Guid hash]
        {
            get
            {
                foreach (Instance inst in _data)
                {
                    if (inst.Hash == hash)
                        return inst;
                }
                return null;
            }
        }

        public Instance[] this[string objectName]
        {
            get
            {
                List<Instance> instances = new List<Instance>();
                foreach (Instance inst in _data)
                {
                    if ((inst.Reference as GameObject).ObjectName == objectName)
                        instances.Add(inst);
                }
                return instances.ToArray();
            }
        }

        public Instance[] this[GameObject reference]
        {
            get
            {
                List<Instance> instances = new List<Instance>();
                foreach (Instance inst in _data)
                {
                    if ((inst.Reference as GameObject) == reference)
                        instances.Add(inst);
                }
                return instances.ToArray();
            }
        }

        public bool IsSorted { get; private set; }

        public int Count => _data.Count;

        public bool IsReadOnly => _data.IsReadOnly;

        public void Add(Instance item)
        {
            _data.Add(item);
            EventAdd(item);
            if (IsSorted) IsSorted = false;
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

        public void SortByDepth(bool force)
        {
            if (IsSorted && !force) return;
            _allowEvent = false;

            Instance[] instances = new Instance[this.Count];

            this.CopyTo(instances, 0);

            this.Clear();

            Instance[] sorted = MergeSort(instances);

            foreach(Instance inst in sorted)
            {
                this.Add(inst);
            }
            _allowEvent = true;
            IsSorted = true;
        }

        private static Instance[] MergeSort(Instance[] arr)
        {
            if (arr.Length < 2) return arr;
            if (arr.Length == 2)
            {
                if (arr[0].Depth > arr[1].Depth)
                {
                    Instance temp = arr[0];
                    arr[0] = arr[1];
                    arr[1] = temp;
                    return arr;
                }
            }
            int split_point = (int)Math.Round((double)arr.Length / 2);
            SplitInstArr sp = split(arr, split_point);
            Instance[] a1 = MergeSort(sp.arr1);
            Instance[] a2 = MergeSort(sp.arr2);
            return Join(a1, a2);
        }

        private static Instance[] Join(Instance[] arr1, Instance[] arr2)
        {
            Instance[] r = new Instance[arr1.Length + arr2.Length];
            int a1 = 0, a2 = 0;
            for (int i = 0; i < r.Length; ++i)
            {
                if (a1 < arr1.Length && a2 < arr2.Length)
                {
                    if (arr1[a1].Depth < arr2[a2].Depth)
                    {
                        // i1 is smaller
                        r[i] = arr1[a1];
                        a1++;
                    } else
                    {
                        // i2 is smaller
                        r[i] = arr2[a2];
                        a2++;
                    }
                } else if (a1 == arr1.Length)
                {
                    r[i] = arr2[a2];
                    a2++;
                } else if (a2 == arr2.Length)
                {
                    r[i] = arr1[a1];
                    a1++;
                }
            }
            /*
            for (int i = 0; i < arr1.Length; ++i)
            {
                r[i] = arr1[i];
            }
            for (int i = 0; i < arr2.Length; ++i)
            {
                r[arr1.Length + i] = arr2[i];
            }
            */
            return r;
        }

        private static SplitInstArr split(Instance[] arr, int splitPoint)
        {
            if (splitPoint >= arr.Length) throw new ArgumentOutOfRangeException("Split point out-of-range.");
            Instance[] a1 = new Instance[splitPoint];
            Instance[] a2 = new Instance[arr.Length - splitPoint];
            for (int i = 0; i < splitPoint; ++i)
            {
                a1[i] = arr[i];
            }
            for (int i = splitPoint; i < arr.Length; ++i)
            {
                a2[i - splitPoint] = arr[i];
            }
            return new SplitInstArr() { arr1 = a1, arr2 = a2 };
        }

        private struct SplitInstArr
        {
            public Instance[] arr1;
            public Instance[] arr2;
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
