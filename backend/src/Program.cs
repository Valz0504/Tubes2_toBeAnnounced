using System.Diagnostics;
using System.Text.Json;
using backend.Parser;
using backend.Search;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("Frontend");

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

app.MapPost("/api/analyze", async (AnalyzeRequest request) =>
{
    var validationError = ValidateRequest(request);
    if (validationError != null)
    {
        return Results.BadRequest(new ErrorResponse(validationError));
    }

    var inputHandler = new HtmlInputHandler();
    var htmlParser = new HtmlParser();
    var cssParser = new CssParser();

    try
    {
        var scrapeTimer = Stopwatch.StartNew();
        var isUrlInput = Is(request.InputMode, "url");
        var html = await inputHandler.GetHtmlContentAsync(
            isUrlInput ? "1" : "2",
            isUrlInput ? request.Url!.Trim() : request.Html!
        );
        scrapeTimer.Stop();

        var parseTimer = Stopwatch.StartNew();
        var domTree = htmlParser.Parse(html);
        var selectorQueries = cssParser.Parse(request.Selector!);
        parseTimer.Stop();

        var searcher = new HtmlNodeWithSelector
        {
            Root = domTree,
            Sq = selectorQueries
        };

        var searchTimer = Stopwatch.StartNew();
        var searchResult = searcher.StartSearching(Is(request.Algorithm, "bfs"));
        searchTimer.Stop();

        var mapper = new DomResponseMapper();
        var dom = mapper.ToDto(domTree);
        var allSolutions = searchResult.SolutionNodes;
        var limitedSolutions = Is(request.ResultMode, "top")
            ? allSolutions.Take(Math.Max(0, request.Limit ?? allSolutions.Count)).ToList()
            : allSolutions;

        var affectedGroups = searchResult.AffectedNodes
            .Select((nodes, index) => new AffectedGroupDto(
                index,
                nodes.Where(mapper.HasNodeId).Select(mapper.GetNodeId).Distinct().ToList()
            ))
            .ToList();

        var affectedNodeIds = affectedGroups
            .SelectMany(group => group.NodeIds)
            .Distinct()
            .ToHashSet();
        var solutionNodeIds = allSolutions
            .Where(mapper.HasNodeId)
            .Select(mapper.GetNodeId)
            .ToHashSet();

        var stats = new AnalysisStatsDto(
            mapper.NodeCount,
            mapper.MaxDepth,
            searchResult.TraversalLog.Count,
            allSolutions.Count,
            limitedSolutions.Count,
            scrapeTimer.Elapsed.TotalMilliseconds,
            parseTimer.Elapsed.TotalMilliseconds,
            searchTimer.Elapsed.TotalMilliseconds
        );

        var traversalLog = searchResult.TraversalLog
            .Where(mapper.HasNodeId)
            .Select((node, index) => mapper.ToTraversalDto(
                node,
                index + 1,
                solutionNodeIds.Contains(mapper.GetNodeId(node)),
                affectedNodeIds.Contains(mapper.GetNodeId(node))
            ))
            .ToList();

        var results = limitedSolutions
            .Where(mapper.HasNodeId)
            .Select((node, index) => mapper.ToResultDto(node, index + 1))
            .ToList();

        var parsedSelector = selectorQueries.Select(ToSelectorDto).ToList();
        var logFileName = await SaveTraversalLogAsync(
            app.Environment.ContentRootPath,
            new TraversalLogFileDto(
                DateTimeOffset.UtcNow,
                request.InputMode ?? "",
                request.Algorithm ?? "",
                request.Selector ?? "",
                stats,
                traversalLog,
                results,
                affectedGroups,
                parsedSelector
            )
        );

        var response = new AnalyzeResponse(
            dom,
            stats,
            traversalLog,
            results,
            affectedGroups,
            parsedSelector,
            logFileName
        );

        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new ErrorResponse(ex.Message));
    }
});

if (Directory.Exists(Path.Combine(app.Environment.ContentRootPath, "wwwroot")))
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
    app.MapFallbackToFile("index.html");
}

app.Run();

static string? ValidateRequest(AnalyzeRequest request)
{
    if (!Is(request.InputMode, "url") && !Is(request.InputMode, "html"))
    {
        return "Mode input harus bernilai 'url' atau 'html'.";
    }

    if (Is(request.InputMode, "url"))
    {
        if (string.IsNullOrWhiteSpace(request.Url))
        {
            return "URL tidak boleh kosong.";
        }

        if (!Uri.TryCreate(request.Url.Trim(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return "URL harus absolut dan memakai skema http atau https.";
        }
    }
    else if (string.IsNullOrWhiteSpace(request.Html))
    {
        return "Teks HTML tidak boleh kosong.";
    }

    if (string.IsNullOrWhiteSpace(request.Selector))
    {
        return "CSS selector tidak boleh kosong.";
    }

    if (!Is(request.Algorithm, "bfs") && !Is(request.Algorithm, "dfs"))
    {
        return "Algoritma harus bernilai 'bfs' atau 'dfs'.";
    }

    if (!Is(request.ResultMode, "all") && !Is(request.ResultMode, "top"))
    {
        return "Mode hasil harus bernilai 'all' atau 'top'.";
    }

    if (Is(request.ResultMode, "top") &&
        (request.Limit == null || request.Limit < 0))
    {
        return "Jumlah top-N harus bernilai minimal 0.";
    }

    return null;
}

static bool Is(string? value, string expected) =>
    string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);

static SelectorQueryDto ToSelectorDto(SelectorQuery query) => new(
    query.TagName,
    query.Id,
    query.Classes,
    query.AttributeName,
    query.AttributeValue,
    query.RelationToPrevious.ToString()
);

static async Task<string> SaveTraversalLogAsync(string contentRootPath, TraversalLogFileDto log)
{
    var logsDirectory = Path.Combine(contentRootPath, "logs");
    Directory.CreateDirectory(logsDirectory);

    var fileName = $"traversal-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}.json";
    var filePath = Path.Combine(logsDirectory, fileName);
    var options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(log, options));
    return fileName;
}

