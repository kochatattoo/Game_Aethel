namespace Infrastructure.AudioSystem.Components.Materials
{
    public interface ISurfaceMaterial<out T>
    {
        T SurfaceKey { get; }
    }
}
