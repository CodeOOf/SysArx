using SysArx.Services.SysMLStore.Models;

namespace SysArx.Services.SysMLStore.Storage;

/// <summary>
/// Abstraction for SysML model storage.
/// Supports multiple storage backends: NoSQL (MongoDB), GitHub, GitLab.
/// </summary>
public interface IStorageProvider
{
    /// <summary>
    /// Gets all SysML items from storage.
    /// </summary>
    Task<List<SysMLItem>> GetAllAsync();

    /// <summary>
    /// Gets a single SysML item by ID.
    /// </summary>
    Task<SysMLItem?> GetByIdAsync(string id);

    /// <summary>
    /// Gets all SysML items matching a specific type.
    /// </summary>
    Task<List<SysMLItem>> GetByTypeAsync(string type);

    /// <summary>
    /// Creates a new SysML item in storage.
    /// </summary>
    Task<SysMLItem> CreateAsync(SysMLItem item);

    /// <summary>
    /// Updates an existing SysML item in storage.
    /// </summary>
    Task UpdateAsync(string id, SysMLItem item);

    /// <summary>
    /// Deletes a SysML item from storage.
    /// </summary>
    Task DeleteAsync(string id);

    /// <summary>
    /// Gets the storage provider type (NoSQL, GitHub, GitLab).
    /// </summary>
    string ProviderType { get; }
}
