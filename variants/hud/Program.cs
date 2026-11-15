using GoomeSharp;

const string AppName = "My Goome App";

Goome g;
try { g = new Goome(new GoomeOptions { Title = AppName }); }
catch (GoomeException e)
{
    Console.Error.WriteLine("couldnt open an overlay: " + e.Message);
    return 1;
}
using (g)
{
    g.Hotkey("alt+h", 1);
    long start = Goome.NowMs, last = start;
    double fps = 0;
    bool hidden = false;
    var pace = new Pacer();
    for (bool running = true; running;)
    {
        int wait = pace.Pace(30);
        while (g.Poll(out var ev, wait))
        {
            if (ev.Type == EventType.Quit) running = false;
            if (ev.Type == EventType.Hotkey) hidden = !hidden;
            wait = pace.Pace(30);
        }
        if (!running) break;
        long now = Goome.NowMs;
        if (now > last) fps = fps * 0.9 + 100.0 / (now - last);
        last = now;
        var c = g.Begin(0);
        if (c == null) continue;
        if (!hidden)
        {
            c.DrawRoundRect(20, 20, 160, 92, 10, 0xB0141820);
            c.DrawText(34, 30, DateTime.Now.ToString("HH:mm:ss"), true, 0xFFFFFFFF);
            c.DrawText(34, 62, $"up {(now - start) / 1000}s", false, 0xFFC0C8D0);
            c.DrawText(34, 84, $"{fps:0} fps", false, 0xFF6FDC5F);
        }
        g.End(c);
    }
}
return 0;
