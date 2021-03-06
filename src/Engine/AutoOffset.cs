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
        private double _tpsHalfRange = 0.5;
        private double _fpsHalfRange = 0.8;

        public int FpsLevel { get => _curAdjustFps; }
        public int TpsLevel { get => _curAdjustTps; }

        // adjustment range
        private double[] _range =
        {
            0.010,
            0.015,
            0.020,
            0.025,
            0.030,
            0.050
        };

        private int _curAdjustFps, _curAdjustTps;

        public AutoOffset(GameEngine gameEngine, double deadzone = 0.01)
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

            double curTpsOffset = _engine.Properties.TPSOffset;
            double curFpsOffset = _engine.Properties.FPSOffset;

            double targetTps = 1000.0 / _engine.Properties.TargetTPS;
            double targetFps = 1000.0 / _engine.Properties.TargetFPS;

            double tpsDeviation = tps - targetTps;
            double fpsDeviation = fps - targetFps;

            if (Math.Abs(tpsDeviation) > _deadzone)
            {
                // adjust tps
                if (tpsDeviation > 0 && curTpsOffset > _baseTPS - _tpsHalfRange)
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
                else if (tpsDeviation < 0 && curTpsOffset < _baseTPS + _tpsHalfRange)
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
                } else
                {
                    _curAdjustTps = 0;
                }

                // apply adjustments
                _engine.Properties.TPSOffset -= _curAdjustTps == 0 ? 0 :
                    _curAdjustTps > 0 ? _range[Math.Abs(_curAdjustTps) - 1] :
                    -_range[Math.Abs(_curAdjustTps) - 1];
            }

            if (Math.Abs(fpsDeviation) > _deadzone)
            {
                // adjust fps
                if (fpsDeviation > 0 && curFpsOffset > _baseFPS - _fpsHalfRange)
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
                else if (fpsDeviation < 0 && curFpsOffset < _baseFPS + _fpsHalfRange)
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
                else
                {
                    _curAdjustFps = 0;
                } 


                // apply adjustments
                _engine.Properties.FPSOffset -= _curAdjustFps == 0 ? 0 :
                    _curAdjustFps > 0 ? _range[Math.Abs(_curAdjustFps) - 1] :
                    -_range[Math.Abs(_curAdjustFps) - 1];
            }
        }
    }
}
