using System.Security.Claims;
using BookLoverECommerce.Web.Models.Auth;
using BookLoverECommerce.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookLoverECommerce.Web.Pages.Account;

[Authorize]
public sealed class ProfileModel : PageModel
{
    private readonly IAuthApiClient _authApiClient;
    private readonly IWebHostEnvironment _environment;

    public ProfileModel(
        IAuthApiClient authApiClient,
        IWebHostEnvironment environment)
    {
        _authApiClient = authApiClient;
        _environment = environment;
    }

    public ProfileResponse? Profile { get; private set; }

    public string? ProfileImageUrl { get; private set; }

    [BindProperty]
    public IFormFile? ProfileImage { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            Profile =
                await _authApiClient.GetProfileAsync(
                    cancellationToken);

            if (Profile is null)
            {
                return NotFound();
            }

            LoadProfileImage();

            return Page();
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage =
                $"Profile could not be loaded. {ex.Message}";

            LoadProfileImage();

            return Page();
        }
    }

    public async Task<IActionResult> OnPostUploadImageAsync(
        CancellationToken cancellationToken)
    {
        var userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        if (ProfileImage is null ||
            ProfileImage.Length == 0)
        {
            ErrorMessage =
                "Please select a profile picture.";

            await ReloadProfileAsync(
                cancellationToken);

            return Page();
        }

        const long maxFileSize =
            5 * 1024 * 1024;

        if (ProfileImage.Length > maxFileSize)
        {
            ErrorMessage =
                "Profile picture must be smaller than 5 MB.";

            await ReloadProfileAsync(
                cancellationToken);

            return Page();
        }

        var extension =
            Path.GetExtension(
                ProfileImage.FileName)
            .ToLowerInvariant();

        var allowedExtensions =
            new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

        if (!allowedExtensions.Contains(extension))
        {
            ErrorMessage =
                "Only JPG, JPEG, PNG, and WEBP images are allowed.";

            await ReloadProfileAsync(
                cancellationToken);

            return Page();
        }

        var uploadFolder =
            Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "profiles");

        Directory.CreateDirectory(
            uploadFolder);

        DeleteExistingProfileImages(
            userId,
            uploadFolder);

        var fileName =
            $"{userId}{extension}";

        var filePath =
            Path.Combine(
                uploadFolder,
                fileName);

        await using var stream =
            new FileStream(
                filePath,
                FileMode.Create);

        await ProfileImage.CopyToAsync(
            stream,
            cancellationToken);

        SuccessMessage =
            "Profile picture updated successfully.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRemoveImageAsync(
        CancellationToken cancellationToken)
    {
        var userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var uploadFolder =
            Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "profiles");

        DeleteExistingProfileImages(
            userId,
            uploadFolder);

        SuccessMessage =
            "Profile picture removed.";

        await Task.CompletedTask;

        return RedirectToPage();
    }

    private async Task ReloadProfileAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            Profile =
                await _authApiClient.GetProfileAsync(
                    cancellationToken);
        }
        catch
        {
            Profile = null;
        }

        LoadProfileImage();
    }

    private void LoadProfileImage()
    {
        var userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        var uploadFolder =
            Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "profiles");

        if (!Directory.Exists(
                uploadFolder))
        {
            return;
        }

        var extensions =
            new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

        foreach (var extension in extensions)
        {
            var fileName =
                $"{userId}{extension}";

            var path =
                Path.Combine(
                    uploadFolder,
                    fileName);

            if (System.IO.File.Exists(path))
            {
                ProfileImageUrl =
                    $"/uploads/profiles/{fileName}";

                return;
            }
        }
    }

    private static void DeleteExistingProfileImages(
        string userId,
        string uploadFolder)
    {
        if (!Directory.Exists(
                uploadFolder))
        {
            return;
        }

        foreach (var extension in new[]
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        })
        {
            var path =
                Path.Combine(
                    uploadFolder,
                    $"{userId}{extension}");

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }
    }
}