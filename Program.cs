using GoomeSharp;

const string AppName = "My Goome App";

int clicks = 0;
double volume = 0.5;
bool overlay = false, mode = false;
double x = 40, y = 60, w = 300, h = 0;

Goome OpenMode(bool ov)
{
    if (!ov) return Goome.OpenApp(AppName, 640, 420);
    var o = new Goome(new GoomeOptions { Title = AppName });
    var mons = o.Monitors;
    x = mons.Length > 0 ? mons[0].Area.X + mons[0].Area.W - w - 40 : 40;
    return o;
}

void Contents(Ui ui)
{
    ui.Toggle("Overlay mode", ref overlay);
    ui.Separator();
    ui.Heading("Hello from " + AppName);
    if (ui.Button("Click me")) clicks++;
    ui.Label($"clicked {clicks} times");
    ui.Slider("Volume", ref volume, 0, 1, "%.2f");
}

Goome g;
try { g = OpenMode(false); }
catch (GoomeException e)
{
    Console.Error.WriteLine("couldnt open a window: " + e.Message);
    return 1;
}
var ui = new Ui(g);
for (bool running = true; running;)
{
    while (g.Poll(out var ev, 16))
    {
        if (ev.Type == EventType.Quit || ev.Type == EventType.Close) running = false;
        ui.Feed(ev);
    }
    if (!running) break;
    if (overlay != mode)
    {
        ui.Dispose();
        g.Dispose();
        try { g = OpenMode(overlay); }
        catch (GoomeException)
        {
            Console.Error.WriteLine("no overlay here, staying in window mode");
            overlay = false;
            g = OpenMode(false);
        }
        mode = overlay;
        ui = new Ui(g);
        Console.WriteLine(mode ? "overlay mode" : "window mode");
        continue;
    }
    if (ui.Begin(0) == null) continue;
    if (mode)
    {
        if (ui.Window(AppName, ref x, ref y, ref w, ref h, WindowFlags.Move | WindowFlags.Collapse)) Contents(ui);
        ui.WindowEnd();
    }
    else
    {
        ui.App(AppName);
        Contents(ui);
        ui.AppEnd();
    }
    ui.End();
}
ui.Dispose();
g.Dispose();
return 0;
