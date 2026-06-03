namespace Western_Restaurant.Services;

public class HardwareService : IHardwareService, IDisposable
{
    private Action? _onShakeCallback;
    private bool _isListeningForShake;

    public bool IsCameraAvailable => MediaPicker.Default.IsCaptureSupported;
    public bool IsAccelerometerAvailable => Accelerometer.Default.IsSupported;

    public async Task<string?> TakePhotoAsync()
    {
        try
        {
            if (!IsCameraAvailable) return null;
            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo == null) return null;
            var localPath = Path.Combine(FileSystem.AppDataDirectory, photo.FileName);
            using var stream = await photo.OpenReadAsync();
            using var fileStream = File.OpenWrite(localPath);
            await stream.CopyToAsync(fileStream);
            return localPath;
        }
        catch (PermissionException)
        {
            System.Diagnostics.Debug.WriteLine("Camera permission denied.");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Camera error: {ex.Message}");
            return null;
        }
    }

    public async Task<string?> PickPhotoAsync()
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync();
            if (photo == null) return null;
            var localPath = Path.Combine(FileSystem.AppDataDirectory, photo.FileName);
            using var stream = await photo.OpenReadAsync();
            using var fileStream = File.OpenWrite(localPath);
            await stream.CopyToAsync(fileStream);
            return localPath;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Pick photo error: {ex.Message}");
            return null;
        }
    }

    public void Vibrate()
    {
        try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Haptic error: {ex.Message}"); }
    }

    public void VibrateLong()
    {
        try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Haptic error: {ex.Message}"); }
    }

    public void StartShakeDetection(Action onShake)
    {
        if (!IsAccelerometerAvailable || _isListeningForShake) return;
        try
        {
            _onShakeCallback = onShake;
            _isListeningForShake = true;
            Accelerometer.Default.ShakeDetected += OnShakeDetected;
            Accelerometer.Default.Start(SensorSpeed.Game);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Accelerometer start error: {ex.Message}");
            _isListeningForShake = false;
        }
    }

    public void StopShakeDetection()
    {
        if (!_isListeningForShake) return;
        try
        {
            Accelerometer.Default.ShakeDetected -= OnShakeDetected;
            Accelerometer.Default.Stop();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Accelerometer stop error: {ex.Message}");
        }
        finally
        {
            _isListeningForShake = false;
            _onShakeCallback = null;
        }
    }

    private void OnShakeDetected(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() => _onShakeCallback?.Invoke());
    }

    public async Task SpeakAsync(string text, CancellationToken cancellationToken = default)
    {
        try
        {
            await TextToSpeech.Default.SpeakAsync(text, new SpeechOptions
            {
                Volume = 1.0f,
                Pitch = 1.0f
            }, cancelToken: cancellationToken);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"TTS error: {ex.Message}");
        }
    }

    public void Dispose()
    {
        StopShakeDetection();
    }
}
