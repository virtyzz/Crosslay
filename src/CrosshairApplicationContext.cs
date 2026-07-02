using System.Windows.Forms;

namespace CrosshairMarker;

internal sealed class CrosshairApplicationContext : ApplicationContext
{
    private readonly ConfigStore store;
    private readonly OverlayForm overlay;
    private readonly TrayController tray;
    private readonly HotkeyManager hotkeys;
    private EditorForm? editor;
    private AppConfig config;

    public CrosshairApplicationContext()
    {
        store = new ConfigStore();
        config = store.Load();
        overlay = new OverlayForm();
        overlay.ApplyMonitor(config.TargetMonitorDeviceName);
        overlay.ApplyProfile(config.CurrentProfile);

        if (config.OverlayVisible)
        {
            overlay.Show();
        }

        tray = new TrayController(
            onToggleOverlay: ToggleOverlay,
            onOpenEditor: OpenEditor,
            onSelectProfile: SelectProfile,
            onExit: ExitApplication);
        tray.SetOverlayVisible(config.OverlayVisible);
        tray.SetProfiles(config.Profiles, config.ActiveProfileId);

        hotkeys = new HotkeyManager();
        RegisterConfiguredHotkeys();
    }

    private void ToggleOverlay()
    {
        config.OverlayVisible = !config.OverlayVisible;
        if (config.OverlayVisible)
        {
            overlay.Show();
        }
        else
        {
            overlay.Hide();
        }

        tray.SetOverlayVisible(config.OverlayVisible);
        store.SaveAtomic(config);
    }

    private void OpenEditor()
    {
        if (editor is { IsDisposed: false })
        {
            editor.Activate();
            return;
        }

        editor = new EditorForm(config);
        editor.ConfigChanged += nextConfig =>
        {
            config = nextConfig;
            config.Normalize();
            overlay.ApplyMonitor(config.TargetMonitorDeviceName);
            overlay.ApplyProfile(config.CurrentProfile);
            tray.SetProfiles(config.Profiles, config.ActiveProfileId);
            RegisterConfiguredHotkeys();
            store.SaveAtomic(config);
        };
        editor.Show();
    }

    private void SelectProfile(string profileId)
    {
        if (config.Profiles.All(profile => profile.Id != profileId))
        {
            return;
        }

        config.ActiveProfileId = profileId;
        overlay.ApplyProfile(config.CurrentProfile);
        tray.SetProfiles(config.Profiles, config.ActiveProfileId);
        store.SaveAtomic(config);
    }

    private void RegisterConfiguredHotkeys()
    {
        hotkeys.UnregisterAll();
        RegisterHotkey(config.Hotkeys.ToggleOverlay, ToggleOverlay);
        RegisterHotkey(config.Hotkeys.NextProfile, () => SelectAdjacentProfile(1));
        RegisterHotkey(config.Hotkeys.PreviousProfile, () => SelectAdjacentProfile(-1));
        RegisterHotkey(config.Hotkeys.OpacityUp, () => AdjustOpacity(15));
        RegisterHotkey(config.Hotkeys.OpacityDown, () => AdjustOpacity(-15));
        RegisterHotkey(config.Hotkeys.SizeUp, () => AdjustSize(1));
        RegisterHotkey(config.Hotkeys.SizeDown, () => AdjustSize(-1));
    }

    private void RegisterHotkey(HotkeyBinding binding, Action action)
    {
        if (!binding.Enabled || !binding.TryGetKeys(out var key))
        {
            return;
        }

        hotkeys.Register(key, binding.ToModifiers(), action);
    }

    private void SelectAdjacentProfile(int direction)
    {
        if (config.Profiles.Count <= 1)
        {
            return;
        }

        var currentIndex = config.Profiles.FindIndex(profile => profile.Id == config.ActiveProfileId);
        if (currentIndex < 0)
        {
            currentIndex = 0;
        }

        var nextIndex = (currentIndex + direction + config.Profiles.Count) % config.Profiles.Count;
        SelectProfile(config.Profiles[nextIndex].Id);
    }

    private void AdjustOpacity(int delta)
    {
        MutateCurrentProfile(profile =>
        {
            var alpha = Math.Clamp(profile.Color.A + delta, 0, 255);
            profile.Color = profile.Color with { A = alpha };
        });
    }

    private void AdjustSize(int direction)
    {
        MutateCurrentProfile(profile =>
        {
            profile.Length = Math.Clamp(profile.Length + direction * 2, 1, 80);
            profile.Gap = Math.Clamp(profile.Gap + direction, 0, 50);
            profile.DotSize = Math.Clamp(profile.DotSize + direction, 1, 30);
        });
    }

    private void MutateCurrentProfile(Action<CrosshairProfile> mutation)
    {
        var profile = config.Profiles.FirstOrDefault(profile => profile.Id == config.ActiveProfileId);
        if (profile is null)
        {
            return;
        }

        mutation(profile);
        overlay.ApplyProfile(profile);
        store.SaveAtomic(config);

        if (editor is { IsDisposed: false })
        {
            editor.ApplyExternalConfig(config);
        }
    }

    private void ExitApplication()
    {
        store.SaveAtomic(config);
        hotkeys.Dispose();
        tray.Dispose();
        overlay.Close();
        editor?.Close();
        ExitThread();
    }
}
