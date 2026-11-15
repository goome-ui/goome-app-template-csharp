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
    g.Hotkey("alt+m", 1);
    double x = 80, y = 80, w = 300, h = 0, speed = 1;
    bool menu = false, fly = false, god = false;
    for (bool running = true; running;)
    {
        while (g.Poll(out var ev, 16))
        {
            if (ev.Type == EventType.Quit) running = false;
            if (ev.Type == EventType.Hotkey) menu = !menu;
            ui.Feed(ev);
        }
        if (!running) break;
        var c = ui.Begin(0);
        if (c == null) continue;
        if (menu)
        {
            if (ui.Window(AppName, ref x, ref y, ref w, ref h, WindowFlags.Move | WindowFlags.Close, ref menu))
            {
                ui.Toggle("Fly", ref fly);
                ui.Checkbox("God mode", ref god);
                ui.Slider("Speed", ref speed, 1, 10, "%.1f");
                if (ui.Button("Respawn")) ui.Toast("respawned", 2000);
            }
            ui.WindowEnd();
        }
        else
        {
            c.DrawRoundRect(20, 20, 170, 30, 8, 0xC0202838);
            c.DrawText(32, 26, "alt+m opens the menu", false, 0xFFFFFFFF);
        }
        ui.End();
    }
}
return 0;
