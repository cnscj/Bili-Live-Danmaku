
using System;
using System.Timers;

public class IntervalTimer
{
    Timer _timer;
    int _period;
    int _count;
    Action _onEvent;

    public IntervalTimer(int interval,int period = 0)
    {
        _timer = new Timer(interval);
        _timer.Elapsed += new ElapsedEventHandler(OnEvent);  //到达时间的时候执行事件;

        _period = period;

    }
    public void Start()
    {
        _timer?.Start();
        _count = 0;
    }

    public void Stop()
    {
        _timer?.Stop();
    }

    public void Event(Action call)
    {
        _onEvent = call;
    }

    private void OnEvent(object source, ElapsedEventArgs e)
    {
        _count++;
        _onEvent?.Invoke();

        if (_period > 0 && _count >= _period)
            Stop();
    }
}
