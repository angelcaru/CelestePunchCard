using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste.Mod.PunchCard;

public class PunchCardModule : EverestModule {
    public static PunchCardModule Instance { get; private set; }

    public PunchCardModule() {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(PunchCardModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(PunchCardModule), LogLevel.Info);
#endif
    }

    public static VirtualButton punchCardButton = null;

    public static void OnLevelUpdate(On.Celeste.Level.orig_Update orig, Level self)
    {
        orig(self);
        Player player = self.Tracker.GetEntity<Player>();
        if (player is null) return;
        if (player.InControl && !self.Paused && punchCardButton.Pressed)
        {
            self.Add(new PunchCardNextFrame());
        }
    }

    public static void InitButton(On.Monocle.Engine.orig_Initialize orig, Engine self)
    {
        orig(self);
        punchCardButton = new VirtualButton(new VirtualButton.KeyboardKey(Keys.P));
    }

    public override void Load()
    {
        On.Celeste.Level.Update += OnLevelUpdate;
        On.Monocle.Engine.Initialize += InitButton;
    }

    public override void Unload() {
        On.Celeste.Level.Update -= OnLevelUpdate;
    }
}

public class PunchCardNextFrame : Entity
{
    float addedTime;
    public override void Added(Scene scene)
    {
        base.Added(scene);
        addedTime = scene.TimeActive;
    }

    public override void Update()
    {
        base.Update();
        if (Scene.TimeActive != addedTime)
        {
            Scene.Add(new PunchCard());
            RemoveSelf();
        }
    }

}

public class PunchCard : Entity
{
    MTexture texture;
    float scale = 2.2f;

    public PunchCard()
    {
        Tag = Tags.HUD;
        texture = GFX.Gui["punchCard"];
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        Level level = (Level)scene;
        Player player = level.Tracker.GetEntity<Player>();
        if (player is null) return;
        player.StateMachine.State = 11; // StDummy
    }

    public override void Update()
    {
        base.Update();
        if (Input.MenuConfirm.Pressed)
        {
            Input.MenuConfirm.ConsumePress();
            RemoveSelf();
        }
        if (Input.MenuCancel.Pressed)
        {
            Input.MenuCancel.ConsumePress();
            RemoveSelf();
        }
    }

    public override void Render()
    {
        base.Render();
        Vector2 screen = new Vector2(1920, 1080);
        texture.DrawCentered(screen / 2, Color.White, scale);
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        Level level = (Level)scene;
        Player player = level.Tracker.GetEntity<Player>();
        if (player is null) return;
        player.StateMachine.State = 0; // StNormal
    }
}