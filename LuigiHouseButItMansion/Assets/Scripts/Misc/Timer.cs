using System;
using UnityEngine;

namespace LucasCustomClasses
{
    public class Timer
    {
        private double _currentTime = 0;
        private float _maxTime = 0;
        public System.Action onEnd;
        public System.Action<double> onPlaying;
        public bool running;

        public Timer(float maxTime, System.Action onEnd)
        {
            this._maxTime = maxTime;
            this.onEnd = onEnd;
            running = true;
        }
        
        public Timer(float maxTime)
        {
            this._maxTime = maxTime;
            running = true;
        }
        
        public void Reset()
        {
            _currentTime = 0;
            running = true;
        }

        public void Update(double dt)
        {
            if (!running)
                return;
            _currentTime += dt;
            onPlaying?.Invoke(_currentTime);
            CheckIfEndIsReached();
        }

        public void CheckIfEndIsReached()
        {
            if (_currentTime >= _maxTime)
            {
                onEnd?.Invoke();
                running = false;
            }
        }

        public double GetCurrentTime()
        {
            return _currentTime;
        }
        
        private string GetFormattedTime(double time)
        {
            var timeSpan = TimeSpan.FromSeconds(time);
            return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
        
        public string GetFormattedTime(bool countDown = false)
        {
            return countDown ? GetFormattedTime(_maxTime - _currentTime) : GetFormattedTime(_currentTime);
        }
    }
}