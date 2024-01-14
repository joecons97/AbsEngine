namespace AbsEngine.Rendering.FullscreenEffects;

public enum TonemapperMode
{ 
    Reinhardt,
    NaughtyDog
}

public class Tonemapping : FullscreenEffect
{
    public TonemapperMode Mode { get; set; }

    Material _reinhardtMaterial;
    Material _naughtDogMaterial;

    public Tonemapping()
    {
        _reinhardtMaterial = new Material("Tonemapper_Reinhardt");
        _naughtDogMaterial = new Material("Tonemapper_NaughtyDog");
    }

    public override void OnRender(RenderTexture source, RenderTexture destination)
    {
        var mat = Mode switch
        {
            TonemapperMode.Reinhardt => _reinhardtMaterial,
            TonemapperMode.NaughtyDog => _naughtDogMaterial,
            _ => throw new NotImplementedException(),
        };

        Blit(source, destination, mat);
    }
}
