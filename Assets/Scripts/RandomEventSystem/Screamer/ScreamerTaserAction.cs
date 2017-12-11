using JetBrains.Annotations;

public class ScreamerTaserAction : ScreamerActionComponent
{
    public override void TriggerAction(float time)
    {
        GetComponentInChildren<Taser>().Activate();
    }
}
