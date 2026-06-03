namespace Western_Restaurant.Services;

public interface IHardwareService
{
    Task<string?> TakePhotoAsync();
    Task<string?> PickPhotoAsync();
    void Vibrate();
    void VibrateLong();
    void StartShakeDetection(Action onShake);
    void StopShakeDetection();
    Task SpeakAsync(string text, CancellationToken cancellationToken = default);
    bool IsCameraAvailable { get; }
    bool IsAccelerometerAvailable { get; }
}
