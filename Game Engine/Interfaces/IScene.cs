namespace Game_Engine.Interfaces;

internal interface IScene
{
    public void OnFrameUpdate();
    public void OnFixedTimeUpdate();
    public void OnSceneStart();
    public void OnSceneExit();
}
