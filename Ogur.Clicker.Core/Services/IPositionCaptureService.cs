using System.Drawing;

namespace Ogur.Clicker.Core.Services;

public interface IPositionCaptureService
{
    event EventHandler<Point>? PositionCaptured;
    
    void StartCapture();
    void StopCapture();
    
    bool IsCapturing { get; }
}