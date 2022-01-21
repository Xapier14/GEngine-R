using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEngine.Engine
{
    internal class AutoOffset
    {
        private GameEngine _engine;
        private double _baseTPS, _baseFPS; // base offsets
        private double _deadzone;

        // adjustment range
        private double[] _range =
        {
            0.10,
            0.20,
            0.30,
            0.50,
            1.00,
            1.50
        };

        private int _curAdjustFps, _curAdjustTps;

        public AutoOffset(GameEngine gameEngine, double deadzone = 0.1)
        {
            _engine = gameEngine;
            _baseTPS = gameEngine.Properties.TPSOffset;
            _baseFPS = gameEngine.Properties.FPSOffset;
            _deadzone = deadzone;
            _curAdjustFps = 0;
            _curAdjustTps = 0;
        }

        public void AdjustOffsets()
        {
            double tps = _engine.CurrentLogictime;
            double fps = _engine.CurrentFrametime;

            double targetTps = 1000.0 / _engine.Properties.TargetTPS;
            double targetFps = 1000.0 / _engine.Properties.TargetFPS;

            double tpsDeviation = tps - targetTps;
            double fpsDeviation = fps - targetFps;

            if (Math.Abs(tpsDeviation) > _deadzone)
            {
                // adjust tps
                if (tpsDeviation > 0)
                {
                    // reset if adjustment was negative
                    if (_curAdjustTps < 0)
                    {
                        _curAdjustTps = 0;
                    }
                    if (Math.Abs(_curAdjustTps) < _range.Length - 1)
                    {
                        _curAdjustTps++;
                    }
                }
                else if (tpsDeviation < 0)
                {
                    // reset if adjustment was positive
                    if (_curAdjustTps > 0)
                    {
                        _curAdjustTps = 0;
                    }
                    if (Math.Abs(_curAdjustTps) < _range.Length - 1)
                    {
                        _curAdjustTps--;
                    }
                }

                // apply adjustments
                _engine.Properties.TPSOffset += _curAdjustTps == 0 ? 0 :
                    _curAdjustTps > 0 ? _range[Math.Abs(_curAdjustTps) - 1] :
                    -_range[Math.Abs(_curAdjustTps) - 1];
            }

            if (Math.Abs(fpsDeviation) > _deadzone)
            {
                // adjust fps
                if (fpsDeviation > 0)
                {
                    // reset if adjustment was negative
                    if (_curAdjustFps < 0)
                    {
                        _curAdjustFps = 0;
                    }
                    if (Math.Abs(_curAdjustFps) < _range.Length - 1)
                    {
                        _curAdjustFps++;
                    }
                }
                else if (fpsDeviation < 0)
                {
                    // reset if adjustment was positive
                    if (_curAdjustFps > 0)
                    {
                        _curAdjustFps = 0;
                    }
                    if (Math.Abs(_curAdjustFps) < _range.Length - 1)
                    {
                        _curAdjustFps--;
                    }
                }

                // apply adjustments
                _engine.Properties.FPSOffset += _curAdjustFps == 0 ? 0 :
                    _curAdjustFps > 0 ? _range[Math.Abs(_curAdjustFps) - 1] :
                    -_range[Math.Abs(_curAdjustFps) - 1];
            }
        }
    }
}
