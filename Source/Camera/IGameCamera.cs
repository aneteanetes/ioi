public interface IGameCamera
{    
    /// <summary>
    /// default true
    /// </summary>
    bool CanZoom { get; }
    
    /// <summary>
    /// default true
    /// </summary>
    bool CanDrag { get; }
    
    /// <summary>
    /// 1.5
    /// </summary>
    float MinZoom { get; }
    
    /// <summary>
    /// 4
    /// </summary>
    float MaxZoom { get; }
}