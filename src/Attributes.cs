using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GEngine;
using GEngine.Engine;

namespace GEngine
{
    [AttributeUsage(AttributeTargets.Class)]
    public class LoaderAttribute : Attribute
    {
        private LoaderType _loader;
        public ClassConstructorType ConstructorType;

        public LoaderAttribute(LoaderType type)
        {
            _loader = type;
            ConstructorType = ClassConstructorType.Automatic;
        }

        public LoaderType GetLoaderType() => _loader;
    }
}
