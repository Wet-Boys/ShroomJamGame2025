using Godot;

namespace ShroomGameReal.Music;

[Tool]
[GlobalClass]
public partial class LoopableAudioStreamSynchronized : AudioStreamSynchronized
{
    public override bool _HasLoop()
    {
        return true;
    }
}
