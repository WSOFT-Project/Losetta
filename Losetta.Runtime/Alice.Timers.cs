using AliceScript.Binding;
using AliceScript.Objects;
using AliceScript.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AliceScript.NameSpaces
{
    [AliceNameSpace(Name = "Alice.Timers")]
    public class Alice_Timers
    {
        public static void Init()
        {
            Alice.RegisterObject<Timer>();
        }
    }
    [AliceObject(Name = "Timer", NameSpace = "Alice.Timers")]
    public sealed class Timer
    {
        private System.Timers.Timer _timer;
        private DelegateObject _onElapsed = new DelegateObject();
        public Timer()
        {
            _timer = new System.Timers.Timer();
            _timer.Elapsed += (sender, e) =>
            {
                _onElapsed.Invoke(null);
            };
        }
        public DelegateObject OnElapsed
        {
            get => _onElapsed;
            set => _onElapsed = value;
        }
        public double Interval
        {
            get => _timer.Interval;
            set => _timer.Interval = value;
        }
        public bool Enabled
        {
            get => _timer.Enabled;
            set => _timer.Enabled = value;
        }
        public void Start()
        {
            _timer.Start();
        }
        public void Stop()
        {
            _timer.Stop();
        }
        public void Perform()
        {
            _onElapsed.Invoke(null);
        }
    }

}