public sealed record AnalyzeRequest(
    string? InputMode,
    string? Url,
    string? Html,
    string? Algorithm,
    string? Selector,
    string? ResultMode,
    int? Limit
);

public sealed record AnalyzeResponse(
    DomNodeDto Dom,
    AnalysisStatsDto Stats,
    List<TraversalLogDto> TraversalLog,
    List<ResultNodeDto> Results,
    List<AffectedGroupDto> AffectedGroups,
    List<SelectorQueryDto> ParsedSelector,
    string LogFileName
);

public sealed record AnalysisStatsDto(
    int NodeCount,
    int MaxDepth,
    int VisitedCount,
    int TotalMatches,
    int DisplayedMatches,
    double ScrapeMilliseconds,
    double ParseMilliseconds,
    double SearchMilliseconds
);

public sealed record DomNodeDto(
    int NodeId,
    string TagName,
    string Label,
    string Id,
    List<string> Classes,
    Dictionary<string, string> Attributes,
    int Depth,
    int ChildCount,
    string Path,
    List<DomNodeDto> Children
);

public sealed record TraversalLogDto(
    int Order,
    int NodeId,
    string Label,
    string TagName,
    int Depth,
    string Path,
    bool IsSolution,
    bool IsAffected
);

public sealed record ResultNodeDto(
    int Rank,
    int NodeId,
    string Label,
    string TagName,
    string Id,
    List<string> Classes,
    Dictionary<string, string> Attributes,
    int Depth,
    string Path
);

public sealed record AffectedGroupDto(int Level, List<int> NodeIds);

public sealed record SelectorQueryDto(
    string TagName,
    string Id,
    List<string> Classes,
    string AttributeName,
    string AttributeValue,
    string RelationToPrevious
);

public sealed record ErrorResponse(string Error);

public sealed record TraversalLogFileDto(
    DateTimeOffset CreatedAt,
    string InputMode,
    string Algorithm,
    string Selector,
    AnalysisStatsDto Stats,
    List<TraversalLogDto> TraversalLog,
    List<ResultNodeDto> Results,
    List<AffectedGroupDto> AffectedGroups,
    List<SelectorQueryDto> ParsedSelector
);

public sealed class DomResponseMapper
{
    private readonly Dictionary<HtmlNode, int> _nodeIds = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<HtmlNode, string> _paths = new(ReferenceEqualityComparer.Instance);

    public int NodeCount { get; private set; }
    public int MaxDepth { get; private set; }

    public DomNodeDto ToDto(HtmlNode root)
    {
        NodeCount = 0;
        MaxDepth = 0;
        _nodeIds.Clear();
        _paths.Clear();
        return Build(root, "document", 1);
    }

    public bool HasNodeId(HtmlNode node) => _nodeIds.ContainsKey(node);

    public int GetNodeId(HtmlNode node) => _nodeIds[node];

    public TraversalLogDto ToTraversalDto(HtmlNode node, int order, bool isSolution, bool isAffected)
    {
        return new TraversalLogDto(
            order,
            GetNodeId(node),
            GetLabel(node),
            node.TagName,
            node.Depth,
            _paths[node],
            isSolution,
            isAffected
        );
    }

    public ResultNodeDto ToResultDto(HtmlNode node, int rank)
    {
        return new ResultNodeDto(
            rank,
            GetNodeId(node),
            GetLabel(node),
            node.TagName,
            node.Id,
            node.Classes.ToList(),
            new Dictionary<string, string>(node.Attributes),
            node.Depth,
            _paths[node]
        );
    }

    private DomNodeDto Build(HtmlNode node, string parentPath, int siblingIndex)
    {
        NodeCount++;
        MaxDepth = Math.Max(MaxDepth, node.Depth);

        var nodeId = NodeCount;
        _nodeIds[node] = nodeId;

        var label = GetLabel(node);
        var path = node.Depth == 0
            ? label
            : $"{parentPath} > {label}:nth-child({siblingIndex})";
        _paths[node] = path;

        var children = node.Children
            .Select((child, index) => Build(child, path, index + 1))
            .ToList();

        return new DomNodeDto(
            nodeId,
            node.TagName,
            label,
            node.Id,
            node.Classes.ToList(),
            new Dictionary<string, string>(node.Attributes),
            node.Depth,
            node.Children.Count,
            path,
            children
        );
    }

    private static string GetLabel(HtmlNode node)
    {
        var label = node.TagName;
        if (!string.IsNullOrWhiteSpace(node.Id))
        {
            label += $"#{node.Id}";
        }

        if (node.Classes.Count > 0)
        {
            label += "." + string.Join(".", node.Classes);
        }

        return label;
    }
}
