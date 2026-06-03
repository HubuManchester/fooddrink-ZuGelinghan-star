using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Western_Restaurant.Models;
using Western_Restaurant.Services;

namespace Western_Restaurant.ViewModels;

public partial class CommentsViewModel : BaseViewModel
{
    private readonly IHardwareService _hardwareService;

    [ObservableProperty] private ObservableCollection<Comment> _comments = new();
    [ObservableProperty] private string _newCommentText = string.Empty;
    [ObservableProperty] private string _attachedPhotoPath = string.Empty;
    [ObservableProperty] private bool _hasPhoto;
    [ObservableProperty] private bool _isUploadingPhoto;

    public CommentsViewModel(IHardwareService hardwareService)
    {
        _hardwareService = hardwareService;
        Title = "Comments";
    }

    [RelayCommand]
    private void AddComment()
    {
        var text = NewCommentText?.Trim();
        if (string.IsNullOrWhiteSpace(text) && string.IsNullOrWhiteSpace(AttachedPhotoPath))
            return;

        Comments.Insert(0, new Comment
        {
            Text = text ?? string.Empty,
            PhotoPath = AttachedPhotoPath,
            Timestamp = DateTime.Now
        });

        NewCommentText = string.Empty;
        AttachedPhotoPath = string.Empty;
        HasPhoto = false;
        _hardwareService.Vibrate();
    }

    [RelayCommand]
    private async Task TakePhotoAsync()
    {
        await ExecuteSafelyAsync(async () =>
        {
            var path = await _hardwareService.TakePhotoAsync();
            if (!string.IsNullOrWhiteSpace(path))
            {
                AttachedPhotoPath = path;
                HasPhoto = true;
                _hardwareService.Vibrate();
            }
        }, "Camera error");
    }

    [RelayCommand]
    private async Task PickPhotoAsync()
    {
        await ExecuteSafelyAsync(async () =>
        {
            var path = await _hardwareService.PickPhotoAsync();
            if (!string.IsNullOrWhiteSpace(path))
            {
                AttachedPhotoPath = path;
                HasPhoto = true;
                _hardwareService.Vibrate();
            }
        }, "Gallery error");
    }

    [RelayCommand]
    private void ClearPhoto()
    {
        AttachedPhotoPath = string.Empty;
        HasPhoto = false;
    }

    [RelayCommand]
    private void DeleteComment(Comment? comment)
    {
        if (comment == null) return;
        Comments.Remove(comment);
        _hardwareService.Vibrate();
    }
}
