using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEngine
{
    public class Sampler
    {
        private List<double> _data;
        private int _max;

        public Sampler(int maxPoints)
        {
            if (maxPoints < 2) throw new Exception("Sampler must have a max point greater than or equal to 2.");
            _max = maxPoints;
            _data = new List<double>();
        }

        public void AddPoint(double data)
        {
            _data.Add(data);
            if (_data.Count > _max)
            {
                _data.RemoveAt(0);
            }
        }

        public double GetAverage()
        {
            double sum = 0;
            foreach(double d in _data)
            {
                sum += d;
            }
            return sum / _data.Count;
        }
    }
}
