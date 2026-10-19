using Godot;
using System;
using System.Threading.Tasks;

public partial class LevelTransition : Control
{
    [Export] public ColorRect background;
    [Export] public float fadeInDuration = 1f;
    [Export] public float fadeOutDuration = 5f;


    public override void _Ready()
    {
        base._Ready();
        background.Modulate = new Color(1, 1, 1, 0);
    }

    public async Task FadeBackground(bool bFadeToBlack)
    {
        Tween tween = CreateTween();
        float alpha = bFadeToBlack ? 1f : 0f;
        float duration = bFadeToBlack ? fadeInDuration : fadeOutDuration;
        tween.TweenProperty(background, "modulate:a", alpha, duration).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.Out).SetDelay(0.1f);
        await ToSignal(tween, Tween.SignalName.Finished);
    }


}
