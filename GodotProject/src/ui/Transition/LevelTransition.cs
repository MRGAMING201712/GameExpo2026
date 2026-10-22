using Godot;
using System;
using System.Threading.Tasks;

public partial class LevelTransition : Control
{
    [Export] public ColorRect background;
    [Export] public float toBlackDuration = 0.35f;
    [Export] public float holdDuration = 0.25f;
    [Export] public float fromBlackDuration = 0.5f;

    private Tween fadeTween;

    public override void _Ready()
    {
        background.Modulate = new Color(1, 1, 1, 0);
        background.MouseFilter = MouseFilterEnum.Ignore;
    }

    public async Task FadeToBlack()
    {
        await Fade(1f, toBlackDuration, Tween.EaseType.Out);
    }

    public async Task FadeFromBlack()
    {
       await Fade(0f, fromBlackDuration, Tween.EaseType.In); 
    } 

    private async Task Fade(float alpha, float duration, Tween.EaseType ease)
    {
        fadeTween?.Kill();
        fadeTween = CreateTween();
        fadeTween.TweenProperty(background, "modulate:a", alpha, duration).SetTrans(Tween.TransitionType.Cubic).SetEase(ease);
        await ToSignal(fadeTween, Tween.SignalName.Finished);
    }
}