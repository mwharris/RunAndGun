public interface IPlayerInput
{
    float Vertical { get; }
    bool VerticalHeld { get; }
    float VerticalRaw { get; }

    float Horizontal { get; }
    bool HorizontalHeld { get; }
    float HorizontalRaw { get; }
    
    float MouseX { get; }
    float MouseY { get; }
    
    bool ShiftDown { get; }
    bool ShiftHeld { get; }
    
    bool SpaceDown { get; }
    bool SpaceHeld { get; }
    
    bool CrouchDown { get; }
    bool CrouchHeld { get; }

    void Tick();
}
