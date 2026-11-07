namespace LD60
{
    public enum DrawLayers
    {
        StoreFrameBuffer,
        ResetInstancers,
        FillInstancers,
        Background,
        BeginWorld,
        World,
        BeginPicking,
        Picking,
        BeginWorldAfterPicking,
        WorldAfterPicking,
        BeginMaterial,
        Material,
        RenderDeferredShader,
        BindIntermediateFramebuffer,
        Sky,
        AntiAliasing,
        Transparents,
        Postprocessing,
        HUD,
        Present,
    }

    public enum UpdateLayers
    {
        ChangePhase,
        PrepareUpdate,
        Input,
        Move,
        Collision,
        PostCollision,
        CreateParticles,
        UpdateParticles,
        ClearAnimations,
        UpdateAnimations,
        SetCamera,
    }

    public enum InputLayer
    {
        ScreenShot,
        Menu,
        Tools,
        Game,
    }
}
