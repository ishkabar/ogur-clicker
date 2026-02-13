using System.Drawing;
using Ogur.Clicker.Core.Services;

namespace Ogur.Clicker.Infrastructure.Services;

public class PositionCaptureService : IPositionCaptureService
{
    private readonly IMouseHookService _mouseHookService;
    private bool _isCapturing;

    public event EventHandler<Point>? PositionCaptured;
    
    public bool IsCapturing => _isCapturing;

    public PositionCaptureService(IMouseHookService mouseHookService)
    {
        _mouseHookService = mouseHookService;
        _mouseHookService.LeftButtonClicked += OnMouseClicked;
    }

    public void StartCapture()
    {
        if (_isCapturing)
            return;

        _isCapturing = true;
        _mouseHookService.StartListening();
    }

    public void StopCapture()
    {
        if (!_isCapturing)
            return;

        _isCapturing = false;
        _mouseHookService.StopListening();
    }

    private void OnMouseClicked(object? sender, Point point)
    {
        if (!_isCapturing)
            return;
        
        _isCapturing = false;
        _mouseHookService.StopListening();
        
        PositionCaptured?.Invoke(this, point);
    }
}