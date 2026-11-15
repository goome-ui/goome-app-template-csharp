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
using (var ui = new Ui(g))
{
    g.Hotkey("alt+h", 1);
    double x = 60, y = 60, w = 280, h = 0;
    int clicks = 0;
    bool hidden = false, open = true;
    while (open)
    {
        while (g.Poll(out var ev, 16))
        {
            if (ev.Type == EventType.Quit) open = false;
            if (ev.Type == EventType.Hotkey) hidden = !hidden;
            ui.Feed(ev);
        }
        if (!open || ui.Begin(0) == null) continue;
        if (!hidden)
        {
            if (ui.Window(AppName, ref x, ref y, ref w, ref h, WindowFlags.Move | WindowFlags.Close, ref open))
            {
                ui.Label("im on top of everything");
                if (ui.Button("Click me")) clicks++;
                ui.Label($"clicked {clicks} times");
                ui.Label("alt+h hides me");
            }
            ui.WindowEnd();
        }
        ui.End();
    }
}
return 0;
