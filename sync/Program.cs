using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Newtonsoft.Json.Linq;

string[] scopes = [SheetsService.Scope.SpreadsheetsReadonly];
string applicationName = "Google Sheets API .NET Quickstart";
const string CSV_PATH = "../shared/table";

UserCredential credential;
using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
{
    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
        GoogleClientSecrets.FromStream(stream).Secrets,
        scopes,
        "user",
        CancellationToken.None,
        new FileDataStore("token.json", true)).Result;
}

var service = new SheetsService(new BaseClientService.Initializer()
{
    HttpClientInitializer = credential,
    ApplicationName = applicationName,
});

var sheetMap = JObject.Parse(File.ReadAllText("sheet.json"));

Directory.Delete(CSV_PATH, true);

foreach (var entry in sheetMap)
{
    var key = entry.Key;
    var spreadsheetId = entry.Value!.ToString();
    var spreadsheet = service.Spreadsheets.Get(spreadsheetId).Execute();

    var outputDir = Path.Combine(CSV_PATH, key);
    Directory.CreateDirectory(outputDir);

    foreach (var sheet in spreadsheet.Sheets)
    {
        var sheetName = sheet.Properties.Title;
        var values = service.Spreadsheets.Values.Get(spreadsheetId, sheetName).Execute().Values;
        if (values == null || values.Count == 0) continue;

        var csv = ToCsv(values);
        if (csv == null) continue;

        File.WriteAllText(Path.Combine(outputDir, $"{key}_{sheetName}.csv"), csv);
        Console.WriteLine($"저장: {key}/{key}_{sheetName}.csv");
    }
}

static string? ToCsv(IList<IList<object>> data)
{
    var rowIndex = 0;

    // 헤더 행 찾기 및 유효 컬럼 인덱스 수집
    var headers = new Dictionary<int, string>();
    for (int i = 0; i < data[rowIndex].Count; i++)
    {
        var str = data[rowIndex][i]?.ToString();
        if (string.IsNullOrWhiteSpace(str) || str.StartsWith('#')) continue;
        headers.Add(i, str);
    }

    if (headers.Count == 0) return null;

    rowIndex++;

    var sb = new System.Text.StringBuilder();
    sb.AppendLine(string.Join(",", headers.Values.Select(EscapeCsv)));

    while (TrySkipRow(data, ref rowIndex))
    {
        var row = data[rowIndex];
        var fields = headers.Keys.Select(col =>
        {
            if (col >= row.Count) return "";
            return EscapeCsv(row[col]?.ToString() ?? "");
        });
        sb.AppendLine(string.Join(",", fields));
        rowIndex++;
    }

    return sb.ToString();
}

static string EscapeCsv(string value)
{
    if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        return $"\"{value.Replace("\"", "\"\"")}\"";
    return value;
}

static bool TrySkipRow(IList<IList<object>> data, ref int index)
{
    for (int i = index; i < data.Count; i++)
    {
        var row = data[i];
        var str = row.Count > 0 ? row[0]?.ToString() : null;
        if (string.IsNullOrWhiteSpace(str) || str.StartsWith('#')) continue;
        index = i;
        return true;
    }
    return false;
}
