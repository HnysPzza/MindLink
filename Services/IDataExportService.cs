namespace M1ndLink.Services;

public interface IDataExportService
{
    Task<string> ExportAsync();
}
