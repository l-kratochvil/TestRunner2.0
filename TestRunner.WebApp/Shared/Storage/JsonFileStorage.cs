namespace TestRunner.WebApp.Shared.Storage;

using System.IO;
using System.Text.Json;

using TestRunner.WebApp.Shared.Logging;

/// <summary>
/// Keeps one piece of data in a JSON file on the machine the application runs on.
/// </summary>
/// <remarks>
/// The counterpart of <see cref="LocalStorage{TData}"/>: what is kept here belongs to the
/// installation and is the same for everyone connecting, rather than to one browser.
/// <para>
/// Reading never fails. A file that is missing, unreadable or no longer shaped like
/// <typeparamref name="TData"/> answers with the fallback, because a tester who cannot start a test
/// run over a broken settings file is worse off than one running with the defaults; the failure is
/// reported to the log instead.
/// </para>
/// </remarks>
/// <typeparam name="TData">Shape of the data kept in the file.</typeparam>
/// <param name="filePath">Full path of the file the data is kept in.</param>
/// <param name="logger">Log the read and write failures are reported to.</param>
/// <param name="fallbackFactory">Produces the data to answer with when the file cannot be read.</param>
public class JsonFileStorage<TData>(
    string filePath,
    IAppLogger logger,
    Func<TData> fallbackFactory)
    where TData : class
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    /// <summary>
    /// Reads what the file holds.
    /// </summary>
    /// <returns>The data read, or the fallback when there is none to be had.</returns>
    public async Task<TData> ReadAsync()
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return fallbackFactory();
            }

            await using FileStream stream = File.OpenRead(filePath);

            return await JsonSerializer.DeserializeAsync<TData>(stream, SerializerOptions)
                ?? fallbackFactory();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.Warning(
                $"The settings file could not be read, so the default settings are used ({filePath}).",
                exception.ToString());

            return fallbackFactory();
        }
    }

    /// <summary>
    /// Writes the data to the file, creating the folder it lives in when needed.
    /// </summary>
    /// <param name="data">Data to write.</param>
    /// <returns>
    /// <see langword="true"/> when the data reached the file. A caller that told the user their
    /// settings are saved has to be able to take it back.
    /// </returns>
    public async Task<bool> WriteAsync(TData data)
    {
        try
        {
            var directoryPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            await using FileStream stream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(stream, data, SerializerOptions);

            return true;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.Error(
                $"The settings could not be saved, so they are lost when the application stops ({filePath}).",
                exception.ToString());

            return false;
        }
    }
}
